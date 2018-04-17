// Mapping64.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"


typedef void(*void_f)(void);



bool isPatchUp;

// ASM imports
extern "C"
{
	void asm_hook_connect();
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

	// push [ba2]
	trampoline[len] = 0x68;
	memcpy(&trampoline[len + 1], &ba2, sizeof(DWORD));

	// mov rax, [ba1]
	trampoline[len + 5] = 0x48;
	trampoline[len + 6] = 0xC7;
	trampoline[len + 7] = 0xC0;
	memcpy(&trampoline[len + 8], &ba1, sizeof(DWORD));

	// mov [rsp + 4], rax (dest, source)
	trampoline[len + 12] = 0x48;
	trampoline[len + 13] = 0x89;
	trampoline[len + 14] = 0x44;
	trampoline[len + 15] = 0x24;
	trampoline[len + 16] = 0x04;

	// ret
	trampoline[len + 17] = 0xC3;


	const int trampSize = (len + 18);
	// Allocates memory for trampoline, Original bytes + Jump
	*funcTramp = VirtualAlloc(NULL, trampSize, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

	// Build tramp
	memcpy(*funcTramp, trampoline, trampSize);
}

void WINAPI Entry()
{
	HookFunc(L"ws2_32.dll", "connect", (LPVOID*)asm_hook_connect, &connectTramp, 10);
}