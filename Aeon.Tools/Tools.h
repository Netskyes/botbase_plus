#pragma once

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
#define _CRT_SECURE_NO_WARNINGS			// Suppress warnings

#include <windows.h>
#include <TlHelp32.h>
#include <tchar.h>
#include <stdlib.h>


// Exports
#ifdef INJECTION_EXPORTS
#define BOTAPI __declspec(dllexport)
#else
#define BOTAPI __declspec(dllimport)
#endif

extern "C" BOTAPI bool Inject(const char* procName, const char* dll);