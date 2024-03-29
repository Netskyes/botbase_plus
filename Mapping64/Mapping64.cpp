// Mapping64.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include <asmjit.h>
using namespace asmjit;

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

	LPVOID connectTramp;
	LPVOID WSAConnectTramp;

	void PatchSocketAddress(SOCKET s, const struct sockaddr *name, int namelen);
	void SendSockPatch(SOCKET s);
};

void __stdcall LogMe(std::string text)
{
	std::ofstream file;
	file.open("C:/Users/alex/Desktop/Debug.txt", std::ios_base::app);
	file << text;
	file.close();
}

WSAPROTOCOL_INFO GetSockInfo(SOCKET s, bool &result)
{
	WSAPROTOCOL_INFO info;
	int len = sizeof(WSAPROTOCOL_INFO);
	int res = getsockopt(s, SOL_SOCKET, SO_PROTOCOL_INFO, (char*)&info, &len);

	result = (res == 0);
	return info;
}

void SendSockPatch(SOCKET s)
{
	if (!isPatchUp)
		return;

	sockaddr_in* server = (struct sockaddr_in*)sockAddrBackup;

	ULONG addr = htonl(server->sin_addr.s_addr);
	USHORT port = server->sin_port;


	bool result;
	WSAPROTOCOL_INFO sockInfo = GetSockInfo(s, result);

	std::stringstream ss;
	ss << "Socket: " << s << " Protocol: " << sockInfo.iProtocol << std::endl;
	LogMe(ss.str());

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

void PatchSocketAddress(SOCKET s, const struct sockaddr *name, int namelen)
{
	sockAddrNew = new sockaddr;
	*sockAddrNew = *sockAddrBackup;

	sockaddr_in* temp = (sockaddr_in*)sockAddrNew;
	std::string hostname = inet_ntoa(temp->sin_addr);

	isPatchUp = false;
	if (hostname != "127.0.0.1")
	{
		((sockaddr_in*)sockAddrNew)->sin_port = htons(8089);
		((sockaddr_in*)sockAddrNew)->sin_addr.s_addr = inet_addr("127.0.0.1");

		bool result;
		WSAPROTOCOL_INFO sockInfo = GetSockInfo(s, result);

		std::stringstream ss;
		ss << "Socket patch -> Socket: " << s << " Protocol: " << sockInfo.iProtocol << std::endl;
		LogMe(ss.str());

		isPatchUp = true;
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

//typedef void(__stdcall *HookConnectFunc)(void);
//HookConnectFunc connectHook;

//void MakeHookConnectFunc()
//{
//	JitRuntime rt;
//	CodeHolder code;
//	code.init(rt.getCodeInfo());
//
//	X86Assembler a(&code);
//	a.mov(x86::rdx, (DWORD64)sockAddrBackup);
//	a.push(x86::rax);
//	a.push(x86::r9);
//	a.push(x86::r10);
//	a.push(x86::r11);
//	a.pushfq();
//	a.push(x86::r8);
//	a.push(x86::rdx);
//	a.push(x86::rcx);
//
//	a.call((DWORD64)hook_connect);
//
//	a.pop(x86::rcx);
//	a.add(x86::rsp, 8);
//	a.mov((DWORD64)sockAddrNew, x86::rdx);
//
//	a.pop(x86::r8);
//	a.popfq();
//	a.pop(x86::r11);
//	a.pop(x86::r10);
//	a.pop(x86::r9);
//	a.pop(x86::rax);
//
//	a.pop(retAddr);
//	a.call((DWORD64)connectTramp);
//	a.push((DWORD64)retAddr);
//
//	a.push(x86::rcx);
//	a.push(x86::rdx);
//	a.push(x86::r8);
//	a.push(x86::r9);
//	a.push(x86::r10);
//	a.pushfq();
//
//	a.mov(x86::rax, sockBackup);
//	a.sub(x86::rsp, 8);
//	a.call((DWORD64)send_sock_patch);
//	a.add(x86::rsp, 8);
//
//	a.popfq();
//	a.pop(x86::r10);
//	a.pop(x86::r9);
//	a.pop(x86::r8);
//	a.pop(x86::rdx);
//	a.pop(x86::rcx);
//
//	a.emit(a.mov(x86::rax, 0));
//	a.ret();
//	
//	Error err = rt.add(&connectHook, &code);
//	if (err)
//	{
//		std::cout << "Asm failed -> Connect Hook" << std::endl;
//		return;
//	}
//}

void WINAPI Entry()
{
	std::cout << "Hooked!" << std::endl;

	HookFunc(L"ws2_32.dll", "connect", (LPVOID*)asm_hook_connect, &connectTramp, 10);
	//HookFunc(L"ws2_32.dll", "WSAConnect", (LPVOID*)asm_hook_wsaconnect, &WSAConnectTramp, 7);
}