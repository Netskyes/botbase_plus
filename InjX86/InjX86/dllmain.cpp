#include "stdafx.h"

void WINAPI Entry();

void CreateEntryThread()
{
	AllocConsole();
	freopen("CONIN$", "r", stdin);
	freopen("CONOUT$", "w", stdout);

	CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)&Entry, NULL, NULL, NULL);
}

//void UnloadSelf()
//{
//	FreeConsole();
//	FreeLibraryAndExitThread(moduleHandle, NULL);
//}

bool ErasePEHeader(HMODULE hModule)
{
	PIMAGE_DOS_HEADER pDosHeader = (PIMAGE_DOS_HEADER)hModule;
	PIMAGE_NT_HEADERS pNTHeader = (PIMAGE_NT_HEADERS)((DWORD)pDosHeader + (DWORD)pDosHeader->e_lfanew);

	if (pNTHeader->Signature != IMAGE_NT_SIGNATURE)
		return 0;

	if (pNTHeader->FileHeader.SizeOfOptionalHeader)
	{
		DWORD Protect;
		WORD Size = pNTHeader->FileHeader.SizeOfOptionalHeader;
		VirtualProtect((void*)hModule, Size, PAGE_EXECUTE_READWRITE, &Protect);
		RtlZeroMemory((void*)hModule, Size);
		VirtualProtect((void*)hModule, Size, Protect, &Protect);
	}

	return 1;
}

extern "C" __declspec(dllexport) int WINAPI DllMain(HINSTANCE hInstDll, DWORD fdwReason, LPVOID lpvReserved)
{
	DisableThreadLibraryCalls(hInstDll);
	ErasePEHeader(hInstDll);
	CreateEntryThread();

	return 1;
}