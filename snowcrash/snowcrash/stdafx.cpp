// stdafx.cpp : source file that includes just the standard includes
// snowcrash.pch will be the pre-compiled header
// stdafx.obj will contain the pre-compiled type information

#include "stdafx.h"

#if (_ATL_VER < 0x0700)
#include <atlimpl.cpp>
#endif //(_ATL_VER < 0x0700)

#ifdef ECHO
void dprintf(char *format, ...)
{
	LPSTR pBuffer = new TCHAR[8192];

	if (pBuffer != NULL)
	{
		va_list args;
		DWORD dwWrote;

		va_start(args, format);
		_vsnprintf(pBuffer, 8192, format, args);
		va_end(args);

		WriteConsole(GetStdHandle(STD_OUTPUT_HANDLE), pBuffer, (DWORD)strlen(pBuffer), &dwWrote, NULL);

		delete [] pBuffer;
	}
}
#else
#define dprintf
#endif