// Injection.cpp : Defines the exported functions for the DLL application.
//
#include "stdafx.h"
#define BOTAPI __declspec(dllexport)


HANDLE GetProcessByName(const wchar_t* procName)
{
	DWORD pid = 0;
	PROCESSENTRY32 entry;
	entry.dwSize = sizeof(PROCESSENTRY32);

	HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, NULL);

	if (Process32First(snapshot, &entry))
	{
		do
		{
			if (_tcscmp(entry.szExeFile, procName) == 0)
			{
				pid = entry.th32ProcessID;
				break;
			}

		} while (Process32Next(snapshot, &entry));
	}

	return (pid != 0) ? OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid) : NULL;
}

wchar_t* charToWChar(const char* text)
{
	const size_t size = strlen(text) + 1;
	wchar_t* wText = new wchar_t[size];
	mbstowcs(wText, text, size);

	return wText;
}

extern "C"
{
	BOOL BOTAPI Inject(const char* procName, const char* dll)
	{
		HANDLE procHandle = GetProcessByName(charToWChar(procName));

		if (!procHandle)
		{
			return FALSE;
		}


		DWORD LLAddress = (DWORD)GetProcAddress(GetModuleHandle(L"kernel32.dll"), "LoadLibraryA");
		PVOID memory = VirtualAllocEx(procHandle, NULL, strlen(dll) + 1, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

		WriteProcessMemory(procHandle, memory, dll, strlen(dll) + 1, NULL);


		if (!CreateRemoteThread(procHandle, NULL, NULL, (LPTHREAD_START_ROUTINE)LLAddress, memory, NULL, NULL))
		{
			return FALSE;
		}


		CloseHandle(procHandle);
		VirtualFreeEx(procHandle, memory, 0, MEM_FREE | MEM_COMMIT);

		return 1;
	}
}
	
