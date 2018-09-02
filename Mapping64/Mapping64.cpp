// Mapping64.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"


typedef void(*void_f)(void);



bool isPatchUp;

// ASM imports
extern "C"
{
	void asm_hook_connect();
	void asm_hook_wsaconnect();
}

// Exporting
extern "C"
{
	SOCKET sockBackup;
	sockaddr* sockAddrNew;
	sockaddr* sockAddrBackup;
	DWORD64 retAddr;

	void hook_connect(SOCKET s, const struct sockaddr *name, int namelen);
	LPVOID connectTramp;
	LPVOID WSAConnectTramp;

	void send_sock_patch(SOCKET s);
};


WSAPROTOCOL_INFO GetSockInfo(SOCKET s, bool &result)
{
	WSAPROTOCOL_INFO info;
	int len = sizeof(WSAPROTOCOL_INFO);
	int res = getsockopt(s, SOL_SOCKET, SO_PROTOCOL_INFO, (char*)&info, &len);

	result = (res == 0);
	return info;
}

void send_sock_patch(SOCKET s)
{
	if (!isPatchUp)
		return;

	std::cout << "Socket: " << s << std::endl;
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

void hook_connect(SOCKET s, const struct sockaddr *name, int namelen)
{
	sockBackup = s;
	sockAddrNew = new sockaddr;
	*sockAddrNew = *name;

	sockaddr_in* temp = (sockaddr_in*)sockAddrNew;
	std::cout << "Socket: " << s << " " << inet_ntoa(temp->sin_addr) << ":" << temp->sin_port  << " Len: " << namelen << std::endl;
	isPatchUp = false;

	std::string hostname = inet_ntoa(temp->sin_addr);

	if (hostname != "127.0.0.1")
	{
		((sockaddr_in*)sockAddrNew)->sin_port = htons(8089);
		((sockaddr_in*)sockAddrNew)->sin_addr.s_addr = inet_addr("127.0.0.1");

		isPatchUp = true;
	}
	else
	{
		std::cout << "Points to 127.0.0.1" << std::endl;
	}
}


void HookFunc(LPCWSTR moduleName, LPCSTR funcName, LPVOID hookFunc, LPVOID* funcTramp, const int len)
{
	BYTE jump[] = { 0xE9, 0x00, 0x00, 0x00, 0x00 };
	BYTE trampoline[64];

	// Get hooking function base address
	DWORD64 funcAddr = (DWORD64)GetProcAddress(GetModuleHandle(moduleName), funcName);

	DWORD prev;
	// Change permissions to be able to access page
	VirtualProtect((LPVOID)funcAddr, len, PAGE_EXECUTE_READWRITE, &prev);

	// Copy original bytes
	memcpy(trampoline, (LPVOID)funcAddr, len);


	// Calculate jump offset to our function
	DWORD64 proxy = ((DWORD64)hookFunc - funcAddr) - 5;

	// Build jump
	memcpy(&jump[1], &proxy, 4);
	memcpy((LPVOID)funcAddr, jump, len);

	// Fill in any unused space with NOP codes
	if (len > 5)
	{
		for (int i = 0; i < (len - 5); i++)
		{
			memset((LPVOID)((funcAddr + 5) + i), 0x90, 1);
		}
	}

	VirtualProtect((LPVOID)funcAddr, len, prev, &prev);

	DWORD ba1 = (DWORD)((funcAddr + 5) >> 32);
	DWORD ba2 = (DWORD)((funcAddr + 5));

	/*
	0000021E841C000A | 68 45 B2 8C DF | push FFFFFFFFDF8CB245 |
	0000021E841C000F | 48 C7 C0 FF 7F 00 00 | mov rax, 7FFF |
	0000021E841C0016 | 48 89 44 24 04 | mov qword ptr ss : [rsp + 4], rax |
	0000021E841C001B | C3 | ret |
	*/

	// sub rsp, 8
	trampoline[len] = 0x48;
	trampoline[len + 1] = 0x83;
	trampoline[len + 2] = 0xEC;
	trampoline[len + 3] = 0x08;

	// push [ba2]
	trampoline[len + 4] = 0x68;
	memcpy(&trampoline[len + 5], &ba2, sizeof(DWORD));


	// mov rsi, 0x0
	trampoline[len + 9] = 0x48;
	trampoline[len + 10] = 0xC7;
	trampoline[len + 11] = 0xC6;
	memcpy(&trampoline[len + 12], &ba1, sizeof(DWORD));

	// mov [rsp + 4], rax (dest, source)
	trampoline[len + 16] = 0x48;
	trampoline[len + 17] = 0x89;
	trampoline[len + 18] = 0x74;
	trampoline[len + 19] = 0x24;
	trampoline[len + 20] = 0x04;

	trampoline[len + 21] = 0x5E;


	trampoline[len + 22] = 0x48;
	trampoline[len + 23] = 0x83;
	trampoline[len + 24] = 0xC4;
	trampoline[len + 25] = 0x08;

	trampoline[len + 26] = 0x56;

	// ret
	trampoline[len + 27] = 0xC3;


	const int trampSize = (len + 28);
	// Allocates memory for trampoline, Original bytes + Jump
	*funcTramp = VirtualAlloc(NULL, trampSize, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

	// Build tramp
	memcpy(*funcTramp, trampoline, trampSize);
}


HMODULE hMono = GetModuleHandle(L"mono.dll");

DWORD GetMonoFunction(const char* funcname)
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

void WINAPI Entry()
{
	/*MonoInject();*/
	HookFunc(L"ws2_32.dll", "connect", (LPVOID*)asm_hook_connect, &connectTramp, 10);
	HookFunc(L"ws2_32.dll", "WSAConnect", (LPVOID*)asm_hook_wsaconnect, &WSAConnectTramp, 7);
}