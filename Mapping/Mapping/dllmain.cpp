#include "stdafx.h"

void WINAPI Entry();
HMODULE hModuleMain;

void CreateEntryThread()
{
	AllocConsole();
	freopen("CONIN$", "r", stdin);
	freopen("CONOUT$", "w", stdout);

	CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)&Entry, NULL, NULL, NULL);
}

void UnloadSelf()
{
	FreeConsole();
	FreeLibraryAndExitThread(hModuleMain, NULL);
}

extern "C" __declspec(dllexport) int WINAPI DllMain(HINSTANCE hInstDll, DWORD fdwReason, LPVOID lpvReserved)
{
	switch (fdwReason)
	{
		case DLL_PROCESS_ATTACH:
		{
			hModuleMain = (HMODULE)hInstDll;
			CreateEntryThread();
			break;
		}
	}

	return 1;
}