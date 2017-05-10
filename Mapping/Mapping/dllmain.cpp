#include "stdafx.h"

DWORD WINAPI Entry();

extern "C" __declspec(dllexport) bool WINAPI DllMain(HINSTANCE hInstDll, DWORD fdwReason, LPVOID lpvReserved)
{
	switch (fdwReason)
	{
		case DLL_PROCESS_ATTACH:
		{
			AllocConsole();
			freopen("CONIN$", "r", stdin);
			freopen("CONOUT$", "w", stdout);
			
			CreateThread(NULL, NULL, (LPTHREAD_START_ROUTINE)&Entry, NULL, NULL, NULL);
			break;
		}
	}

	return true;
}

DWORD GetMainThreadId(DWORD procId)
{
	THREADENTRY32 entry;
	entry.dwSize = sizeof(THREADENTRY32);

	HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, procId);
	if (snapshot == INVALID_HANDLE_VALUE)
	{
		return 0;
	}

	DWORD result = 0;

	for (BOOL success = Thread32First(snapshot, &entry); !result && success && GetLastError() != ERROR_NO_MORE_FILES; success = Thread32Next(snapshot, &entry))
	{
		if (entry.th32OwnerProcessID == procId)
		{
			result = entry.th32ThreadID;
		}
	}

	CloseHandle(snapshot);
	return result;
}

void EnableDebugPriv()
{
	HANDLE hToken;
	LUID luid;
	TOKEN_PRIVILEGES tokenPriv;

	OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken);
	LookupPrivilegeValue(NULL, SE_DEBUG_NAME, &luid);
	tokenPriv.PrivilegeCount = 1;
	tokenPriv.Privileges[0].Luid = luid;
	tokenPriv.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
	AdjustTokenPrivileges(hToken, FALSE, &tokenPriv, sizeof(tokenPriv), NULL, NULL);
	CloseHandle(hToken);
}

WSAPROTOCOL_INFO GetSockInfo(SOCKET s, bool &result)
{
	WSAPROTOCOL_INFO info;
	int len = sizeof(WSAPROTOCOL_INFO);
	int res = getsockopt(s, SOL_SOCKET, SO_PROTOCOL_INFO, (char*)&info, &len);

	result = (res == 0);
	return info;
}

void __stdcall LogMe(std::string text)
{
	std::ofstream file;
	file.open("C:/Users/SumYungHo/Desktop/Debug.txt", std::ios_base::app);
	file << text;
	file.close();
}



DWORD retAddrBackup;
struct sockaddr* sockAddrNew;
struct sockaddr* sockAddrBackup;

LPVOID connectTramp;
LPVOID WSAConnectTramp;
LPVOID bindTramp;

std::mutex patchLock;


void __stdcall PatchSockAddr()
{
	patchLock.lock();

	struct sockaddr* addrNew = new sockaddr;
	sockAddrNew = addrNew;

	struct sockaddr tempBackup = *sockAddrBackup;
	*sockAddrNew = tempBackup;
	
	((sockaddr_in*)sockAddrNew)->sin_port = htons(8085);
	((sockaddr_in*)sockAddrNew)->sin_addr.s_addr = inet_addr("127.0.0.1");

	patchLock.unlock();
}

void __stdcall sendRelay(SOCKET s)
{
	struct sockaddr_in* server = (struct sockaddr_in*)sockAddrBackup;

	ULONG addr = htonl(server->sin_addr.s_addr);
	USHORT port = server->sin_port;
	
	char frame[sizeof(USHORT) + sizeof(ULONG)];

	memcpy(frame, &port, sizeof(USHORT));
	memcpy(&frame[2], &addr, sizeof(ULONG));


	int result = send(s, (const char*)frame, sizeof(frame), 0);
}


void __stdcall mConnect(SOCKET s, const struct sockaddr *name)
{
	sockaddr_in* addr = ((sockaddr_in*)sockAddrBackup);

	std::stringstream ss;
	ss << "Connect: " << inet_ntoa(addr->sin_addr) << ":" << addr->sin_port << " Socket: " << s << " Family: " << addr->sin_family << std::endl;

	LogMe(ss.str());
}

__declspec(naked) int hookConnect()
{
	__asm
	{
		pushad;
		pushfd;

		push[esp + 0x2C];
		pop sockAddrBackup;

		call PatchSockAddr;

		popfd;
		popad;


		push[esp + 0xC];
		push sockAddrNew;
		push[esp + 0xC];

		call connectTramp;

		// Log
		push[esp + 0x8];
		push[esp + 0x8];
		call mConnect;

		pop retAddrBackup;
		
		call sendRelay;
		
		add esp, 8;
		push retAddrBackup;

		ret;
	}
}

void __stdcall mWSAConnect(SOCKET s, const struct sockaddr *name)
{
	sockaddr_in* addr = (sockaddr_in*)name;

	std::stringstream ss;
	ss << "WSAConnect: " << inet_ntoa(addr->sin_addr) << ":" << addr->sin_port << " Socket: " << s << std::endl;

	LogMe(ss.str());

	bool result;
	WSAPROTOCOL_INFO info = GetSockInfo(s, result);
}

__declspec(naked) int hookWSAConnect()
{
	__asm
	{
		pushad;
		pushfd;

		push[esp + 0x2C];
		pop sockAddrBackup;

		call PatchSockAddr;

		popfd;
		popad;


		push[esp + 0x1C];
		push[esp + 0x1C];
		push[esp + 0x1C];
		push[esp + 0x1C];
		push[esp + 0x1C];
		push sockAddrNew;
		push[esp + 0x1C];
		
		call WSAConnectTramp;

		// Log
		push[esp + 0x8];
		push[esp + 0x8];
		call mWSAConnect;

		pop retAddrBackup;
		call sendRelay;

		add esp, 24;
		push retAddrBackup;

		ret;
	}
}

void __stdcall mBind(SOCKET s, const struct sockaddr *name, int namelen)
{
	sockaddr_in* addr = ((sockaddr_in*)name);

	std::stringstream ss;
	ss << "Bind: " << inet_ntoa(addr->sin_addr) << ":" << addr->sin_port << " Socket: " << s << " Family: " << addr->sin_family << std::endl;

	LogMe(ss.str());
}

__declspec(naked) int hookBind()
{
	__asm
	{
		pushad;
		pushfd;

		push[esp + 0x2C];
		pop sockAddrBackup;

		call PatchSockAddr;

		popfd;
		popad;


		push[esp + 0xC];
		push sockAddrNew;
		push[esp + 0xC];

		call bindTramp;

		pop retAddrBackup;
		call mBind;
		
		push retAddrBackup;

		ret;
	}
}


/*
bool result;
WSAPROTOCOL_INFO info = GetSockInfo(s, result);

if (result)
{
std::cout << "Socket (" << s << ") -> Type: " << info.iSocketType << " Protocol: " << info.iProtocol << std::endl;
}
*/


#pragma region Hooks

LPVOID socketTramp;
LPVOID WSARecvTramp;
LPVOID WSASendTramp;
LPVOID acceptTramp;
LPVOID listenTramp;


void __stdcall mWSARecv(SOCKET s, LPWSABUF lpBuffers)
{
	std::stringstream ss;
	ss << "WSARecv: " << lpBuffers->buf << " Socket: " << s << " Len: " << lpBuffers->len << std::endl;
	
	LogMe(ss.str());
}

__declspec(naked) int hookWSARecv()
{
	__asm
	{
		pushad;
		pushfd;

		push[esp + 0x2C];
		push[esp + 0x2C];
		call mWSARecv;

		popfd;
		popad;

		jmp WSARecvTramp;
	}
}


void __stdcall mWSASend(SOCKET s, LPWSABUF lpBuffers)
{
	std::stringstream ss;
	ss << "WSASend: " << lpBuffers->buf << " Socket: " << s << " Len: " << lpBuffers->len << std::endl;

	LogMe(ss.str());
}

__declspec(naked) int hookWSASend()
{
	__asm
	{
		pushad;
		pushfd;

		push[esp + 0x2C];
		push[esp + 0x2C];
		call mWSASend;

		popfd;
		popad;

		jmp WSASendTramp;
	}
}


void __stdcall mAccept(SOCKET s, struct sockaddr *name)
{
	sockaddr_in* addr = ((sockaddr_in*)name);

	std::stringstream ss;
	ss << "Accept: " << inet_ntoa(addr->sin_addr) << ":" << addr->sin_port << " Socket: " << s << " Family: " << addr->sin_family << std::endl;

	LogMe(ss.str());
}

__declspec(naked) int hookAccept()
{
	__asm
	{
		pushad;
		pushfd;

		push[esp + 0x2C];
		push[esp + 0x2C];
		call mAccept;

		popfd;
		popad;

		jmp acceptTramp;
	}
}


void __stdcall mSocket(int af, int type, int protocol)
{
	std::stringstream ss;
	ss << "Socket: " << af << " Type: " << type << " Protocol: " << protocol << std::endl;

	LogMe(ss.str());
}

__declspec(naked) int hookSocket()
{
	__asm
	{
		pushad;
		pushfd;

		push[esp + 0x30];
		push[esp + 0x30];
		push[esp + 0x30];
		call mSocket;

		popfd;
		popad;

		jmp socketTramp;
	}
}


void __stdcall mListen(SOCKET s, int backlog)
{
	std::stringstream ss;
	ss << "Listen: " << s << " Backlog: " << backlog << std::endl;

	LogMe(ss.str());
}

__declspec(naked) int hookListen()
{
	__asm
	{
		pushad;
		pushfd;

		push[esp + 0x2C];
		push[esp + 0x2C];
		call mListen;

		popfd;
		popad;

		jmp listenTramp;
	}
}

#pragma endregion


// Jump -> Proxy -> Trampoline -> Original + 5
BOOL HookFunc(LPCWSTR moduleName, LPCSTR funcName, LPVOID hookFunc, LPVOID* funcTramp, const int len)
{
	BYTE jump[] = { 0xE9, 0x00, 0x00, 0x00, 0x00 };
	BYTE trampoline[60];

	*funcTramp = VirtualAlloc(NULL, (len + 5), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

	DWORD baseAddr = (DWORD)GetProcAddress(GetModuleHandle(moduleName), funcName);
	

	DWORD prev;
	VirtualProtect((LPVOID)baseAddr, len, PAGE_EXECUTE_READWRITE, &prev);


	DWORD retJmp = (baseAddr - (DWORD)*funcTramp) - len;

	memcpy(trampoline, (LPVOID)baseAddr, len);
	trampoline[len] = 0xE9;
	memcpy(&trampoline[len + 1], &retJmp, 4);
	memcpy(*funcTramp, trampoline, len + 5);


	DWORD proxy = ((DWORD)hookFunc - baseAddr) - len;
	memcpy(&jump[1], &proxy, 4);
	memcpy((LPVOID)baseAddr, jump, len);


	VirtualProtect((LPVOID)baseAddr, len, prev, &prev);
	FlushInstructionCache(GetCurrentProcess(), NULL, NULL);

	return true;
}


void HookUp()
{
	std::cout << "Hooks enabled!" << std::endl;

	HookFunc(L"ws2_32.dll", "connect", (LPVOID*)hookConnect, &connectTramp, 5);
	HookFunc(L"ws2_32.dll", "WSAConnect", (LPVOID*)hookWSAConnect, &WSAConnectTramp, 5);
	//HookFunc(L"ws2_32.dll", "listen", (LPVOID*)hookListen, &listenTramp, 5);
	//HookFunc(L"ws2_32.dll", "recv", (LPVOID*)hookRecv, &recvTramp, 5);
	//HookFunc(L"ws2_32.dll", "send", (LPVOID*)hookSend, &sendTramp, 5);
	HookFunc(L"ws2_32.dll", "WSARecv", (LPVOID*)hookWSARecv, &WSARecvTramp, 5);
	HookFunc(L"ws2_32.dll", "WSASend", (LPVOID*)hookWSASend, &WSASendTramp, 5);
	HookFunc(L"ws2_32.dll", "socket", (LPVOID*)hookSocket, &socketTramp, 5);
	//HookFunc(L"ws2_32.dll", "accept", (LPVOID*)hookAccept, &acceptTramp, 5);
	//HookFunc(L"ws2_32.dll", "bind", (LPVOID*)hookBind, &bindTramp, 5);
}


void Test()
{
	//HookFunc(L"ws2_32.dll", "recv", (LPVOID*)hookRecv, &recvTramp, 5);
	

	/*
	int offset = 4100;
	uintptr_t base = reinterpret_cast<uintptr_t>(GetModuleHandle(L"ygopro_vs.exe"));
	uintptr_t* func = (uintptr_t *)(base + (offset)); //4096
	*/
}

DWORD WINAPI Entry()
{
	HookUp();

	/*
	while (true)
	{
		std::string input;
		std::cout << "> ";
		std::cin >> input;

		if (input == "hook")
		{
			HookUp();
		}
		else if (input == "unhook")
		{
			Unhook();
		}
		else if (input == "test")
		{
			Test();
		}
		else
		{
			Unhook();
			FreeConsole();
			break;
		}
	}
	*/

	return 0;
}