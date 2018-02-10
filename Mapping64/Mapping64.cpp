// Mapping64.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"

#pragma region Deprecated

BOOL HookFunc64(LPCWSTR moduleName, LPCSTR funcName, LPVOID hookFunc, LPVOID* funcTramp, const int len)
{
	BYTE jump[] = { 0xE9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
	BYTE trampoline[60];

	// Original bytes + jump (Trampoline)
	*funcTramp = VirtualAlloc(NULL, (len + 9), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

	// Get base address of the function we're hooking
	DWORD64 baseAddr = (DWORD64)GetProcAddress(GetModuleHandle(moduleName), funcName);

	// Set permissions to length of bytes that we're to override
	// with our jump from base address.
	DWORD prev;
	VirtualProtect((LPVOID)baseAddr, len, PAGE_EXECUTE_READWRITE, &prev);


	DWORD64 retJmpAddr = (baseAddr - (DWORD64)*funcTramp) - len;

	memcpy(trampoline, (LPVOID)baseAddr, len);
	trampoline[len] = 0xE9;
	memcpy(&trampoline[len + 1], &retJmpAddr, 8);

	// Create trampoline
	memcpy(*funcTramp, trampoline, len + 9);


	// Build jump
	DWORD64 proxy = ((DWORD64)hookFunc - baseAddr) - 9;
	memcpy(&jump[1], &proxy, 8);

	// Override target with jump
	memcpy((LPVOID)baseAddr, jump, 9);

	if (len > 9)
	{
		for (int i = 0; i < (len - 9); i++)
		{
			memset((LPVOID)((baseAddr + 9) + i), 0x90, 1);
		}
	}


	VirtualProtect((LPVOID)baseAddr, len, prev, &prev);
	FlushInstructionCache(GetCurrentProcess(), NULL, NULL);

	return true;
}

#pragma endregion

typedef void(*void_f)(void);

// ASM exports
extern "C"
{
	void asm_hook_connect();
}

// Exporting
extern "C"
{
	void hook_connect();
	LPVOID connectTramp;
};

void hook_connect()
{
	std::cout << "OK" << std::endl;
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