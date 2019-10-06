#include "stdafx.h"

void UnloadSelf();
DWORD HookFunc(LPCWSTR moduleName, LPCSTR funcName, LPVOID hookFunc, LPVOID* funcTramp, const int len);

DWORD retAddrBackup;
struct sockaddr* sockAddrNew;
struct sockaddr* sockAddrBackup;
bool isPatchUp = false;

LPVOID sendTramp;
LPVOID recvTramp;
LPVOID WSARecvTramp;
LPVOID WSASendTramp;
LPVOID WSAAcceptTramp;
LPVOID WSAConnectTramp;
LPVOID socketTramp;
LPVOID listenTramp;
LPVOID acceptTramp;
LPVOID connectTramp;
LPVOID bindTramp;
LPVOID setThreadInfoTramp;

SOCKET socketLocal;


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
	file.open("C:/Users/alex/Desktop/Debug.txt", std::ios_base::app);
	file << text;
	file.close();
}


void __stdcall PatchSockAddr()
{
	sockAddrNew = new sockaddr;
	*sockAddrNew = *sockAddrBackup;

	isPatchUp = false;

	sockaddr_in* temp = (sockaddr_in*)sockAddrNew;
	std::string hostname = inet_ntoa(temp->sin_addr);

	if (hostname != "127.0.0.1")
	{
		((sockaddr_in*)sockAddrNew)->sin_port = htons(8089);
		((sockaddr_in*)sockAddrNew)->sin_addr.s_addr = inet_addr("127.0.0.1");

		isPatchUp = true;
	}
}

void __stdcall SendPatch(SOCKET s)
{
	if (!isPatchUp)
		return;

	struct sockaddr_in* server = (struct sockaddr_in*)sockAddrBackup;

	ULONG addr = htonl(server->sin_addr.s_addr);
	USHORT port = server->sin_port;

	bool result;
	WSAPROTOCOL_INFO sockInfo = GetSockInfo(s, result);

	int resultSend;
	if (sockInfo.iProtocol == 6)
	{
		char frame[6];

		memcpy(frame, &port, sizeof(USHORT));
		memcpy(&frame[2], &addr, sizeof(ULONG));

		resultSend = send(s, (const char*)frame, sizeof(frame), 0);
	}
	else if (sockInfo.iProtocol == 17)
	{
		char frame[8];
		USHORT opcode = htons(1337);

		memcpy(frame, &opcode, sizeof(USHORT));
		memcpy(&frame[2], &port, sizeof(USHORT));
		memcpy(&frame[4], &addr, sizeof(ULONG));

		resultSend = send(s, (const char*)frame, sizeof(frame), 0);
	}
	else 
	{
		std::cout << "Unknown protocol: " << sockInfo.iProtocol << std::endl;
	}
}

void __stdcall mConnect(SOCKET s, const struct sockaddr *addr)
{
	sockaddr_in* addrNew = ((sockaddr_in*)sockAddrNew);
	sockaddr_in* addrBackup = ((sockaddr_in*)sockAddrBackup);

	bool result;
	WSAPROTOCOL_INFO sockInfo = GetSockInfo(s, result);

	std::stringstream ss;
	ss << "Connect: " << inet_ntoa(addrNew->sin_addr) << ":" << ntohs(addrNew->sin_port);
	ss << " ~~~~> " << inet_ntoa(addrBackup->sin_addr) << ":" << ntohs(addrBackup->sin_port) << " (Socket: " << s;
	ss << " Type: " << sockInfo.iSocketType << " Protocol: " << sockInfo.iProtocol << ")" << std::endl;

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

		// Log
		push[esp + 0x2C];
		push[esp + 0x2C];
		call mConnect;

		popfd;
		popad;


		push[esp + 0xC];
		push sockAddrNew;
		push[esp + 0xC];

		call connectTramp;


		pop retAddrBackup;
		call SendPatch;

		add esp, 8;
		mov eax, 0x00; // Success

		push retAddrBackup;

		ret;
	}

	delete sockAddrBackup;
	delete sockAddrNew;
}

//__declspec(naked) int hookConnectLog()
//{
//	__asm
//	{
//		pushad;
//		pushfd;
//
//		push[esp + 0x2C];
//		push[esp + 0x2C];
//		call mConnectLog;
//
//		popfd;
//		popad;
//
//		jmp connectTramp;
//	}
//}

void __stdcall mWSAConnect(SOCKET s, const struct sockaddr *name)
{
	sockaddr_in* addrNew = ((sockaddr_in*)sockAddrNew);
	sockaddr_in* addrBackup = ((sockaddr_in*)sockAddrBackup);

	bool result;
	WSAPROTOCOL_INFO sockInfo = GetSockInfo(s, result);

	std::stringstream ss;
	ss << "WSAConnect: " << inet_ntoa(addrNew->sin_addr) << ":" << ntohs(addrNew->sin_port);
	ss << " ~~~~> " << inet_ntoa(addrBackup->sin_addr) << ":" << ntohs(addrBackup->sin_port) << " Socket: " << s;
	ss << " (Type: " << sockInfo.iSocketType << " Protocol: " << sockInfo.iProtocol << ")" << std::endl;

	LogMe(ss.str());
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

		// Log
		push[esp + 0x2C];
		push[esp + 0x2C];
		call mWSAConnect;

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


		pop retAddrBackup;
		call SendPatch;

		add esp, 24;
		mov eax, 0x00; // Success

		push retAddrBackup;

		ret;
	}

	delete sockAddrBackup;
	delete sockAddrNew;
}

void __stdcall mBind(SOCKET s, const struct sockaddr *addr)
{
	sockaddr_in* addrNew = ((sockaddr_in*)sockAddrNew);
	sockaddr_in* addrBackup = ((sockaddr_in*)sockAddrBackup);

	bool result;
	WSAPROTOCOL_INFO sockInfo = GetSockInfo(s, result);

	std::stringstream ss;
	ss << "Bind: " << inet_ntoa(addrNew->sin_addr) << ":" << ntohs(addrNew->sin_port);
	ss << " -> " << inet_ntoa(addrBackup->sin_addr) << ":" << ntohs(addrBackup->sin_port) << " Socket: " << s;
	ss << " (Type: " << sockInfo.iSocketType << " Protocol: " << sockInfo.iProtocol << ")" << std::endl;

	LogMe(ss.str());
}

void __stdcall mBindLog(SOCKET s, const struct sockaddr *addr)
{
	sockaddr_in* temp = ((sockaddr_in*)addr);

	bool result;
	WSAPROTOCOL_INFO sockInfo = GetSockInfo(s, result);

	std::stringstream ss;
	ss << "Bind: " << inet_ntoa(temp->sin_addr) << ":" << ntohs(temp->sin_port) << " Socket: " << s;
	ss << " (Type: " << sockInfo.iSocketType << " Protocol: " << sockInfo.iProtocol << ")" << std::endl;

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

		push[esp + 0x2C];
		push[esp + 0x2C];
		call mBind;

		popfd;
		popad;


		push[esp + 0xC];
		push sockAddrNew;
		push[esp + 0xC];

		call bindTramp;


		pop retAddrBackup;

		add esp, 12;
		push retAddrBackup;

		ret;
	}

	delete sockAddrBackup;
}

__declspec(naked) int hookBindLog()
{
	__asm
	{
		pushad;
		pushfd;

		push[esp + 0x2C];
		push[esp + 0x2C];
		call mBindLog;

		popfd;
		popad;

		jmp bindTramp;
	}
}

void __stdcall mWSARecv(SOCKET s, LPWSABUF lpBuffers)
{
	bool result;
	WSAPROTOCOL_INFO sockInfo = GetSockInfo(s, result);

	std::stringstream ss;
	ss << "WSARecv: Socket: " << s << " P: " << sockInfo.iProtocol << " T: " << sockInfo.iSocketType << " AF: " << sockInfo.iAddressFamily << " Len: " << lpBuffers->len << std::endl;
	
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
	bool result;
	WSAPROTOCOL_INFO sockInfo = GetSockInfo(s, result);

	std::stringstream ss;
	ss << "WSASend: Socket: " << s << " P: " << sockInfo.iProtocol << " T: " << sockInfo.iSocketType << " AF: " << sockInfo.iAddressFamily << " Len: " << lpBuffers->len << std::endl;

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

void __stdcall mSend(SOCKET s, const char *buf, int len)
{
	std::stringstream ss;
	ss << "Send: " << buf << " Socket: " << s << " Len: " << len << std::endl;

	LogMe(ss.str());
}

__declspec(naked) int hookSend()
{
	__asm
	{
		pushad;
		pushfd;

		push[esp + 0x30];
		push[esp + 0x30];
		push[esp + 0x30];
		call mSend;

		popfd;
		popad;

		jmp sendTramp;
	}
}

void __stdcall mRecv(SOCKET s, char *buf, int len)
{
	std::stringstream ss;
	ss << "Recv: " << buf << " Socket: " << s << " Len: " << len << std::endl;

	LogMe(ss.str());
}

__declspec(naked) int hookRecv()
{
	__asm
	{
		pushad;
		pushfd;

		push[esp + 0x30];
		push[esp + 0x30];
		push[esp + 0x30];

		call mRecv;

		popfd;
		popad;

		jmp recvTramp;
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




SOCKET connectLocal()
{
	sockaddr_in server;
	server.sin_family = AF_INET;
	server.sin_addr.s_addr = inet_addr("127.0.0.1");
	server.sin_port = htons(8089);
	
	SOCKET sock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

	int result = connect(sock, (const sockaddr*)&server, sizeof(server));
	if (result != SOCKET_ERROR)
	{
		std::cout << "Connected locally!" << std::endl;
	}
	else 
	{
		std::cout << "An error occured connecting: " << WSAGetLastError() << std::endl;
	}
	return sock;
}


void _stdcall mSocketLogSocket(SOCKET s)
{
	std::stringstream ss;
	ss << "<<<<< Socket: " << s << " >>>>>>" << std::endl;
	LogMe(ss.str());
}

__declspec(naked) int hookSocket()
{
	__asm
	{
		push[esp + 0x0C];
		push[esp + 0x0C];
		push[esp + 0x0C];

		call socketTramp;

		pushad;
		pushfd;

		push eax;
		call mSocketLogSocket;

		popfd;
		popad;
		
		pop retAddrBackup;
		add esp, 12;
		push retAddrBackup;

		ret;
	}
}

void __stdcall mListen(SOCKET s, int backlog)
{
	struct sockaddr_in sin;
	int len = sizeof(sin);

	getsockname(s, (struct sockaddr*)&sin, &len);


	std::stringstream ss;
	ss << "Listen: " << inet_ntoa(sin.sin_addr) << ":" << ntohs(sin.sin_port) << " Socket: " << s << " Backlog: " << backlog << std::endl;

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

void __stdcall mNtSetInformationThread() 
{
	std::cout << "In function!!" << std::endl;
}

__declspec(naked) int hookSetInfoThread()
{
	__asm 
	{
		pushad;
		pushfd;

		call mNtSetInformationThread;

		popfd;
		popad;

		jmp setThreadInfoTramp;
	}
}

void __stdcall mWSAAccept(SOCKET s, struct sockaddr * addr)
{
	std::cout << "WSAAccept --> S: " << s << std::endl;
}

__declspec(naked) int hookWSAAccept()
{
	__asm
	{
		pushad;
		pushfd;

		push[esp + 0x2C];
		push[esp + 0x2C];
		call mWSAAccept;

		popfd;
		popad;

		jmp WSAAcceptTramp;
	}
}


void HookUp()
{
	std::cout << "Hooks enabled!" << std::endl;

	//HookFunc(L"ntdll.dll", "NtSetInformationThread", (LPVOID*)hookSetInfoThread, &setThreadInfoTramp, 5);

	DWORD fa1 = HookFunc(L"ws2_32.dll", "connect", (LPVOID*)hookConnect, &connectTramp, 5);
	std::cout << "Connect: " << std::hex << fa1 << std::endl;

	DWORD fa2 = HookFunc(L"ws2_32.dll", "WSAConnect", (LPVOID*)hookWSAConnect, &WSAConnectTramp, 5);
	std::cout << "WSAConnect: " << std::hex << fa2 << std::endl;

	//DWORD fa3 = HookFunc(L"mswsock.dll", "ConnectEx", (LPVOID*)hookWSAConnect, &WSAConnectTramp, 5);
	//std::cout << "WSAConnect: " << std::hex << fa3 << std::endl;

	//HookFunc(L"ws2_32.dll", "connect", (LPVOID*)hookConnectLog, &connectTramp, 5);

	//HookFunc(L"ws2_32.dll", "bind", (LPVOID*)hookBind, &bindTramp, 5);
	//HookFunc(L"ws2_32.dll", "bind", (LPVOID*)hookBindLog, &bindTramp, 5);
	//HookFunc(L"ws2_32.dll", "listen", (LPVOID*)hookListen, &listenTramp, 5);
	//HookFunc(L"ws2_32.dll", "recv", (LPVOID*)hookRecv, &recvTramp, 5);
	//HookFunc(L"ws2_32.dll", "send", (LPVOID*)hookSend, &sendTramp, 5);
	//HookFunc(L"ws2_32.dll", "WSARecv", (LPVOID*)hookWSARecv, &WSARecvTramp, 5);
	//HookFunc(L"ws2_32.dll", "WSASend", (LPVOID*)hookWSASend, &WSASendTramp, 5);
	//HookFunc(L"ws2_32.dll", "socket", (LPVOID*)hookSocket, &socketTramp, 5);
	//HookFunc(L"ws2_32.dll", "accept", (LPVOID*)hookAccept, &acceptTramp, 5);
	//HookFunc(L"ntdll.dll", "NtSetInformationThread", (LPVOID*)hookSetInfoThread, &setThreadInfoTramp, 5);

	//HookFunc(L"ws2_32.dll", "WSAAccept", (LPVOID*)hookWSAAccept, &WSAAcceptTramp, 5);
}


void Test()
{
	int offset = 4100;
	uintptr_t base = reinterpret_cast<uintptr_t>(GetModuleHandle(L"ygopro_vs.exe"));
	uintptr_t* func = (uintptr_t *)(base + (offset)); //4096


	int* health = (int*)(base + 0x02AEC44 + 4096);

	int value = *health;

	std::cout << "Current health: " << value << std::endl;
}

void Debug()
{
	DWORD hModule = (DWORD)GetModuleHandle(L"archeage.exe");
	if (hModule == 0)
		return;

	BYTE patch[2] = { 0xB0, 0x01 };
	int len = sizeof(patch);
	DWORD patchAddr = (DWORD)(hModule + 0x1580 + 0x4F);
	
	DWORD prev;
	VirtualProtect((BYTE*)patchAddr, len, PAGE_EXECUTE_READWRITE, &prev);
	memcpy((BYTE*)patchAddr, patch, len);
	VirtualProtect((BYTE*)patchAddr, len, prev, &prev);

	std::cout << "Patching done!" << std::endl;
}

void RemoveMutex(LPCWSTR mutexName)
{
	HANDLE mHandle = OpenMutex(MUTEX_ALL_ACCESS, TRUE, mutexName);
	if (mHandle != NULL)
	{
		CloseHandle(mHandle);
	}
}


void WINAPI Entry()
{
	//Debug();
	HookUp();
	//UnloadSelf();
}


// Jump -> Proxy -> Trampoline -> Original + 5
DWORD HookFunc(LPCWSTR moduleName, LPCSTR funcName, LPVOID hookFunc, LPVOID* funcTramp, const int len)
{
	BYTE jump[] = { 0xE9, 0x00, 0x00, 0x00, 0x00 };
	BYTE trampoline[60];

	// Original bytes + jump
	*funcTramp = VirtualAlloc(NULL, (len + 5), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

	DWORD baseAddr = (DWORD)GetProcAddress(GetModuleHandle(moduleName), funcName);


	DWORD prev;
	VirtualProtect((LPVOID)baseAddr, len, PAGE_EXECUTE_READWRITE, &prev);


	DWORD retJmpAddr = (baseAddr - (DWORD)*funcTramp) - len;

	memcpy(trampoline, (LPVOID)baseAddr, len);
	trampoline[len] = 0xE9;
	memcpy(&trampoline[len + 1], &retJmpAddr, 4);
	memcpy(*funcTramp, trampoline, len + 5);


	DWORD proxy = ((DWORD)hookFunc - baseAddr) - 5;
	memcpy(&jump[1], &proxy, 4);
	memcpy((LPVOID)baseAddr, jump, len);

	if (len > 5)
	{
		for (int i = 0; i < (len - 5); i++)
		{
			memset((LPVOID)((baseAddr + 5) + i), 0x90, 1);
		}
	}

	VirtualProtect((LPVOID)baseAddr, len, prev, &prev);
	FlushInstructionCache(GetCurrentProcess(), NULL, NULL);

	return baseAddr;
}