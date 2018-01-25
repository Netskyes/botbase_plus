#pragma once

#ifdef INJECTION_EXPORTS
#define BOTAPI __declspec(dllexport)
#else
#define BOTAPI __declspec(dllimport)
#endif


extern "C" BOTAPI bool Inject(const char* procName, const char* dll);