#include "stdafx.h"

DWORD WINAPI Entry();

extern "C" __declspec(dllexport) int WINAPI DllMain(HINSTANCE hInstDll, DWORD fdwReason, LPVOID lpvReserved)
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

	return 1;
}