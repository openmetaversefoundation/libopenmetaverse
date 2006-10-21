// stdafx.cpp : source file that includes just the standard includes
// snowflake.pch will be the pre-compiled header
// stdafx.obj will contain the pre-compiled type information

#include "stdafx.h"

#ifdef ECHO
FILE *fpLog = NULL;
#endif

#ifdef ECHODEBUG
void dprintf(TCHAR *format, ...)
{
	LPTSTR pBuffer = new TCHAR[8192];

	if (pBuffer != NULL)
	{
		va_list args;
		DWORD dwWrote;

		va_start(args, format);
		_vsntprintf(pBuffer, 8192, format, args);
		va_end(args);

		WriteConsole(GetStdHandle(STD_OUTPUT_HANDLE), pBuffer, (DWORD)_tcslen(pBuffer), &dwWrote, NULL);

		if (fpLog)
		{
			fwrite(pBuffer, 1, _tcslen(pBuffer), fpLog);
			fflush(fpLog);
		}

		delete [] pBuffer;
	}
}
#else
#define dprintf
#endif

#ifdef ECHONORMAL
void myprintf(TCHAR *format, ...)
{
	LPTSTR pBuffer = new TCHAR[8192];

	if (pBuffer != NULL)
	{
		va_list args;
		DWORD dwWrote;

		va_start(args, format);
		_vsntprintf(pBuffer, 8192, format, args);
		va_end(args);

		WriteConsole(GetStdHandle(STD_OUTPUT_HANDLE), pBuffer, (DWORD)_tcslen(pBuffer), &dwWrote, NULL);

		if (fpLog)
		{
			fwrite(pBuffer, 1, _tcslen(pBuffer), fpLog);
			fflush(fpLog);
		}

		delete [] pBuffer;
	}
}
#else
#define myprintf
#endif
