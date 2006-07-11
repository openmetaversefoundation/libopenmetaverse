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

void pktsave(char *buf, int len, int extra)
{
	if (len > 8)
	{
		char filename[256];
		WORD wSeq;

		memcpy(&wSeq, &buf[2], sizeof(wSeq));

		if ((unsigned char)buf[4] == 0xFF)
		{
			if ((unsigned char)buf[5] == 0xFF)
			{
				// Low Frequency Message
				sprintf(filename, "c:\\csl\\packets\\l%02x%02x-%05d.%d.bin", (unsigned char)buf[6], (unsigned char)buf[7], htons(wSeq), extra);
			}
			else
			{
				// Medium Frequency Message
				sprintf(filename, "c:\\csl\\packets\\m00%02x-%05d.%d.bin", (unsigned char)buf[5], htons(wSeq), extra);
			}
		}
		else
		{
			// High Frequency Message
			sprintf(filename, "c:\\csl\\packets\\h00%02x-%05d.%d.bin", (unsigned char)buf[4], htons(wSeq), extra);
		}

		FILE *fp = fopen(filename, "wb");

		if (fp)
		{
			fwrite(buf, 1, len, fp);
			fflush(fp);
			fclose(fp);
		}
	}
}
