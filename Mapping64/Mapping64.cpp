// Mapping64.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"

extern "C"
{
	void hook_connect();
};


LPVOID connectTramp;

int hookConnect()
{
	hook_connect();
	return 1;
}


BOOL HookFunc(LPCWSTR moduleName, LPCSTR funcName, LPVOID hookFunc, LPVOID* funcTramp, const int len)
{
	BYTE jump[] = { 0xE9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
	BYTE trampoline[60];

	// Original bytes + jump
	*funcTramp = VirtualAlloc(NULL, (len + 9), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

	DWORD64 baseAddr = (DWORD64)GetProcAddress(GetModuleHandle(moduleName), funcName);


	DWORD prev;
	VirtualProtect((LPVOID)baseAddr, len, PAGE_EXECUTE_READWRITE, &prev);


	DWORD64 retJmpAddr = (baseAddr - (DWORD64)*funcTramp) - len;

	memcpy(trampoline, (LPVOID)baseAddr, len);
	trampoline[len] = 0xE9;
	memcpy(&trampoline[len + 1], &retJmpAddr, 4);
	memcpy(*funcTramp, trampoline, len + 9);


	DWORD64 proxy = ((DWORD64)hookFunc - baseAddr) - 9;
	memcpy(&jump[1], &proxy, 4);
	memcpy((LPVOID)baseAddr, jump, len);

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

void WINAPI Entry()
{
	HookFunc(L"ws2_32.dll", "connect", (LPVOID*)hookConnect, &connectTramp, 5);
}