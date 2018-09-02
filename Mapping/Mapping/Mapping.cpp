#include "stdafx.h"

void UnloadSelf();


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

/*
bool result;
WSAPROTOCOL_INFO info = GetSockInfo(s, result);

if (result)
{
std::cout << "Socket (" << s << ") -> Type: " << info.iSocketType << " Protocol: " << info.iProtocol << std::endl;
}
*/



DWORD retAddrBackup;
struct sockaddr* sockAddrNew;
struct sockaddr* sockAddrBackup;
bool isPatchUp = false;

LPVOID connectTramp;
LPVOID WSAConnectTramp;
LPVOID bindTramp;


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

	if (result && sockInfo.iProtocol == 17)
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
		char frame[6];

		memcpy(frame, &port, sizeof(USHORT));
		memcpy(&frame[2], &addr, sizeof(ULONG));

		resultSend = send(s, (const char*)frame, sizeof(frame), 0);
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
	ss << " ~~~~> Relay: " << inet_ntoa(addrBackup->sin_addr) << ":" << ntohs(addrBackup->sin_port) << " Socket: " << s;
	ss << " (Type: " << sockInfo.iSocketType << " Protocol: " << sockInfo.iProtocol << ")" << std::endl;

	LogMe(ss.str());
}

void __stdcall mConnectLog(SOCKET s, const struct sockaddr * addr)
{
	bool result;
	WSAPROTOCOL_INFO sockInfo = GetSockInfo(s, result);

	sockaddr_in* temp = ((sockaddr_in*)addr);

	if (result)
	{
		std::stringstream ss;
		ss << "Connect: " << inet_ntoa(temp->sin_addr) << ":" << ntohs(temp->sin_port);
		ss << " (Type: " << sockInfo.iSocketType << " Protocol: " << sockInfo.iProtocol << ")" << std::endl;

		LogMe(ss.str());
	}
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

__declspec(naked) int hookConnectLog()
{
	__asm
	{
		pushad;
		pushfd;

		push[esp + 0x2C];
		push[esp + 0x2C];
		call mConnectLog;

		popfd;
		popad;

		jmp connectTramp;
	}
}


void __stdcall mWSAConnect(SOCKET s, const struct sockaddr *name)
{
	sockaddr_in* addrNew = ((sockaddr_in*)sockAddrNew);
	sockaddr_in* addrBackup = ((sockaddr_in*)sockAddrBackup);

	bool result;
	WSAPROTOCOL_INFO sockInfo = GetSockInfo(s, result);

	std::stringstream ss;
	ss << "WSAConnect: " << inet_ntoa(addrNew->sin_addr) << ":" << ntohs(addrNew->sin_port);
	ss << " ~~~~> Relay: " << inet_ntoa(addrBackup->sin_addr) << ":" << ntohs(addrBackup->sin_port) << " Socket: " << s;
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
		push[esp + 0x1C];
		push[esp + 0x1C];
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


#pragma region Hooks

LPVOID sendTramp;
LPVOID recvTramp;
LPVOID WSARecvTramp;
LPVOID WSASendTramp;
LPVOID socketTramp;
LPVOID listenTramp;
LPVOID acceptTramp;


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

#pragma endregion


// Jump -> Proxy -> Trampoline -> Original + 5
BOOL HookFunc(LPCWSTR moduleName, LPCSTR funcName, LPVOID hookFunc, LPVOID* funcTramp, const int len)
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

	return true;
}


void HookUp()
{
	//std::cout << "Hooks enabled!" << std::endl;

	HookFunc(L"ws2_32.dll", "connect", (LPVOID*)hookConnect, &connectTramp, 5);
	HookFunc(L"ws2_32.dll", "WSAConnect", (LPVOID*)hookWSAConnect, &WSAConnectTramp, 5);
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


HMODULE hMono = GetModuleHandle(L"mono.dll");

DWORD GetMonoFunction(char* funcname) 
{
	return (DWORD)GetProcAddress(hMono, funcname);
}

void MonoInject()
{
	//Attach
	typedef MonoThread* (__cdecl* mono_thread_attach_t)(MonoDomain* mDomain);
	mono_thread_attach_t mono_thread_attach = (mono_thread_attach_t)GetMonoFunction("mono_thread_attach");

	//Class
	typedef MonoClass* (__cdecl* mono_class_from_name_t)(MonoImage* image, const char* name_space, const char* name);
	typedef MonoMethod* (__cdecl* mono_class_get_method_from_name_t)(MonoClass* mclass, const char* name, int param_count);
	mono_class_from_name_t mono_class_from_name = (mono_class_from_name_t)GetMonoFunction("mono_class_from_name");
	mono_class_get_method_from_name_t mono_class_get_method_from_name = (mono_class_get_method_from_name_t)GetMonoFunction("mono_class_get_method_from_name");

	//Code execution
	typedef MonoObject* (__cdecl* mono_runtime_invoke_t)(MonoMethod* method, void* obj, void** params, MonoObject** exc);
	mono_runtime_invoke_t mono_runtime_invoke = (mono_runtime_invoke_t)GetMonoFunction("mono_runtime_invoke");

	//Assembly
	typedef MonoAssembly* (__cdecl* mono_assembly_open_t)(MonoDomain* mDomain, const char* filepath);
	typedef MonoImage* (__cdecl* mono_assembly_get_image_t)(MonoAssembly *assembly);
	mono_assembly_open_t mono_assembly_open_ = (mono_assembly_open_t)GetMonoFunction("mono_domain_assembly_open");
	mono_assembly_get_image_t mono_assembly_get_image_ = (mono_assembly_get_image_t)GetMonoFunction("mono_assembly_get_image");

	//Domain
	typedef MonoDomain* (__cdecl* mono_root_domain_get_t)();
	typedef MonoDomain* (__cdecl* mono_domain_get_t)();
	mono_root_domain_get_t mono_root_domain_get = (mono_root_domain_get_t)GetMonoFunction("mono_get_root_domain");
	mono_domain_get_t mono_domain_getnormal = (mono_domain_get_t)GetMonoFunction("mono_domain_get");


	mono_thread_attach(mono_root_domain_get());
	//Now that we're attached we get the domain we are in.
	MonoDomain* domain = mono_domain_getnormal();
	//Opening a custom assembly in the domain.
	MonoAssembly* domainassembly = mono_assembly_open_(domain, "G:\\SoftwareDev\\C#\\BotRelay\\NetMapping\\bin\\Debug\\NetMapping.dll");
	//Getting the assemblys Image(Binary image, essentially a file-module).
	MonoImage* Image = mono_assembly_get_image_(domainassembly);
	//Declaring the class inside the custom assembly we're going to use. (Image, NameSpace, ClassName)
	MonoClass* pClass = mono_class_from_name(Image, "UnityGameObject", "Entry");
	//Declaring the method, that attaches our assembly to the game. (Class, MethodName, Parameters)
	MonoMethod* MonoClassMethod = mono_class_get_method_from_name(pClass, "Load", 0);
	//Invoking said method.
	mono_runtime_invoke(MonoClassMethod, NULL, NULL, NULL);
}

void Debug()
{
	// Output some useful information.
}


void WINAPI Entry()
{
	Debug();

	//HookUp();
	//MonoInject();

	// Hook listeners etc.
	HookUp();

	// Unload the DLL
	//UnloadSelf();
}