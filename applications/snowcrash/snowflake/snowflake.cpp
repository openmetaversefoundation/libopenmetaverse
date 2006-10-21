// snowflake.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include ".\snowflake.h"
#include ".\MainFrame.h"
#include ".\Server.h"
#include ".\ServerList.h"
#include ".\Message.h"
#include ".\Block.h"
#include ".\Var.h"
#include ".\Config.h"
#include <tlhelp32.h>
#include <wininet.h>
#include <wincrypt.h>
#include <gl/gl.h>
#include <gl/glu.h>
#include <math.h>
#include <winuser.h>
#include <time.h>
#include ".\keywords.h"

#pragma pack(1)

struct COMMANDVAR
{
	char *lpszVar;
	int nKeywordPos;
	int nType;
	int nTypeLen;
	struct COMMANDVAR *lpNext;
	struct COMMANDVAR *lpPrev;
} typedef COMMANDVARS;

typedef COMMANDVAR * LPCOMMANDVAR;
typedef COMMANDVARS * LPCOMMANDVARS;

struct COMMANDSTRUCT
{
	char *lpszStruct;
	int nKeywordPos;
	int nType;
	BYTE cItems;
	LPCOMMANDVARS vars;
	struct COMMANDSTRUCT *lpNext;
	struct COMMANDSTRUCT *lpPrev;
} typedef COMMANDSTRUCTS;

typedef COMMANDSTRUCT * LPCOMMANDSTRUCT;
typedef COMMANDSTRUCTS * LPCOMMANDSTRUCTS;

typedef struct
{
	char *lpszCmd;
	bool bZerocoded;
	bool bTrusted;
	LPCOMMANDSTRUCTS structs;
} COMMAND;

typedef COMMAND * LPCOMMAND;

#define MAX_COMMANDS_LOW	65536
#define MAX_COMMANDS_MEDIUM	256
#define MAX_COMMANDS_HIGH	256

COMMAND cmds_low[MAX_COMMANDS_LOW];
COMMAND cmds_med[MAX_COMMANDS_MEDIUM];
COMMAND cmds_high[MAX_COMMANDS_HIGH];

typedef struct
{
	LPCTSTR	szCommand;
	PROC	pProc;
} CMDHOOK, *LPCMDHOOK;

CMDHOOK	pCMDHooks[];

#pragma data_seg(".shared")
HHOOK h_hCBTHook = NULL;
#pragma data_seg()
#pragma comment(linker, "/SECTION:.shared,RWS")

HINSTANCE g_hinstDLL = NULL;
HMODULE g_hOpenGL = NULL;
CMainFrame* g_pMainFrm = NULL;
CAppModule _Module;
CMessageLoop g_msgLoop;
CConfig* g_pConfig = NULL;
BOOL g_bAllowSub = TRUE;

CServerList servers;

typedef struct
{
	LPCTSTR	szPatch;
	LPCTSTR	szModule;
	LPCSTR	szImport;
	BOOL	bOrdinal;
	PROC	pOldProc;
	PROC	pNewProc;
} APIHOOK, *LPAPIHOOK;

APIHOOK	pAPIHooks[];

#define MAKEPTR(cast, ptr, add) (cast)((DWORD)(ptr)+(DWORD)(add))

enum APIHOOKS
{
	APIHOOK_GETPROCADDRESS,
	APIHOOK_LOADLIBRARYA,
	APIHOOK_LOADLIBRARYW,
	APIHOOK_LOADLIBRARYEXA,
	APIHOOK_LOADLIBRARYEXW,
	APIHOOK_FREELIBRARYA,
	APIHOOK_DELETEFILEA,
	APIHOOK_WRITEFILEA,
	APIHOOK_LSTRCMPIA,
	APIHOOK_CONNECT,
	APIHOOK_RECV,
	APIHOOK_RECVFROM,
	APIHOOK_SEND,
	APIHOOK_SENDTO,
	APIHOOK_GETHOSTBYNAME,
//	APIHOOK_SETWINDOWLONGA,
//	APIHOOK_SETWINDOWLONGW,
	APIHOOK_INTERNETREADFILE,
	APIHOOK_INTERNETOPENURLA,
	APIHOOK_CPENCRYPT,
	APIHOOK_PEEKMESSAGEA,
	APIHOOK_GLGETERROR,
	APIHOOK_GLBEGIN,
	APIHOOK_GLENABLE,
	APIHOOK_GLISENABLED,
	APIHOOK_GLTRANSLATED,
	APIHOOK_GLTRANSLATEF,
	APIHOOK_GLTEXCOORD2F,
	APIHOOK_GLTEXCOORD2FV,
	APIHOOK_GLVERTEX2F,
	APIHOOK_GLVERTEX3F,
	APIHOOK_GLVERTEX3FV,
	APIHOOK_GLVERTEX4F,
	APIHOOK_GLVERTEX4FV,
	APIHOOK_GLVERTEXPOINTER,
	APIHOOK_GLTEXCOORDPOINTER,
	APIHOOK_GLNORMALPOINTER,
	APIHOOK_GLDRAWELEMENTS,
	APIHOOK_GLDRAWARRAYS,
	APIHOOK_GLDRAWPIXELS,
	APIHOOK_GLTEXIMAGE2D,
	APIHOOK_GLCOLOR3F,
	APIHOOK_GLCOLOR3FV,
	APIHOOK_GLCOLOR4F,
	APIHOOK_GLCOLOR4FV,
	APIHOOK_GLCOLOR4UBV,
	APIHOOK_GLCOLORPOINTER,
	APIHOOK_GLVIEWPORT,
	APIHOOK_GLFLUSH,
	APIHOOK_GLUPERSPECTIVE,
	APIHOOK_GLUQUADRICDRAWSTYLE,
	APIHOOK_GLUTESSVERTEX,
	APIHOOK_NULL
};

bool InstallSLHooks(HWND hwnd);
bool RemoveSLHooks();
void InstallImportHooks();
void RemoveImportHooks();
void SaveImportHooks();

int ZeroDecode(char *src, int srclen, char *dest, int destlen)
{
	int zerolen = 0;

	if (src[0] & MSG_ZEROCODED)
	{
		memcpy(dest, src, 4);
		zerolen += 4;

		for (int i = zerolen; i < srclen; i++)
		{
			if ((unsigned char)src[i] == 0x00)
			{
				for (unsigned char j = 0; j < (unsigned char)src[i+1]; j++)
					dest[zerolen++] = 0x00;

				i++;
			}
			else
				dest[zerolen++] = src[i];
		}
	}
	else
	{
		memcpy(dest, src, srclen);
		zerolen = srclen;
	}

	return zerolen;
}

int ZeroEncode(char *src, int srclen, char *dest, int destlen)
{
	int zerolen = 0;
	unsigned char zerocount = 0;

	if (src[0] & MSG_ZEROCODED)
	{
		memcpy(dest, src, 4);
		zerolen += 4;

		for (int i = zerolen; i < srclen; i++)
		{
			if ((unsigned char)src[i] == 0x00)
			{
				zerocount++;

				if (zerocount == 0)
				{
					dest[zerolen++] = 0x00;
					dest[zerolen++] = 0xff;
					zerocount++;
				}
			}
			else
			{
				if (zerocount)
				{
					dest[zerolen++] = 0x00;
					dest[zerolen++] = zerocount;
					zerocount = 0;
				}

				dest[zerolen++] = src[i];
			}
		}

		if (zerocount)
		{
			dest[zerolen++] = 0x00;
			dest[zerolen++] = zerocount;
		}
	}
	else
	{
		memcpy(dest, src, srclen);
		zerolen = srclen;
	}

	return zerolen;
}

// Convert a "hex string" to an integer by Anders Molin
int httoi(const TCHAR *value)
{
	struct HEXMAP
	{
		TCHAR c;
		int value;
	};

	const int nHexMap = 16;
	
	HEXMAP hmLookup[nHexMap] =
	{
		{'0', 0}, {'1', 1},
		{'2', 2}, {'3', 3},
		{'4', 4}, {'5', 5},
		{'6', 6}, {'7', 7},
		{'8', 8}, {'9', 9},
		{'A', 10}, {'B', 11},
		{'C', 12}, {'D', 13},
		{'E', 14}, {'F', 15}
	};

	TCHAR *mstr = _tcsupr(_tcsdup(value));
	TCHAR *s = mstr;
	int result = 0;
	
	if (*s == '0' && *(s + 1) == 'X')
		s += 2;
	
	bool firsttime = true;
	
	while (*s != '\0')
	{
		bool found = false;

		for (int i = 0; i < nHexMap; i++)
		{
			if (*s == hmLookup[i].c)
			{
				if (!firsttime)
					result <<= 4;
				
				result |= hmLookup[i].value;
				found = true;
				break;
			}
		}
		
		if (!found)
			break;
		
		s++;
		firsttime = false;
	}

	free(mstr);
	
	return result;
}

// Trim beginning, ending, and excess embedded whitespace from a string
char *trim(char *szStr)
{
	char *iBuf, *oBuf;

	if (szStr)
	{
		for (iBuf = oBuf = szStr; *iBuf;)
		{
			while (*iBuf && (isspace(*iBuf)))
				iBuf++;
			
			if (*iBuf && (oBuf != szStr))
				*(oBuf++) = ' ';
			
			while (*iBuf && (!isspace(*iBuf)))
				*(oBuf++) = *(iBuf++);
		}
		
		*oBuf = NULL;
	}

	return(szStr);
}

int get_var_type(TCHAR *lptszType)
{
	int i = 0;

	while (LLTYPES[i])
	{
		if (!_tcscmp(lptszType, LLTYPES[i]))
		{
			//printf("Type: %s\n", LLTYPES[i]);
			return i;
		}

		i++;
	}

	return -1;
}

int get_keyword_pos(TCHAR *lptszKeyword)
{
	int i = 0;

	while (LLKEYWORDS[i])
	{
		if (!_tcscmp(lptszKeyword, LLKEYWORDS[i]))
		{
			//printf("Keyword: %s\n", LLKEYWORDS[i]);
			return i;
		}

		i++;
	}

	dprintf("Unhandled keyword: %s\n", lptszKeyword);
	return -1;
}

// Get message template block deliminator positions
bool get_block_markers(LPBYTE lpBuffer, DWORD &dwStart, DWORD &dwEnd, DWORD &dwChildren)
{
	DWORD dwStartBlock = 0;
	DWORD dwEndBlock = 0;
	DWORD dwDepth = 0;
	
	dwChildren = 0;

	for (DWORD dwPos = dwStart; dwPos <= dwEnd; dwPos++)
	{
		if (lpBuffer[dwPos] == '{')
		{
			dwDepth++;

			if (dwDepth == 1)
				dwStartBlock = dwPos;
			else if (dwDepth == 2 && !dwChildren)
				dwChildren = dwPos;
		}
		
		else if (lpBuffer[dwPos] == '}')
		{
			dwDepth--;

			if (dwDepth == 0 && dwStartBlock)
			{
				dwEndBlock = dwPos;
				dwStart = dwStartBlock;
				dwEnd = dwEndBlock;
				return true;
			}
		}
	}

	return false;
}

// Parse the variables message template block of a struct
bool get_var_blocks(LPCOMMANDSTRUCT lpStruct, LPBYTE lpBuffer, DWORD dwStart, DWORD dwEnd)
{
	DWORD dwVarStart = dwStart;
	DWORD dwVarEnd = dwEnd;
	DWORD dwVarChildren = 0;

	while (get_block_markers(lpBuffer, dwVarStart, dwVarEnd, dwVarChildren))
	{
		char szVarLine[256];
		DWORD dwVarLen = (dwVarChildren ? dwVarChildren - 1 : dwVarEnd - 1) - dwVarStart;

		memcpy(&szVarLine, &lpBuffer[dwVarStart+1], dwVarLen);
		szVarLine[dwVarLen] = '\0';
		trim(szVarLine);
		
		//printf("\t\t%s\n", szVarLine);

		char *lpszVar = strtok(szVarLine, " ");
		char *lpszType = strtok(NULL, " ");
		char *lpszTypeLen = NULL;
		int nKeywordPos = get_keyword_pos(lpszVar);
		int nVarType = get_var_type(lpszType);

		LPCOMMANDVAR lpVar = lpStruct->vars;

		if (lpVar)
		{
			// Insert after an item
			if (nKeywordPos > lpVar->nKeywordPos)
			{
				while (lpVar->lpNext && nKeywordPos > lpVar->lpNext->nKeywordPos)
					lpVar = lpVar->lpNext;

				LPCOMMANDVAR lpBelow = lpVar->lpNext;

				lpVar->lpNext = (LPCOMMANDVAR)malloc(sizeof(COMMANDVAR));
				
				if (!lpVar->lpNext)
					return false;

				ZeroMemory(lpVar->lpNext, sizeof(COMMANDVAR));
				lpVar->lpNext->lpPrev = lpVar;
				lpVar->lpNext->lpNext = lpBelow;
				lpVar = lpVar->lpNext;
			}
			// Insert before all items
			else
			{
				lpVar->lpPrev = (LPCOMMANDVAR)malloc(sizeof(COMMANDVAR));
				
				if (!lpVar->lpPrev)
					return false;

				ZeroMemory(lpVar->lpPrev, sizeof(COMMANDVAR));

				lpVar->lpPrev->lpNext = lpVar;
				lpVar->lpPrev->lpPrev = NULL;
				lpVar = lpVar->lpPrev;
				lpStruct->vars = lpVar;
			}
		}
		// No existing list, create a new list with our entry
		else
		{
			lpVar = (LPCOMMANDVAR)malloc(sizeof(COMMANDVAR));
			
			if (!lpVar)
				return false;

			ZeroMemory(lpVar, sizeof(COMMANDVAR));
			lpStruct->vars = lpVar;
		}

		lpVar->lpszVar = strdup(lpszVar);
		lpVar->nType = nVarType;
		lpVar->nKeywordPos = nKeywordPos;

		if (nVarType == LLTYPE_VARIABLE || nVarType == LLTYPE_FIXED)
		{
			lpszTypeLen = strtok(NULL, " ");
			lpVar->nTypeLen = atoi(lpszTypeLen);
		}

		dwVarStart = dwVarEnd + 1;
		dwVarEnd = dwEnd;
	}

	return true;
}

// Parse the struct message template block of a command
bool get_struct_blocks(LPCOMMAND lpCmd, LPBYTE lpBuffer, DWORD dwStart, DWORD dwEnd)
{
	DWORD dwStructStart = dwStart;
	DWORD dwStructEnd = dwEnd;
	DWORD dwStructChildren = 0;

	while (get_block_markers(lpBuffer, dwStructStart, dwStructEnd, dwStructChildren))
	{
		char szStructLine[256];
		DWORD dwStructLen = (dwStructChildren ? dwStructChildren - 1 : dwStructEnd - 1) - dwStructStart;

		memcpy(&szStructLine, &lpBuffer[dwStructStart+1], dwStructLen);
		szStructLine[dwStructLen] = '\0';
		trim(szStructLine);
		
		//printf("\t%s\n", szStructLine);

		char *lpszStruct = strtok(szStructLine, " ");
		char *lpszType = strtok(NULL, " ");
		int nKeywordPos = get_keyword_pos(lpszStruct);
		int nVarType = get_var_type(lpszType);

		LPCOMMANDSTRUCT lpStruct = lpCmd->structs;

		if (lpStruct)
		{
			// Insert after an item
			if (nKeywordPos > lpStruct->nKeywordPos)
			{
				while (lpStruct->lpNext && nKeywordPos > lpStruct->lpNext->nKeywordPos)
					lpStruct = lpStruct->lpNext;

				LPCOMMANDSTRUCT lpBelow = lpStruct->lpNext;

				lpStruct->lpNext = (LPCOMMANDSTRUCT)malloc(sizeof(COMMANDSTRUCT));
			
				if (!lpStruct->lpNext)
					return false;

				ZeroMemory(lpStruct->lpNext, sizeof(COMMANDSTRUCT));
				lpStruct->lpNext->lpPrev = lpStruct;
				lpStruct->lpNext->lpNext = lpBelow;
				lpStruct = lpStruct->lpNext;
			}
			// Insert before all items
			else
			{
				lpStruct->lpPrev = (LPCOMMANDSTRUCT)malloc(sizeof(COMMANDSTRUCT));
				
				if (!lpStruct->lpPrev)
					return false;

				ZeroMemory(lpStruct->lpPrev, sizeof(COMMANDSTRUCT));

				lpStruct->lpPrev->lpNext = lpStruct;
				lpStruct->lpPrev->lpPrev = NULL;
				lpStruct = lpStruct->lpPrev;
				lpCmd->structs = lpStruct;
			}
		}
		// No existing list, create a new list with our entry
		else
		{
			lpCmd->structs = (LPCOMMANDSTRUCT)malloc(sizeof(COMMANDSTRUCT));
			
			if (!lpCmd->structs)
				return false;

			ZeroMemory(lpCmd->structs, sizeof(COMMANDSTRUCT));
			lpStruct = lpCmd->structs;
		}

		lpStruct->lpszStruct = strdup(lpszStruct);
		lpStruct->nKeywordPos = nKeywordPos;
		lpStruct->nType = nVarType;

		if (nVarType == LLTYPE_VARIABLE)
		{
			lpStruct->cItems = 1;
		}
		else if (nVarType == LLTYPE_MULTIPLE)
		{
			char *lpszTypeLen = strtok(NULL, " ");
			lpStruct->cItems = atoi(lpszTypeLen);
		}

		get_var_blocks(lpStruct, lpBuffer, dwStructStart + 1, dwStructEnd - 1);

		dwStructStart = dwStructEnd + 1;
		dwStructEnd = dwEnd;
	}

	return true;
}

// Parse the command message template blocks
bool get_command_blocks(LPBYTE lpBuffer, DWORD dwStart, DWORD dwEnd)
{
	DWORD dwCmdStart = dwStart;
	DWORD dwCmdEnd = dwEnd;
	DWORD dwCmdChildren = 0;
	
	while (get_block_markers(lpBuffer, dwCmdStart, dwCmdEnd, dwCmdChildren))
	{
		char szCmdLine[256];
		DWORD dwCmdLen = (dwCmdChildren ? dwCmdChildren - 1 : dwCmdEnd - 1) - dwCmdStart;

		memcpy(&szCmdLine, &lpBuffer[dwCmdStart+1], dwCmdLen);
		szCmdLine[dwCmdLen] = '\0';
		trim(szCmdLine);
		
		//printf("%s\n", szCmdLine);

		char *lpszCmd = strtok(szCmdLine, " ");
		char *lpszFreq = strtok(NULL, " ");
		char *lpszFixed = NULL;
		char *lpszTrust = NULL;
		char *lpszCoding = NULL;
		static DWORD dwLow = 1;
		static DWORD dwMed = 1;
		static DWORD dwHigh = 1;
		COMMAND *lpCmd = NULL;

		// Get the commands frequency
		if (!strnicmp(lpszFreq, "Fixed", 6))
		{
			lpszFixed = strtok(NULL, " ");
			DWORD dwFixed = (DWORD)httoi(lpszFixed) ^ 0xffff0000;
			lpCmd = &cmds_low[dwFixed];
		}
		else if (!strnicmp(lpszFreq, "Low", 4))
		{
			lpCmd = &cmds_low[dwLow++];
		}
		else if (!strnicmp(lpszFreq, "Medium", 7))
		{
			lpCmd = &cmds_med[dwMed++];
		}
		else if (!strnicmp(lpszFreq, "High", 5))
		{
			lpCmd = &cmds_high[dwHigh++];
		}

		lpszTrust = strtok(NULL, " ");
		lpszCoding = strtok(NULL, " ");
		
		lpCmd->lpszCmd = strdup(lpszCmd);
		
		// Is the command zero encoded?
		if (!strnicmp(lpszCoding, "Zerocoded", 10))
		{
			lpCmd->bZerocoded = true;
		}

		// Is the command trusted?
		if (!strnicmp(lpszTrust, "Trusted", 8))
		{
			lpCmd->bTrusted = true;
		}

		get_struct_blocks(lpCmd, lpBuffer, dwCmdStart + 1, dwCmdEnd - 1);

		//printf("----------------------\n");

		dwCmdStart = dwCmdEnd + 1;
		dwCmdEnd = dwEnd;
	}

	return true;
}

void dump_structs(LPCOMMANDSTRUCT lpStruct)
{
	while (lpStruct)
	{
		//dprintf("\t%04d %s (%s / %hu)\n", lpStruct->nKeywordPos, lpStruct->lpszStruct, LLTYPES[lpStruct->nType], lpStruct->cItems);

		LPCOMMANDVAR lpVar = lpStruct->vars;

		while (lpVar)
		{
			//dprintf("\t\t%04d %s (%s / %d)\n", lpVar->nKeywordPos, lpVar->lpszVar, LLTYPES[lpVar->nType], lpVar->nTypeLen);
			lpVar = lpVar->lpNext;
		}

//		if (lpStruct->lpNext)
//		{
			lpStruct = lpStruct->lpNext;
//			SAFE_FREE(lpStruct->lpPrev->lpszStruct);
//			SAFE_FREE(lpStruct->lpPrev);
//		}
//		else
//		{
//			SAFE_FREE(lpStruct->lpszStruct);
//			SAFE_FREE(lpStruct);
//		}
	}
}

void WINAPI parse_command(LPCOMMAND lpCommand, CServer *server, char *zerobuf, int *len, int pos)
{
	//dprintf("--- %s ---\n", lpCommand->lpszCmd);

	LPCOMMANDSTRUCT lpStruct = lpCommand->structs;

	while (lpStruct)
	{
		//dprintf("\t%04d %s (%s / %hu)\n", lpStruct->nKeywordPos, lpStruct->lpszStruct, LLTYPES[lpStruct->nType], lpStruct->cItems);
		BYTE cItems = 1;

		if (lpStruct->nType == LLTYPE_VARIABLE)
		{
			memcpy(&cItems, &zerobuf[pos], sizeof(cItems));
			pos += sizeof(cItems);
		}
		else if (lpStruct->nType == LLTYPE_MULTIPLE)
		{
			cItems = lpStruct->cItems;
		}

		for (BYTE c = 0; c < cItems; c++)
		{
			//dprintf("--- %s ----\n", lpStruct->lpszStruct);

			LPCOMMANDVAR lpVar = lpStruct->vars;

			while (lpVar)
			{
				//dprintf("\t\t%04d %s (%s / %d)\n", lpVar->nKeywordPos, lpVar->lpszVar, LLTYPES[lpVar->nType], lpVar->nTypeLen);

				switch (lpVar->nType)
				{
					case LLTYPE_U8:
						{
							unsigned char ubData;
							memcpy(&ubData, &zerobuf[pos], sizeof(ubData));
							pos += sizeof(ubData);
							//dprintf("%s: %hu\n", lpVar->lpszVar, ubData);
						}
						break;

					case LLTYPE_U16:
						{
							WORD wData;
							memcpy(&wData, &zerobuf[pos], sizeof(wData));
							pos += sizeof(wData);
							//dprintf("%s: %u\n", lpVar->lpszVar, wData);
						}
						break;

					case LLTYPE_U32:
						{
							DWORD dwData;
							memcpy(&dwData, &zerobuf[pos], sizeof(dwData));
							pos += sizeof(dwData);
							//dprintf("%s: %lu\n", lpVar->lpszVar, dwData);
						}
						break;

					case LLTYPE_U64:
						{
							ULONGLONG ullData;
							memcpy(&ullData, &zerobuf[pos], sizeof(ullData));
							pos += sizeof(ullData);
							//dprintf("%s: %I64u\n", lpVar->lpszVar, ullData);
						}
						break;

					case LLTYPE_S8:
						{
							BYTE bData;
							memcpy(&bData, &zerobuf[pos], sizeof(bData));
							pos += sizeof(bData);
							//dprintf("%s: %hd\n", lpVar->lpszVar, bData);
						}
						break;

					case LLTYPE_S16:
						{
							SHORT sData;
							memcpy(&sData, &zerobuf[pos], sizeof(sData));
							pos += sizeof(sData);
							//dprintf("%s: %d\n", lpVar->lpszVar, sData);
						}
						break;

					case LLTYPE_S32:
						{
							LONG nData;
							memcpy(&nData, &zerobuf[pos], sizeof(nData));
							pos += sizeof(nData);
							//dprintf("%s: %ld\n", lpVar->lpszVar, nData);
						}
						break;

					case LLTYPE_S64:
						break;

					case LLTYPE_F8:
						break;

					case LLTYPE_F16:
						break;

					case LLTYPE_F32:
						{
							FLOAT fData;
							memcpy(&fData, &zerobuf[pos], sizeof(fData));
							pos += sizeof(fData);
							//dprintf("%s: %f\n", lpVar->lpszVar, fData);
						}
						break;

					case LLTYPE_F64:
						{
							double dData;
							memcpy(&dData, &zerobuf[pos], sizeof(dData));
							pos += sizeof(dData);
							//dprintf("%s: %f\n", lpVar->lpszVar, dData);
						}
						break;

					case LLTYPE_LLUUID:
						{
							BYTE bData[16];
							memcpy(&bData, &zerobuf[pos], sizeof(bData));
							pos += sizeof(bData);
							//dprintf("%s: ", lpVar->lpszVar);
							//for (int u = 0; u < sizeof(bData); u++)
								//dprintf("%02x", bData[u]);
							//dprintf("\n");
						}
						break;

					case LLTYPE_BOOL:
						{
							BYTE bData;
							memcpy(&bData, &zerobuf[pos], sizeof(bData));
							pos += sizeof(bData);
							//dprintf("%s: %s\n", lpVar->lpszVar, (bData) ? "True" : "False");
						}
						break;

					case LLTYPE_LLVECTOR3:
						{
							FLOAT fData[3];
							memcpy(&fData, &zerobuf[pos], sizeof(fData));
							pos += sizeof(fData);
							//dprintf("%s: %f, %f, %f\n", lpVar->lpszVar, fData[0], fData[1], fData[2]);
						}
						break;

					case LLTYPE_LLVECTOR3D:
						{
							double dData[3];
							memcpy(&dData, &zerobuf[pos], sizeof(dData));
							pos += sizeof(dData);
							//dprintf("%s: %f, %f, %f\n", lpVar->lpszVar, dData[0], dData[1], dData[2]);
						}
						break;
					
					/*case LLTYPE_VECTOR4:
						{
							FLOAT fData[4];
							memcpy(&fData, &zerobuf[pos], sizeof(fData));
							pos += sizeof(fData);
							dprintf("%s: %f, %f, %f, %f\n", lpVar->lpszVar, fData[0], fData[1], fData[2], fData[3]);
						}
						break;*/

					case LLTYPE_QUATERNION:
						{
							FLOAT fData[4];
							memcpy(&fData, &zerobuf[pos], sizeof(fData));
							pos += sizeof(fData);
							//dprintf("%s: %f, %f, %f, %f\n", lpVar->lpszVar, fData[0], fData[1], fData[2], fData[3]);
						}
						break;

					case LLTYPE_IPADDR:
						{
							BYTE ipData[4];
							memcpy(&ipData, &zerobuf[pos], sizeof(ipData));
							pos += sizeof(ipData);
							//dprintf("%s: %hu.%hu.%hu.%hu\n", lpVar->lpszVar, ipData[0], ipData[1], ipData[2], ipData[3]);
						}
						break;

					case LLTYPE_IPPORT:
						{
							WORD wData;
							memcpy(&wData, &zerobuf[pos], sizeof(wData));
							pos += sizeof(wData);
							//dprintf("%s: %hu\n", lpVar->lpszVar, htons(wData));
						}
						break;

					case LLTYPE_VARIABLE:
						{
							if (lpVar->nTypeLen == 1)
							{
								BYTE cDataLen;
								LPBYTE lpData = NULL;

								memcpy(&cDataLen, &zerobuf[pos], sizeof(cDataLen));
								pos += sizeof(cDataLen);
							
								if (cDataLen > 0)
									lpData = (LPBYTE)malloc(cDataLen);

								if (lpData)
									memcpy(lpData, &zerobuf[pos], cDataLen);

								pos += cDataLen;

								if (lpData)
								{
									bool bPrintable = true;

									for (int j = 0; j < cDataLen - 1; j++)
									{
										if (((unsigned char)lpData[j] < 0x20 || (unsigned char)lpData[j] > 0x7E) && (unsigned char)lpData[j] != 0x09 && (unsigned char)lpData[j] != 0x0D)
											bPrintable = false;
									}

									if (bPrintable && lpData[cDataLen - 1] == '\0')
									{
										//dprintf("%s: %s\n", lpVar->lpszVar, lpData);
									}
									else
									{
										for (int j = 0; j < cDataLen; j += 16)
										{
											//dprintf("%s: ", lpVar->lpszVar);

											for (int k = 0; k < 16; k++)
											{
												if ((j + k) < cDataLen)
												{
													//dprintf("%02x ", (unsigned char)lpData[j+k]);
												}
												else
												{
													//dprintf("   ");
												}
											}

											for (int k = 0; k < 16 && (j + k) < cDataLen; k++)
											{
												//dprintf("%c", ((unsigned char)lpData[j+k] >= 0x20 && (unsigned char)lpData[j+k] <= 0x7E) ? (unsigned char)lpData[j+k] : '.');
											}

											//dprintf("\n");
										}
									}
								}

								SAFE_FREE(lpData);
							}
							else if (lpVar->nTypeLen == 2)
							{
								WORD cDataLen;
								LPBYTE lpData = NULL;

								memcpy(&cDataLen, &zerobuf[pos], sizeof(cDataLen));
								pos += sizeof(cDataLen);
							
								if (cDataLen > 0)
									lpData = (LPBYTE)malloc(cDataLen);

								if (lpData)
									memcpy(lpData, &zerobuf[pos], cDataLen);

								pos += cDataLen;

								if (lpData)
								{
									bool bPrintable = true;

									for (int j = 0; j < cDataLen - 1; j++)
									{
										if (((unsigned char)lpData[j] < 0x20 || (unsigned char)lpData[j] > 0x7E) && (unsigned char)lpData[j] != 0x09 && (unsigned char)lpData[j] != 0x0D)
											bPrintable = false;
									}

									if (bPrintable && lpData[cDataLen - 1] == '\0')
									{
										//dprintf("%s: %s\n", lpVar->lpszVar, lpData);
									}
									else
									{
										for (int j = 0; j < cDataLen; j += 16)
										{
											//dprintf("%s: ", lpVar->lpszVar);

											for (int k = 0; k < 16; k++)
											{
												if ((j + k) < cDataLen)
												{
													//dprintf("%02x ", (unsigned char)lpData[j+k]);
												}
												else
												{
													//dprintf("   ");
												}
											}

											for (int k = 0; k < 16 && (j + k) < cDataLen; k++)
											{
												//dprintf("%c", ((unsigned char)lpData[j+k] >= 0x20 && (unsigned char)lpData[j+k] <= 0x7E) ? (unsigned char)lpData[j+k] : '.');
											}

											//dprintf("\n");
										}
									}
								}
			
								SAFE_FREE(lpData);
							}
						}
						break;

					case LLTYPE_FIXED:
						{
							LPBYTE lpData = NULL;

							if (lpVar->nTypeLen > 0)
								lpData = (LPBYTE)malloc(lpVar->nTypeLen);

							if (lpData)
								memcpy(lpData, &zerobuf[pos], lpVar->nTypeLen);

							pos += lpVar->nTypeLen;

							if (lpData)
							{
								bool bPrintable = true;

								for (int j = 0; j < lpVar->nTypeLen - 1; j++)
								{
									if (((unsigned char)lpData[j] < 0x20 || (unsigned char)lpData[j] > 0x7E) && (unsigned char)lpData[j] != 0x09 && (unsigned char)lpData[j] != 0x0D)
										bPrintable = false;
								}

								if (bPrintable && lpData[lpVar->nTypeLen - 1] == '\0')
								{
									//dprintf("%s: %s\n", lpVar->lpszVar, lpData);
								}
								else
								{
									for (int j = 0; j < lpVar->nTypeLen; j += 16)
									{
										//dprintf("%s: ", lpVar->lpszVar);

										for (int k = 0; k < 16; k++)
										{
											if ((j + k) < lpVar->nTypeLen)
											{
												//dprintf("%02x ", (unsigned char)lpData[j+k]);
											}
											else
											{
												//dprintf("   ");
											}
										}

										for (int k = 0; k < 16 && (j + k) < lpVar->nTypeLen; k++)
										{
											//dprintf("%c", ((unsigned char)lpData[j+k] >= 0x20 && (unsigned char)lpData[j+k] <= 0x7E) ? (unsigned char)lpData[j+k] : '.');
										}

										//dprintf("\n");
									}
								}
							}

							SAFE_FREE(lpData);
						}
						break;

					case LLTYPE_SINGLE:
					case LLTYPE_MULTIPLE:
					case LLTYPE_NULL:
					default:
						break;
				}
				
				lpVar = lpVar->lpNext;
			}
		}

		lpStruct = lpStruct->lpNext;
	}
}

CMessage * WINAPI map_command(LPCOMMAND lpCommand, CServer *server, char *zerobuf, int *len, int pos)
{
//	dprintf("--- %s ---\n", lpCommand->lpszCmd);

	int oldPos = pos;

	CMessage *msg = new CMessage;

	if (!msg)
		return NULL;

	msg->SetCommand(lpCommand->lpszCmd);

	LPCOMMANDSTRUCT lpStruct = lpCommand->structs;

	while (lpStruct)
	{
		//dprintf("\t%04d %s (%s / %hu)\n", lpStruct->nKeywordPos, lpStruct->lpszStruct, LLTYPES[lpStruct->nType], lpStruct->cItems);
		BYTE cItems = 1;

		if (lpStruct->nType == LLTYPE_VARIABLE)
		{
			memcpy(&cItems, &zerobuf[pos], sizeof(cItems));
			pos += sizeof(cItems);
		}
		else if (lpStruct->nType == LLTYPE_MULTIPLE)
		{
			cItems = lpStruct->cItems;
		}

		for (BYTE c = 0; c < cItems; c++)
		{
			//dprintf("--- %s ----\n", lpStruct->lpszStruct);
			CBlock *block = new CBlock;
			msg->AddBlock(lpStruct->lpszStruct, lpStruct->nType, block);

			LPCOMMANDVAR lpVar = lpStruct->vars;

			while (lpVar)
			{
				//dprintf("\t\t%04d %s (%s / %d)\n", lpVar->nKeywordPos, lpVar->lpszVar, LLTYPES[lpVar->nType], lpVar->nTypeLen);
				CVar *var = new CVar;
				var->SetVar(lpVar->lpszVar);
				var->SetType(lpVar->nType, lpVar->nTypeLen);
				pos += var->SetData((LPBYTE)&zerobuf[pos]);
				block->AddVar(var);
				lpVar = lpVar->lpNext;
			}
		}

		lpStruct = lpStruct->lpNext;
	}

	BYTE bPack[4096];
	int nPackedSize = msg->Pack(bPack);
	int diff = memcmp(bPack, &zerobuf[oldPos], nPackedSize);

	if (diff)
	{
		msg->Dump();
		dprintf("PACKED: %d / %d (%d) ===> %d\n", nPackedSize, *len, oldPos, diff);
	}

	return msg;
}

void WINAPI cmd_Silent(LPCOMMAND lpCommand, CServer *server, char *zerobuf, int *len, int pos)
{
}

void WINAPI cmd_Default(LPCOMMAND lpCommand, CServer *server, char *zerobuf, int *len, int pos)
{
	//dprintf("Flags: %u\n", zerobuf[0]);
	CMessage *msg = map_command(lpCommand, server, zerobuf, len, pos);
	msg->Dump();
	SAFE_DELETE(msg);
}

void WINAPI cmd_LoginReply(LPCOMMAND lpCommand, CServer *server, char *zerobuf, int *len, int pos)
{
	parse_command(lpCommand, server, zerobuf, len, pos);
}

CMDHOOK pCMDHooks[] = {
	{ _T("Default"),					(PROC)cmd_Default			},
	{ _T("DirLandReply"),				(PROC)cmd_Silent			}, // Silence the most common
	{ _T("AvatarAnimation"),			(PROC)cmd_Silent			}, // packets
	{ _T("CoarseLocationUpdate"),		(PROC)cmd_Silent			},
	{ _T("CompletePingCheck"),			(PROC)cmd_Silent			},
	{ _T("LayerData"),					(PROC)cmd_Silent			},
	{ _T("PacketAck"),					(PROC)cmd_Silent			},
	{ _T("StartPingCheck"),				(PROC)cmd_Silent			},
	{ _T("SimulatorViewerTimeMessage"),	(PROC)cmd_Silent			},
	{ _T("ImagePacket"),				(PROC)cmd_Silent			},
	{ _T("TransferPacket"),				(PROC)cmd_Silent			},
	{ _T("ObjectUpdate"),				(PROC)cmd_Silent			},
	{ _T("ObjectUpdateCompressed"),		(PROC)cmd_Silent			},
	{ _T("AgentThrottle"),				(PROC)cmd_Silent			},
	{ _T("CoarseLocationUpdate"),		(PROC)cmd_Silent			},
	{ _T("UUIDNameReply"),				(PROC)cmd_Silent			},
	{ _T("RequestImage"),				(PROC)cmd_Silent			},
	{ _T("ImageData"),					(PROC)cmd_Silent			},
	{ _T("SimStats"),					(PROC)cmd_Silent			},
	{ _T("ViewerEffect"),				(PROC)cmd_Silent			},
	{ _T("TransferRequest"),			(PROC)cmd_Silent			},
	{ _T("DirClassifiedReply"),			(PROC)cmd_Silent			},
	{ _T("DirEventsReply"),				(PROC)cmd_Silent			},
	{ _T("DirPopularReply"),			(PROC)cmd_Silent			},
	{ _T("DirLandReply"),				(PROC)cmd_Silent			},
	{ _T("AgentUpdate"),				(PROC)cmd_Silent			},
	{ _T("ObjectUpdateCached"),			(PROC)cmd_Silent			},
	{ _T("ImprovedTerseObjectUpdate"),	(PROC)cmd_Silent			},
	{ _T("RequestMultipleObjects"),		(PROC)cmd_Silent			},
	{ _T("AttachedSound"),				(PROC)cmd_Silent			},
	{ _T("ViewerStats"),				(PROC)cmd_Silent			},
	{ _T("TransferInfo"),				(PROC)cmd_Silent			},
	{ _T("ParcelOverlay"),				(PROC)cmd_Silent			},
	{ _T("SendXferPacket"),				(PROC)cmd_Silent			},
	{ _T("DirPlacesReply"),				(PROC)cmd_Silent			},
	{ NULL,								NULL						}
};

int decomm()
{
	FILE *fpComm;
	FILE *fpMsg;

	fpComm = fopen(g_pConfig->m_pCommDatPath, "rb");

	if (!fpComm)
	{
		printf("Couldn't open %s for reading, aborting...\n", g_pConfig->m_pCommDatPath);
		return -1;
	}

	fpMsg = fopen(g_pConfig->m_pMessageTemplatePath, "wb");

	if (!fpMsg)
	{
		printf("Couldn't open %s for writing, aborting...\n", g_pConfig->m_pMessageTemplatePath);
		return -1;
	}

	printf("Decrypting %s to %s\n", g_pConfig->m_pCommDatPath, g_pConfig->m_pMessageTemplatePath);
	static unsigned char ucMagicKey = 0;
	long lTemplateSize = 0;

	fseek(fpComm, 0, SEEK_END);
	lTemplateSize = ftell(fpComm);
	fseek(fpComm, 0, SEEK_SET);

	BYTE buffer[2048];
	BYTE stripped[2048];
	LPBYTE lpTemplate = (LPBYTE)malloc(lTemplateSize);
	DWORD dwTemplateWrote = 0;

	if (!lpTemplate)
		return -1;

	bool bComment = false;

	while (!feof(fpComm))
	{
		size_t stRead = fread(&buffer, 1, sizeof(buffer), fpComm);
		size_t stStripped = 0;

		for (size_t stCount = 0; stCount < stRead; stCount++)
		{
			buffer[stCount] ^= ucMagicKey;

			if (!bComment && buffer[stCount] != '/')
				stripped[stStripped++] = buffer[stCount];

			if (bComment && buffer[stCount] == '\n')
				bComment = false;

			if (!bComment && buffer[stCount] == '/')
				bComment = true;

			ucMagicKey += 43;
		}

		memcpy(lpTemplate + dwTemplateWrote, &stripped, stStripped);
		dwTemplateWrote += (DWORD)stStripped;
		
		size_t stWrote = fwrite(&stripped, 1, stStripped, fpMsg);

		printf(".");
		fflush(stdout);
	}

	printf("\nDone.\n");

	printf("template size: %ld\n", lTemplateSize);

	ZeroMemory(&cmds_low, sizeof(cmds_low));
	ZeroMemory(&cmds_med, sizeof(cmds_med));
	ZeroMemory(&cmds_high, sizeof(cmds_high));

	get_command_blocks(lpTemplate, 0, lTemplateSize);
	
	fclose(fpComm);
	fclose(fpMsg);

	for (int i = 1; i < MAX_COMMANDS_LOW; i++)
	{
		if (cmds_low[i].lpszCmd)
		{
			//dprintf("LOW %05d - %s - %s - %s\n", i, cmds_low[i].lpszCmd, cmds_low[i].bTrusted ? "Trusted" : "Untrusted", cmds_low[i].bZerocoded ? "Zerocoded" : "Unencoded");
			dump_structs(cmds_low[i].structs);
			//SAFE_FREE(cmds_low[i].lpszCmd);
		}
	}

	for (int i = 1; i < MAX_COMMANDS_MEDIUM; i++)
	{
		if (cmds_med[i].lpszCmd)
		{
			//dprintf("Medium %05d - %s\n", i, cmds_med[i].lpszCmd);
			dump_structs(cmds_med[i].structs);
			//SAFE_FREE(cmds_med[i].lpszCmd);
		}
	}

	for (int i = 1; i < MAX_COMMANDS_HIGH; i++)
	{
		if (cmds_high[i].lpszCmd)
		{
			//dprintf("High %05d - %s\n", i, cmds_high[i].lpszCmd);
			dump_structs(cmds_high[i].structs);
			//SAFE_FREE(cmds_high[i].lpszCmd);
		}
	}

	return 0;
}

HMODULE WINAPI new_LoadLibraryA(
	LPCSTR lpLibFileName
	)
{
	if (g_hOpenGL != NULL && !_tcsicmp(lpLibFileName, "OPENGL32"))
		return g_hOpenGL;

//	dprintf("[LoadLibraryA] Filename(%s)\n", lpLibFileName);

	HMODULE hModule = NULL;
	PROC pOldProc = pAPIHooks[APIHOOK_LOADLIBRARYA].pOldProc;

	if (pOldProc != NULL && !IsBadCodePtr((PROC)pOldProc))
	{
		hModule = ((HMODULE (WINAPI *)(LPCSTR))pOldProc)(lpLibFileName);
		if (hModule != NULL)
		{
			SaveImportHooks();
			InstallImportHooks();
		}
	}

	if (!_tcsicmp(lpLibFileName, "OPENGL32"))
		g_hOpenGL = hModule;

	return hModule;
}

BOOL WINAPI new_FreeLibraryA(
	HMODULE hLibModule
	)
{
	if (hLibModule == g_hOpenGL)
	{
		return TRUE;
	}
	
	return ((BOOL (WINAPI *)(HMODULE))pAPIHooks[APIHOOK_FREELIBRARYA].pOldProc)(hLibModule);
}

HMODULE WINAPI new_LoadLibraryW(
	LPCWSTR lpLibFileName
	)
{
//	dprintf("[LoadLibraryW] Filename(%S)\n", lpLibFileName);

	HMODULE hModule = NULL;
	PROC pOldProc = pAPIHooks[APIHOOK_LOADLIBRARYW].pOldProc;

	if (pOldProc != NULL && !IsBadCodePtr((PROC)pOldProc))
	{
		hModule = ((HMODULE (WINAPI *)(LPCWSTR))pOldProc)(lpLibFileName);
		if (hModule != NULL)
		{
			SaveImportHooks();
			InstallImportHooks();
		}
	}

	return hModule;
}

HMODULE WINAPI new_LoadLibraryExA(
	LPCSTR lpLibFileName,
	HANDLE hFile,
	DWORD dwFlags
	)
{
//	dprintf("[LoadLibraryExA] Filename(%s)\n", lpLibFileName);

	HMODULE hModule = NULL;
	PROC pOldProc = pAPIHooks[APIHOOK_LOADLIBRARYEXA].pOldProc;

	if (pOldProc != NULL && !IsBadCodePtr((PROC)pOldProc))
	{
		hModule = ((HMODULE (WINAPI *)(LPCSTR, HANDLE, DWORD))pOldProc)(lpLibFileName, hFile, dwFlags);
		if (hModule != NULL)
		{
			SaveImportHooks();
			InstallImportHooks();
		}
	}

	return hModule;
}

HMODULE WINAPI new_LoadLibraryExW(
	LPCWSTR lpLibFileName,
	HANDLE hFile,
	DWORD dwFlags)
{
//	dprintf("[LoadLibraryExW] Filename(%S)\n", lpLibFileName);

	HMODULE hModule = NULL;
	PROC pOldProc = pAPIHooks[APIHOOK_LOADLIBRARYEXW].pOldProc;

	if (pOldProc != NULL && !IsBadCodePtr((PROC)pOldProc))
	{
		hModule = ((HMODULE (WINAPI *)(LPCWSTR, HANDLE, DWORD))pOldProc)(lpLibFileName, hFile, dwFlags);
		if (hModule != NULL)
		{
			SaveImportHooks();
			InstallImportHooks();
		}
	}

	return hModule;
}

FARPROC WINAPI new_GetProcAddress(HMODULE hModule, LPCSTR lpProcName)
{
	TCHAR szMod[MAX_PATH];

	GetModuleFileName(hModule, szMod, sizeof(szMod));

	if (HIWORD((DWORD)lpProcName) == 0)
	{
		WORD wOrdinal = LOWORD((DWORD)lpProcName);
//		dprintf(_T("[GetProcAddress] [%s][@%d]\n"), szMod, wOrdinal);
	}
	else
	{
		if (lpProcName != NULL && !IsBadCodePtr((PROC)lpProcName))
		{
			if (!stricmp(lpProcName, "wglSwapBuffers"))
			{
				
			}
			else if (!stricmp(lpProcName, "InternetReadFile"))
			{
				dprintf(_T("[***GetProcAddress] [%s][%s]\n"), szMod, lpProcName);
				return pAPIHooks[(int)APIHOOK_INTERNETREADFILE].pNewProc;
			}
			else if (!stricmp(lpProcName, "InternetOpenUrlA"))
			{
				dprintf(_T("[***GetProcAddress] [%s][%s]\n"), szMod, lpProcName);
				return pAPIHooks[(int)APIHOOK_INTERNETOPENURLA].pNewProc;
			}
			else if (!stricmp(lpProcName, "CPEncrypt"))
			{
				dprintf(_T("[***CPEncrypt] [%s][%s]\n"), szMod, lpProcName);
				return pAPIHooks[(int)APIHOOK_CPENCRYPT].pNewProc;
			}
			//else
				//dprintf(_T("[GetProcAddress] [%s][%s]\n"), szMod, lpProcName);
		}
		//else
			//dprintf(_T("[GetProcAddress] [%s][<Bad Ptr>]\n"), szMod);
	}

	return ((FARPROC (WINAPI *)(HMODULE, LPCSTR))pAPIHooks[(int)APIHOOK_GETPROCADDRESS].pOldProc)(hModule, lpProcName);
}

struct hostent FAR * WINAPI new_gethostbyname(
	const char FAR *name
	)
{
	dprintf("[new_gethostbyname] name(%s)\n", name);

	struct hostent FAR * hp = ((struct hostent FAR * (WINAPI *)(const char FAR *))pAPIHooks[(int)APIHOOK_GETHOSTBYNAME].pOldProc)(name);

	return hp;
}

int WINAPI new_connect(
	SOCKET s,
	const struct sockaddr FAR *name,
	int namelen
	)
{
	dprintf("[connect] socket(0x%08X) host(%s) port(%d)\n", s, inet_ntoa(((struct sockaddr_in *)name)->sin_addr), ntohs(((struct sockaddr_in *)name)->sin_port));

	return ((int (WINAPI *)(SOCKET, const struct sockaddr FAR *, int))pAPIHooks[(int)APIHOOK_CONNECT].pOldProc)(s, name, namelen);
}

int WINAPI new_recv(
	SOCKET s,
	char *buf,
	int len,
	int flags
	)
{
	//dprintf("[recv] socket(0x%08X)\n", s);

	int nRes = 0;
	{
		nRes = ((int (WINAPI *)(SOCKET, char *, int, int))pAPIHooks[APIHOOK_RECV].pOldProc)(s, buf, len, flags);
	}
/*
	for (int j = 0; j < nRes; j += 16)
	{
		for (int k = 0; k < 16; k++)
		{
			if ((j + k) < nRes)
			{
				dprintf("%02x ", (unsigned char)buf[j+k]);
			}
			else
			{
				dprintf("   ");
			}
		}

		for (int k = 0; k < 16 && (j + k) < nRes; k++)
		{
			dprintf("%c", ((unsigned char)buf[j+k] >= 0x20 && (unsigned char)buf[j+k] <= 0x7E) ? (unsigned char)buf[j+k] : '.');
		}

		dprintf("\n");
	}
*/
	return nRes;
}

int WINAPI new_recvfrom(
	SOCKET s,
	char *buf,
	int len,
	int flags,
	struct sockaddr *from,
	int *fromlen
	)
{
	int nRes = 0;
	WORD wSeq = 0;
	int zerolen = 0;
	static char zerobuf[8192];
	bool bZerocoded = false;
	CServer *server = NULL;
	CSequence *sequence = NULL;

	nRes = ((int (WINAPI *)(SOCKET, char *, int, int, struct sockaddr *, int *))pAPIHooks[APIHOOK_RECVFROM].pOldProc)(s, buf, len, flags, from, fromlen);

	// !!!
	//return nRes;

	if (nRes > 0)
	{
		//dprintf("Receiving %u bytes\n", nRes);

		// Get packet sequence number
		memcpy(&wSeq, &buf[2], sizeof(wSeq));
		wSeq = htons(wSeq);

		// Get current server
		server = servers.FindServer((struct sockaddr_in *)from);

		// If server isn't in our list add it as new
		if (!server)
		{
			server = new CServer((struct sockaddr_in *)from);
			servers.AddServer(server);
		}

		// Handle packet acks
		BYTE bAppended[4096];
		int nAppendedLen = 0;

		if (buf[0] & MSG_APPENDED_ACKS)
		{
			//dprintf("APPENDED ACKS\n");

			BYTE cPacketsItems = buf[nRes - 1];
			
			int nOldRes = nRes;

			nRes = nOldRes - 1 - (cPacketsItems * sizeof(DWORD));
			//dprintf("NEW AND OLD: %d / %d\n", nRes, nOldRes);
			nAppendedLen = nOldRes - nRes;
			memcpy(bAppended, &buf[nRes], nAppendedLen);
		}
		zerolen = ZeroDecode(buf, nRes, zerobuf, sizeof(zerobuf));

		if ((unsigned char)buf[4] != 0xff)
		{
			// High
			//dprintf("parse high: %hu\n", zerobuf[4]);

			bool bCmdFound = false;

			for (int j = 0; pCMDHooks[j].szCommand; j++)
			{
				if (_tcsicmp(cmds_high[zerobuf[4]].lpszCmd, pCMDHooks[j].szCommand) == 0 && pCMDHooks[j].pProc != NULL && !IsBadCodePtr(pCMDHooks[j].pProc))
				{
					bCmdFound = true;
					((void (WINAPI *)(LPCOMMAND, CServer *, char *, int *, int))pCMDHooks[j].pProc)(&cmds_high[zerobuf[4]], server, zerobuf, &zerolen, 5);
					break;
				}
			}

			if (!bCmdFound)
			{
				((void (WINAPI *)(LPCOMMAND, CServer *, char *, int *, int))pCMDHooks[0].pProc)(&cmds_high[zerobuf[4]], server, zerobuf, &zerolen, 5);
			}
		}

		else if ((unsigned char)buf[5] != 0xff)
		{
			// Medium
			//dprintf("parse med: %hu\n", zerobuf[5]);

			bool bCmdFound = false;

			for (int j = 0; pCMDHooks[j].szCommand; j++)
			{
				if (!_tcsicmp(cmds_med[zerobuf[5]].lpszCmd, pCMDHooks[j].szCommand) && pCMDHooks[j].pProc != NULL && !IsBadCodePtr(pCMDHooks[j].pProc))
				{
					bCmdFound = true;
					((void (WINAPI *)(LPCOMMAND, CServer *, char *, int *, int))pCMDHooks[j].pProc)(&cmds_med[zerobuf[5]], server, zerobuf, &zerolen, 6);
					break;
				}
			}

			if (!bCmdFound)
			{
				((void (WINAPI *)(LPCOMMAND, CServer *, char *, int *, int))pCMDHooks[0].pProc)(&cmds_med[zerobuf[5]], server, zerobuf, &zerolen, 6);
			}
		}
		
		else if ((unsigned char)buf[4] == 0xff && (unsigned char)buf[5] == 0xff)
		{
			// Fixed
			// Low

			//dprintf("parse low: %hu\n", htons(wFreq));

			bool bCmdFound = false;
			WORD wFreq;
			memcpy(&wFreq, &zerobuf[6], sizeof(wFreq));

			for (int j = 0; pCMDHooks[j].szCommand; j++)
			{
				if (!_tcsicmp(cmds_low[htons(wFreq)].lpszCmd, pCMDHooks[j].szCommand) && pCMDHooks[j].pProc != NULL && !IsBadCodePtr(pCMDHooks[j].pProc))
				{
					bCmdFound = true;
					((void (WINAPI *)(LPCOMMAND, CServer *, char *, int *, int))pCMDHooks[j].pProc)(&cmds_low[htons(wFreq)], server, zerobuf, &zerolen, 8);
					break;
				}
			}

			if (!bCmdFound)
			{
				((void (WINAPI *)(LPCOMMAND, CServer *, char *, int *, int))pCMDHooks[0].pProc)(&cmds_low[htons(wFreq)], server, zerobuf, &zerolen, 8);
			}
		}

		nRes = ZeroEncode(zerobuf, zerolen, buf, len);

		if (nAppendedLen > 0)
		{
			memcpy(&buf[nRes], bAppended, nAppendedLen);
			nRes += nAppendedLen;
		}
	}
	else
	{
		// We can use this area to synthesize incoming packets
	}

	return nRes;
}

int WINAPI new_send(
	SOCKET s,
	const char *buf,
	int len,
	int flags
	)
{
//	dprintf("[send] socket(0x%08X)\n", s);

	int nRes = 0;
	{
		nRes = ((int (WINAPI *)(SOCKET, const char *, int, int))pAPIHooks[APIHOOK_SEND].pOldProc)(s, buf, len, flags);
	}

	return nRes;
}

int WINAPI new_sendto(
	SOCKET s,
	char *buf,
	int len,
	int flags,
	struct sockaddr *to,
	int tolen
	)
{
	int nRes = 0;
	WORD wSeq = 0;
	int zerolen = 0;
	char *zerobuf = NULL;
	bool bZerocoded = false;
	CServer *server = NULL;
	CSequence *sequence = NULL;

	// Get packet sequence number
	memcpy(&wSeq, &buf[2], sizeof(wSeq));
	wSeq = htons(wSeq);

	// Get current server
	server = servers.FindServer((struct sockaddr_in *)to);

	// If server isn't in our list add it as new
	if (!server)
	{
		server = new CServer((struct sockaddr_in *)to);
		servers.AddServer(server);
	}

	static int nDropPacket = 0;

	if (buf[0] & MSG_ZEROCODED)
	{
		bZerocoded = true;
		zerolen = 4;

		for (int i = 4; i < len; i++)
		{
			if ((unsigned char)buf[i] == 0x00)
				zerolen += (unsigned char)buf[++i];
			else
				zerolen++;
		}

		zerobuf = (char *)malloc(zerolen);

		if (!zerobuf)
		{
			dprintf("Couldn't allocate memory for zerocoded buffer\n");
			return -1;
		}

		int j = 4;
		memcpy(zerobuf, buf, 4);
		
		for (int i = 4; i < len; i++)
		{
			if ((unsigned char)buf[i] == 0x00)
			{
				for (int z = 0; z < (unsigned char)buf[i+1]; z++)
					zerobuf[j++] = 0x00;

				i++;
			}
			else
				zerobuf[j++] = buf[i];
		}
	}
	else
	{
		zerolen = len;
		zerobuf = buf;
	}

	nRes = ((int (WINAPI *)(SOCKET, char *, int, int, struct sockaddr *, int))pAPIHooks[APIHOOK_SENDTO].pOldProc)(s, buf, len, flags, to, tolen);

	// !!!
	//return nRes;

	//dprintf("Sending %u bytes\n", nRes);

	if ((unsigned char)buf[4] != 0xff)
	{
		// High
		//dprintf("parse high: %hu\n", zerobuf[4]);

		bool bCmdFound = false;

		for (int j = 0; pCMDHooks[j].szCommand; j++)
		{
			if (_tcsicmp(cmds_high[zerobuf[4]].lpszCmd, pCMDHooks[j].szCommand) == 0 && pCMDHooks[j].pProc != NULL && !IsBadCodePtr(pCMDHooks[j].pProc))
			{
				bCmdFound = true;
				((void (WINAPI *)(LPCOMMAND, CServer *, char *, int *, int))pCMDHooks[j].pProc)(&cmds_high[zerobuf[4]], server, zerobuf, &zerolen, 5);
				break;
			}
		}

		if (!bCmdFound)
		{
			((void (WINAPI *)(LPCOMMAND, CServer *, char *, int *, int))pCMDHooks[0].pProc)(&cmds_high[zerobuf[4]], server, zerobuf, &zerolen, 5);
		}
	}

	else if ((unsigned char)buf[5] != 0xff)
	{
		// Medium
		//dprintf("parse med: %hu\n", zerobuf[5]);

		bool bCmdFound = false;

		for (int j = 0; pCMDHooks[j].szCommand; j++)
		{
			if (!_tcsicmp(cmds_med[zerobuf[5]].lpszCmd, pCMDHooks[j].szCommand) && pCMDHooks[j].pProc != NULL && !IsBadCodePtr(pCMDHooks[j].pProc))
			{
				bCmdFound = true;
				((void (WINAPI *)(LPCOMMAND, CServer *, char *, int *, int))pCMDHooks[j].pProc)(&cmds_med[zerobuf[5]], server, zerobuf, &zerolen, 6);
				break;
			}
		}

		if (!bCmdFound)
		{
			((void (WINAPI *)(LPCOMMAND, CServer *, char *, int *, int))pCMDHooks[0].pProc)(&cmds_med[zerobuf[5]], server, zerobuf, &zerolen, 6);
		}
	}
	
	else if ((unsigned char)buf[4] == 0xff && (unsigned char)buf[5] == 0xff)
	{
		// Fixed
		// Low

		//dprintf("parse low: %hu\n", htons(wFreq));

		bool bCmdFound = false;
		WORD wFreq;
		memcpy(&wFreq, &zerobuf[6], sizeof(wFreq));

		for (int j = 0; pCMDHooks[j].szCommand; j++)
		{
			if (!_tcsicmp(cmds_low[htons(wFreq)].lpszCmd, pCMDHooks[j].szCommand) && pCMDHooks[j].pProc != NULL && !IsBadCodePtr(pCMDHooks[j].pProc))
			{
				bCmdFound = true;
				((void (WINAPI *)(LPCOMMAND, CServer *, char *, int *, int))pCMDHooks[j].pProc)(&cmds_low[htons(wFreq)], server, zerobuf, &zerolen, 8);
				break;
			}
		}

		if (!bCmdFound)
		{
			((void (WINAPI *)(LPCOMMAND, CServer *, char *, int *, int))pCMDHooks[0].pProc)(&cmds_low[htons(wFreq)], server, zerobuf, &zerolen, 8);
		}
	}
	else
	{
		dprintf("[sendto] *** UNKNOWN FREQUENCY TYPE ***\n", s);
		int i, j;

		for (i = 0; i < len + 16; i += 16)
		{
			for (j = 0; j < 16; j++)
			{
				if ((i + j) < len)
				{
					dprintf("%02x ", (unsigned char)buf[i+j]);
				}
				else
				{
					dprintf("   ");
				}
			}
			
			for (j = 0; j < 16 && (i + j) < len; j++)
			{
				dprintf("%c", isprint(buf[i+j]) ? (unsigned char)buf[i+j] : '.');
			}

			dprintf("\n");
		}

		dprintf("\n");
	}

	if (bZerocoded && zerobuf)
		free(zerobuf);

	return nRes;
}

/*
LONG WINAPI new_SetWindowLongA(
	HWND hWnd,
	int nIndex,
	LONG dwNewLong
)
{
	LONG nRes = 0;

	if (nIndex == GWL_WNDPROC && g_pMainFrm && hWnd == g_pMainFrm->m_hWnd)
	{
		dprintf(_T("[SetWindowLongA] %08x [%x] = "), hWnd, dwNewLong);

		nRes = ((LONG (WINAPI *)(HWND, int, LONG))pAPIHooks[APIHOOK_SETWINDOWLONGA].pOldProc)(hWnd, nIndex, dwNewLong);

		dprintf(_T("[%x]\n"), nRes);
	}
	else
	{
		if (nIndex == GWL_WNDPROC)
			dprintf(_T("[SetWindowLongA] %08x [%x]\n"), hWnd, dwNewLong);

		nRes = ((LONG (WINAPI *)(HWND, int, LONG))pAPIHooks[APIHOOK_SETWINDOWLONGA].pOldProc)(hWnd, nIndex, dwNewLong);
	}

	return nRes;
}

LONG WINAPI new_SetWindowLongW(
	HWND hWnd,
	int nIndex,
	LONG dwNewLong
)
{
	LONG nRes = 0;

	if (nIndex == GWL_WNDPROC)
	{
		dprintf(_T("[SetWindowLongW] %08x\n"), hWnd);

		nRes = ((LONG (WINAPI *)(HWND, int, LONG))pAPIHooks[APIHOOK_SETWINDOWLONGW].pOldProc)(hWnd, nIndex, dwNewLong);
	}
	else
	{
		nRes = ((LONG (WINAPI *)(HWND, int, LONG))pAPIHooks[APIHOOK_SETWINDOWLONGW].pOldProc)(hWnd, nIndex, dwNewLong);
	}

	return nRes;
}
*/
BOOL WINAPI new_InternetReadFile(
    IN HINTERNET hFile,
    IN LPVOID lpBuffer,
    IN DWORD dwNumberOfBytesToRead,
    OUT LPDWORD lpNumberOfBytesRead
)
{
	BOOL bRes = FALSE;

	bRes = ((BOOL (WINAPI *)(HINTERNET, LPVOID, DWORD, LPDWORD))pAPIHooks[APIHOOK_INTERNETREADFILE].pOldProc)(hFile, lpBuffer, dwNumberOfBytesToRead, lpNumberOfBytesRead);

	dprintf(_T("[InternetReadFile] [%s] %d\n"), lpBuffer, *lpNumberOfBytesRead);

	return bRes;
}

HINTERNET WINAPI new_InternetOpenUrlA(
  HINTERNET hInternet,
  LPCTSTR lpszUrl,
  LPCTSTR lpszHeaders,
  DWORD dwHeadersLength,
  DWORD dwFlags,
  DWORD_PTR dwContext
)
{
	HINTERNET hNet = 0;

	dprintf(_T("[InternetOpenUrlA] [%s][%s]\n"), lpszUrl, lpszHeaders);

	hNet = ((HINTERNET (WINAPI *)(HINTERNET, LPCTSTR, LPCTSTR, DWORD, DWORD, DWORD_PTR))pAPIHooks[APIHOOK_INTERNETOPENURLA].pOldProc)(hInternet, lpszUrl, lpszHeaders, dwHeadersLength, dwFlags, dwContext);

	return hNet;
}

BOOL WINAPI new_CPEncrypt(
  HCRYPTPROV hProv,
  HCRYPTKEY hKey,
  HCRYPTHASH hHash,
  BOOL Final,
  DWORD dwFlags,
  BYTE* pbData,
  DWORD* pdwDataLen,
  DWORD dwBufLen
)
{
	BOOL bRes = FALSE;

	bRes = ((BOOL (WINAPI *)(HCRYPTPROV, HCRYPTKEY, HCRYPTHASH, BOOL, DWORD, BYTE *, DWORD *, DWORD))pAPIHooks[APIHOOK_CPENCRYPT].pOldProc)(hProv, hKey, hHash, Final, dwFlags, pbData, pdwDataLen, dwBufLen);

	TCHAR szStr[2048];
	ZeroMemory(szStr, sizeof(szStr));
	memcpy(szStr, pbData, sizeof(szStr));

	dprintf(_T("[CPEncrypt] [%s] %d\n"), szStr, 0);

	return bRes;
}

BOOL WINAPI new_PeekMessageA(
  LPMSG lpMsg,         // pointer to structure for message
  HWND hWnd,           // handle to window
  UINT wMsgFilterMin,  // first message
  UINT wMsgFilterMax,  // last message
  UINT wRemoveMsg      // removal flags
)
{
	//dprintf(_T("[PeekMessageA] [%x]\n"), hWnd);

	BOOL bRes = FALSE;
	static UINT uiAntiIdle = 0;

	bRes = ((BOOL (WINAPI *)(LPMSG, HWND, UINT, UINT, UINT))pAPIHooks[APIHOOK_PEEKMESSAGEA].pOldProc)(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);

	if (!bRes && GetFocus() == NULL)
	{
		// Wait for any message sent or posted to this queue 
		DWORD dwResult = MsgWaitForMultipleObjects(0, NULL, FALSE, 1000, QS_ALLINPUT);

		uiAntiIdle++;

		if (uiAntiIdle > 4)
		{
			//dprintf(_T("idle.. %d\n"), uiAntiIdle);
			if (g_pMainFrm)
			{
				//dprintf(_T("sending anti.. idle...\n"));
				PostMessage(g_pMainFrm->m_hWnd, WM_MOUSEMOVE, 0, MAKEWORD(10, 10));
			}

			uiAntiIdle = 0;
		}
	}

	static UINT uiAutoPilot = 0;

	if (g_pMainFrm)
	{
		//uiAutoPilot++;

		if (uiAutoPilot > 200)
		{
			WORD wRepeatCount = 1;
			LPARAM lParam = MAKELPARAM(wRepeatCount, 0);
			
			lParam |= 1 << 24;
			lParam |= 0 << 29;
			lParam |= 1 << 30;
			lParam |= 0 << 31;

			//PostMessage(g_pMainFrm->m_hWnd, WM_KEYDOWN, VK_UP, lParam);

			lParam |= 0 << 24;
			//PostMessage(g_pMainFrm->m_hWnd, WM_KEYDOWN, 'E', 0);
			
			char sc = 0;
			sc = MapVirtualKey('E', 0);
				
			for (int k = 0; k < 90; k++)
				PostMessage(g_pMainFrm->m_hWnd, WM_KEYDOWN, 'E', 1 | (1 << 30) | (sc << 16));

			//PostMessage(g_pMainFrm->m_hWnd, WM_KEYUP, 'E', 1 | (3 << 30) | (sc << 16));

			uiAutoPilot = 0;
		}
	}

	return bRes;
}

VOID WINAPI new_glEnable(
	GLenum cap
)
{
	dprintf(_T("[glEnable] [0x%08x]\n"), cap);

//	if (cap != GL_TEXTURE_2D)
		((VOID (WINAPI *)(GLenum))pAPIHooks[APIHOOK_GLENABLE].pOldProc)(cap);
//	else
//		glDisable(GL_TEXTURE_2D);
}

GLboolean WINAPI new_glIsEnabled(
	GLenum cap   
)
{
//	if (cap == GL_TEXTURE_2D)
//		return GL_TRUE;

	return ((GLboolean (WINAPI *)(GLenum))pAPIHooks[APIHOOK_GLISENABLED].pOldProc)(cap);
}

VOID WINAPI new_glTranslatef(
  GLfloat x, 
  GLfloat y, 
  GLfloat z  
)
{
	dprintf(_T("[glTranslatef]\n"));
	((VOID (WINAPI *)(GLfloat, GLfloat, GLfloat))pAPIHooks[APIHOOK_GLTRANSLATEF].pOldProc)(x, y, z);
}

VOID WINAPI new_glTranslated(
  GLdouble x, 
  GLdouble y, 
  GLdouble z  
)
{
	dprintf(_T("[glTranslated]\n"));
	((VOID (WINAPI *)(GLdouble, GLdouble, GLdouble))pAPIHooks[APIHOOK_GLTRANSLATED].pOldProc)(x, y, z);
}

VOID WINAPI new_glTexCoord2f(
	GLfloat s, 
	GLfloat t  
)
{
	((VOID (WINAPI *)(GLfloat, GLfloat))pAPIHooks[APIHOOK_GLTEXCOORD2F].pOldProc)(s, t);
}

VOID WINAPI new_glVertex2f(
  GLfloat x, 
  GLfloat y
)
{
	dprintf("[glVertex2f]\n");
	((VOID (WINAPI *)(GLfloat, GLfloat))pAPIHooks[APIHOOK_GLVERTEX2F].pOldProc)(x, y);
}

VOID WINAPI new_glVertex3f(
  GLfloat x, 
  GLfloat y, 
  GLfloat z  
)
{
	dprintf("[glVertex3f]\n");
	((VOID (WINAPI *)(GLfloat, GLfloat, GLfloat))pAPIHooks[APIHOOK_GLVERTEX3F].pOldProc)(x, y, z);
}

// Moves floating name tags
VOID WINAPI new_glVertex3fv(
  GLfloat *v
)
{
	dprintf("[glVertex3fv]\n");
	((VOID (WINAPI *)(const GLfloat *v))pAPIHooks[APIHOOK_GLVERTEX3FV].pOldProc)(v);
}

VOID WINAPI new_glVertex4f(
  GLfloat x, 
  GLfloat y, 
  GLfloat z,  
  GLfloat w
)
{
	dprintf("[glVertex4f]\n");
	((VOID (WINAPI *)(GLfloat, GLfloat, GLfloat, GLfloat))pAPIHooks[APIHOOK_GLVERTEX4F].pOldProc)(x, y, z, w);
}

VOID WINAPI new_glVertex4fv(
  GLfloat *v
)
{
	dprintf("[glVertex4v]\n");
	((VOID (WINAPI *)(const GLfloat *v))pAPIHooks[APIHOOK_GLVERTEX4FV].pOldProc)(v);
}

VOID WINAPI new_glTexCoord2fv(
	GLfloat *v
)
{
	((VOID (WINAPI *)(GLfloat *))pAPIHooks[APIHOOK_GLTEXCOORD2FV].pOldProc)(v);
}

VOID WINAPI new_glColor4f(
	GLfloat red,    
	GLfloat green,  
	GLfloat blue,   
	GLfloat alpha   
)
{
	((VOID (WINAPI *)(GLfloat, GLfloat, GLfloat, GLfloat))pAPIHooks[APIHOOK_GLCOLOR4F].pOldProc)(red, green, blue, alpha);
}

VOID WINAPI new_glColor3f(
	GLfloat red,    
	GLfloat green,  
	GLfloat blue
)
{
	GLfloat alpha;
	alpha = 1.0f;

	((VOID (WINAPI *)(GLfloat, GLfloat, GLfloat, GLfloat))pAPIHooks[APIHOOK_GLCOLOR4F].pOldProc)(red, green, blue, alpha);
}

VOID WINAPI new_glColor4fv(
	GLfloat *v
)
{
	((VOID (WINAPI *)(const GLfloat *))pAPIHooks[APIHOOK_GLCOLOR4FV].pOldProc)(v);
}

VOID WINAPI new_glColor4ubv(
	GLubyte *v
)
{
	((VOID (WINAPI *)(const GLubyte *))pAPIHooks[APIHOOK_GLCOLOR4UBV].pOldProc)(v);
}

VOID WINAPI new_glColor3fv(
	GLfloat *v
)
{
	GLfloat n[4];
	n[0] = v[0];
	n[1] = v[1];
	n[2] = v[2];
	n[3] = 1.0;

	((VOID (WINAPI *)(const GLfloat *))pAPIHooks[APIHOOK_GLCOLOR4FV].pOldProc)(n);
}

GLenum WINAPI new_glGetError(
	void
)
{
	dprintf("[glGetError] ");
	GLenum error = ((GLenum (WINAPI *)(void))pAPIHooks[APIHOOK_GLGETERROR].pOldProc)();

	dprintf("0x%x\n", error);
	if (error == GL_INVALID_OPERATION)
		return GL_NO_ERROR;
	else return error;
}

VOID WINAPI new_glColorPointer(
	GLint size,             
	GLenum type,            
	GLsizei stride,         
	const GLvoid *pointer   
)
{
	dprintf("[glColorPointer]\n");
	((VOID (WINAPI *)(GLint, GLenum, GLsizei, const GLvoid *))pAPIHooks[APIHOOK_GLCOLORPOINTER].pOldProc)(size, type, stride, pointer);
}

// Used to render avatars and trees
VOID WINAPI new_glDrawElements(
	GLenum mode,
	GLsizei count,
	GLenum type,
	const GLvoid *indices
)
{
	//glColor4f(1.0, 1.0, 1.0, 0.2);
	/*glBegin(GL_QUADS);
	for (int i = 0; i < 10; i++)
	{
		GLfloat fPosX;
		GLfloat fPosY;
		GLfloat fPosZ;

		fPosX = (float)(rand() % 300) - 150.0f;
		fPosY = -10;//(float)(rand() % 200) - 10.0f;
		fPosZ = (float)(rand() % 300) - 150.0f;

		glTexCoord2f(0.0, 1.0);
		glVertex3f(fPosX - 4.0f, fPosY + 1.0f, fPosZ - 4.0f);

		glTexCoord2f(0.0, 0.0);
		glVertex3f(fPosX - 4.0f, fPosY + 1.0f, fPosZ + 4.0f);

		glTexCoord2f(1.0, 0.0);
		glVertex3f(fPosX + 4.0f, fPosY + 1.0f, fPosZ + 4.0f);

		glTexCoord2f(1.0, 1.0);
		glVertex3f(fPosX + 4.0f, fPosY + 1.0f, fPosZ - 4.0f);

	}
	glEnd();*/

	dprintf("[glDrawElements] %d\n", count);
	((VOID (WINAPI *)(GLenum, GLsizei, GLenum, const GLvoid *))pAPIHooks[APIHOOK_GLDRAWELEMENTS].pOldProc)(mode, count, type, indices);
}

VOID WINAPI new_glDrawArrays(
	GLenum mode,   
	GLint first,   
	GLsizei count  
)
{
	dprintf("[glDrawArrays] %d\n", count);
	((VOID (WINAPI *)(GLenum, GLint, GLsizei))pAPIHooks[APIHOOK_GLDRAWARRAYS].pOldProc)(mode, first, count);
}

VOID WINAPI new_glDrawPixels(
	GLsizei width,         
	GLsizei height,        
	GLenum format,         
	GLenum type,           
	const GLvoid *pixels   
)
{
	dprintf("[glDrawPixels]\n");
	((VOID (WINAPI *)(GLsizei, GLsizei, GLenum, GLenum, const GLvoid))pAPIHooks[APIHOOK_GLDRAWPIXELS].pOldProc)(width, height, format, type, pixels);
}

VOID WINAPI new_glVertexPointer(
	GLint size,             
	GLenum type,            
	GLsizei stride,         
	const GLvoid *pointer   
)
{
	((VOID (WINAPI *)(GLint, GLenum, GLsizei, const GLvoid *))pAPIHooks[APIHOOK_GLVERTEXPOINTER].pOldProc)(size, type, stride, pointer);
}

VOID WINAPI new_glNormalPointer(
	GLenum type,            
	GLsizei stride,         
	const GLvoid *pointer   
)
{
	dprintf("[glNormalPointer]\n");
	((VOID (WINAPI *)(GLenum, GLsizei, const GLvoid *))pAPIHooks[APIHOOK_GLNORMALPOINTER].pOldProc)(type, stride, pointer);
}

VOID WINAPI new_glTexCoordPointer(
	GLint size,             
	GLenum type,            
	GLsizei stride,         
	const GLvoid *pointer   
)
{
	((VOID (WINAPI *)(GLint, GLenum, GLsizei, const GLvoid *))pAPIHooks[APIHOOK_GLTEXCOORDPOINTER].pOldProc)(size, type, stride, pointer);
}

VOID WINAPI new_glViewport(
	GLint x,        
	GLint y,        
	GLsizei width,  
	GLsizei height  
)
{
	((VOID (WINAPI *)(GLint, GLint, GLsizei, GLsizei))pAPIHooks[APIHOOK_GLVIEWPORT].pOldProc)(x, y, width, height);
}

VOID WINAPI new_glTexImage2D(
	GLenum target,        
	GLint level,          
	GLint components,     
	GLsizei width,        
	GLsizei height,       
	GLint border,         
	GLenum format,        
	GLenum type,          
	const GLvoid *pixels  
)
{
	((VOID (WINAPI *)(GLenum, GLint, GLint, GLsizei, GLsizei, GLint, GLenum, GLenum, const GLvoid *))pAPIHooks[APIHOOK_GLTEXIMAGE2D].pOldProc)(target, level, components, width, height, border, format, type, pixels);
}

VOID WINAPI new_gluPerspective(
  GLdouble fovy,    
  GLdouble aspect,  
  GLdouble zNear,   
  GLdouble zFar     
)
{
	((VOID (WINAPI *)(GLdouble, GLdouble, GLdouble, GLdouble))pAPIHooks[APIHOOK_GLUPERSPECTIVE].pOldProc)(fovy, aspect, zNear, zFar);
}

VOID WINAPI new_glBegin(
	GLenum mode
)
{
	((VOID (WINAPI *)(GLenum))pAPIHooks[APIHOOK_GLBEGIN].pOldProc)(mode);
}

VOID WINAPI new_glFlush(
	void
)
{
	((VOID (WINAPI *)(void))pAPIHooks[APIHOOK_GLFLUSH].pOldProc)();
}

VOID WINAPI new_gluQuadricDrawStyle(
	GLUquadricObj * qobj,   
	GLenum drawStyle        
	)
{
	((VOID (WINAPI *)(GLUquadricObj *, GLenum))pAPIHooks[APIHOOK_GLUQUADRICDRAWSTYLE].pOldProc)(qobj, drawStyle);
}

VOID WINAPI new_gluTessVertex(
	GLUtesselator * tess,   
	GLdouble coords[3],     
	void * data             
)
{
	dprintf("[gluTessVertex]\n");
	((VOID (WINAPI *)(GLUtesselator *, GLdouble[3], void *))pAPIHooks[APIHOOK_GLUTESSVERTEX].pOldProc)(tess, coords, data);
}

BOOL WINAPI new_WriteFileA(
	HANDLE hFile,                    // handle to file to write to
	LPCVOID lpBuffer,                // pointer to data to write to file
	DWORD nNumberOfBytesToWrite,     // number of bytes to write
	LPDWORD lpNumberOfBytesWritten,  // pointer to number of bytes written
	LPOVERLAPPED lpOverlapped        // pointer to structure for overlapped I/O
)
{
	return ((BOOL (WINAPI *)(HANDLE, LPCVOID, DWORD, LPDWORD, LPOVERLAPPED))pAPIHooks[APIHOOK_WRITEFILEA].pOldProc)(hFile, lpBuffer, nNumberOfBytesToWrite, lpNumberOfBytesWritten, lpOverlapped);
}

BOOL WINAPI new_DeleteFileA(
	LPCTSTR lpFileName   // pointer to name of file to delete
)
{
	dprintf("[DeleteFileA] %s\n", lpFileName);

	//return TRUE;
	return ((BOOL (WINAPI *)(LPCTSTR))pAPIHooks[APIHOOK_DELETEFILEA].pOldProc)(lpFileName);
}

int WINAPI new_lstrcmpiA(
	LPCTSTR lpString1,  // pointer to first string
	LPCTSTR lpString2   // pointer to second string
)
{
	//dprintf("[lstrcmpiA] %s <> %s\n", lpString1, lpString2);
	return ((int (WINAPI *)(LPCTSTR, LPCTSTR))pAPIHooks[APIHOOK_LSTRCMPIA].pOldProc)(lpString1, lpString2);
}

APIHOOK	pAPIHooks[] = {
	{ NULL,					_T("KERNEL32.DLL"),		"GetProcAddress",			FALSE,	NULL,	(PROC)new_GetProcAddress		},
	{ NULL,					_T("KERNEL32.DLL"),		"LoadLibraryA",				FALSE,	NULL,	(PROC)new_LoadLibraryA			},
	{ NULL,					_T("KERNEL32.DLL"),		"LoadLibraryW",				FALSE,	NULL,	(PROC)new_LoadLibraryW			},
	{ NULL,					_T("KERNEL32.DLL"),		"LoadLibraryExA",			FALSE,	NULL,	(PROC)new_LoadLibraryExA		},
	{ NULL,					_T("KERNEL32.DLL"),		"LoadLibraryExW",			FALSE,	NULL,	(PROC)new_LoadLibraryExW		},
	{ NULL,					_T("KERNEL32.DLL"),		"FreeLibraryA",				FALSE,	NULL,	(PROC)new_FreeLibraryA			},
	{ NULL,					_T("KERNEL32.DLL"),		"DeleteFileA",				FALSE,	NULL,	(PROC)new_DeleteFileA			},
	{ NULL,					_T("XKERNEL32.DLL"),	"WriteFileA",				FALSE,	NULL,	(PROC)new_WriteFileA			},
	{ NULL,					_T("XKERNEL32.DLL"),	"lstrcmpiA",				FALSE,	NULL,	(PROC)new_lstrcmpiA				},
	{ NULL,					_T("WS2_32.DLL"),		(LPCSTR)4,					TRUE,	NULL,	(PROC)new_connect				},
	{ NULL,					_T("WS2_32.DLL"),		(LPCSTR)16,					TRUE,	NULL,	(PROC)new_recv					},
	{ NULL,					_T("WS2_32.DLL"),		(LPCSTR)17,					TRUE,	NULL,	(PROC)new_recvfrom				},
	{ NULL,					_T("WS2_32.DLL"),		(LPCSTR)19,					TRUE,	NULL,	(PROC)new_send					},
	{ NULL,					_T("WS2_32.DLL"),		(LPCSTR)20,					TRUE,	NULL,	(PROC)new_sendto				},
	{ NULL,					_T("WS2_32.DLL"),		(LPCSTR)52,					TRUE,	NULL,	(PROC)new_gethostbyname			},
//	{ NULL,					_T("USER32.DLL"),		"SetWindowLongA",			FALSE,	NULL,	(PROC)new_SetWindowLongA		},
//	{ NULL,					_T("USER32.DLL"),		"SetWindowLongW",			FALSE,	NULL,	(PROC)new_SetWindowLongW		},
	{ NULL,					_T("WININET.DLL"),		"InternetReadFile",			FALSE,	NULL,	(PROC)new_InternetReadFile		},
	{ NULL,					_T("WININET.DLL"),		"InternetOpenUrlA",			FALSE,	NULL,	(PROC)new_InternetOpenUrlA		},
	{ NULL,					_T("RSAENH.DLL"),		"CPEncrypt",				FALSE,	NULL,	(PROC)new_CPEncrypt				},
	{ NULL,					_T("USER32.DLL"),		"PeekMessageA",				FALSE,	NULL,	(PROC)new_PeekMessageA			},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glGetError",				FALSE,	NULL,	(PROC)new_glGetError			},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glBegin",					FALSE,	NULL,	(PROC)new_glBegin				},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glEnable",					FALSE,	NULL,	(PROC)new_glEnable				},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glIsEnabled",				FALSE,	NULL,	(PROC)new_glIsEnabled			},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glTranslated",				FALSE,	NULL,	(PROC)new_glTranslated			},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glTranslatef",				FALSE,	NULL,	(PROC)new_glTranslatef			},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glTexCoord2f",				FALSE,	NULL,	(PROC)new_glTexCoord2f			},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glTexCoord2fv",			FALSE,	NULL,	(PROC)new_glTexCoord2fv			},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glVertex2f",				FALSE,	NULL,	(PROC)new_glVertex2f			},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glVertex3f",				FALSE,	NULL,	(PROC)new_glVertex3f			},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glVertex3fv",				FALSE,	NULL,	(PROC)new_glVertex3fv			},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glVertex4f",				FALSE,	NULL,	(PROC)new_glVertex4f			},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glVertex4fv",				FALSE,	NULL,	(PROC)new_glVertex4fv			},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glVertexPointer",			FALSE,	NULL,	(PROC)new_glVertexPointer		},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glTexCoordPointer",		FALSE,	NULL,	(PROC)new_glTexCoordPointer		},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glNormalPointer",			FALSE,	NULL,	(PROC)new_glNormalPointer		},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glDrawElements",			FALSE,	NULL,	(PROC)new_glDrawElements		},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glDrawArrays",				FALSE,	NULL,	(PROC)new_glDrawArrays			},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glDrawPixels",				FALSE,	NULL,	(PROC)new_glDrawPixels			},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glTexImage2D",				FALSE,	NULL,	(PROC)new_glTexImage2D			},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glColor3f",				FALSE,	NULL,	(PROC)new_glColor3f				},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glColor3fv",				FALSE,	NULL,	(PROC)new_glColor3fv			},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glColor4f",				FALSE,	NULL,	(PROC)new_glColor4f				},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glColor4fv",				FALSE,	NULL,	(PROC)new_glColor4fv			},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glColor4ubv",				FALSE,	NULL,	(PROC)new_glColor4ubv			},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glColorPointer",			FALSE,	NULL,	(PROC)new_glColorPointer		},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glViewport",				FALSE,	NULL,	(PROC)new_glViewport			},
//	{ NULL,					_T("UNCOMMENT"/*"OPENGL32.DLL"*/),		"glFlush",					FALSE,	NULL,	(PROC)new_glFlush				},
//	{ NULL,					_T("UNCOMMENT"/*"GLU32.DLL"*/),		"gluPerspective",			FALSE,	NULL,	(PROC)new_gluPerspective		},
//	{ NULL,					_T("UNCOMMENT"/*"GLU32.DLL"*/),		"gluQuadricDrawStyle",		FALSE,	NULL,	(PROC)new_gluQuadricDrawStyle	},
//	{ NULL,					_T("UNCOMMENT"/*"GLU32.DLL"*/),		"gluTessVertex",			FALSE,	NULL,	(PROC)new_gluTessVertex			},
	{ NULL,					NULL,					NULL,						FALSE,	NULL,	NULL							}
};

PROC HookModuleImport(HMODULE hModule, LPCTSTR szModule, LPCSTR szImport, PROC pNewProc, BOOL bOrdinal)
{
	PIMAGE_DOS_HEADER pDOSHeader = (PIMAGE_DOS_HEADER)hModule;

	if (IsBadReadPtr(pDOSHeader, sizeof(IMAGE_DOS_HEADER)) || pDOSHeader->e_magic != IMAGE_DOS_SIGNATURE)
	{
		SetLastErrorEx(ERROR_INVALID_PARAMETER, SLE_ERROR);
		return NULL;
	}

	PIMAGE_NT_HEADERS pNTHeader = MAKEPTR (PIMAGE_NT_HEADERS, pDOSHeader, pDOSHeader->e_lfanew);
	if (IsBadReadPtr(pNTHeader, sizeof(IMAGE_NT_HEADERS)) || pNTHeader->Signature != IMAGE_NT_SIGNATURE)
	{
		SetLastErrorEx(ERROR_INVALID_PARAMETER, SLE_ERROR);
		return NULL;
	}

	if (pNTHeader->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].Size != 0)
	{
		PIMAGE_IMPORT_DESCRIPTOR pImportDesc = MAKEPTR(PIMAGE_IMPORT_DESCRIPTOR, pDOSHeader, pNTHeader->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress);
		while (pImportDesc && pImportDesc->Name != NULL)
		{
	        PTSTR szImportModule = MAKEPTR(PTSTR, pDOSHeader, pImportDesc->Name);
			if (szImportModule != NULL && _tcsicmp(szImportModule, szModule) == 0)
			{ 
				PIMAGE_THUNK_DATA pOrigThunk = MAKEPTR(PIMAGE_THUNK_DATA, hModule, pImportDesc->OriginalFirstThunk);
				PIMAGE_THUNK_DATA pRealThunk = MAKEPTR(PIMAGE_THUNK_DATA, hModule, pImportDesc->FirstThunk);
				while (pOrigThunk != NULL && pRealThunk != NULL && pOrigThunk->u1.Function != NULL && pRealThunk->u1.Function)
				{
			        PIMAGE_IMPORT_BY_NAME pByName = MAKEPTR(PIMAGE_IMPORT_BY_NAME, hModule, pOrigThunk->u1.AddressOfData);
	
					if ((!bOrdinal && IMAGE_ORDINAL_FLAG != (pOrigThunk->u1.Ordinal & IMAGE_ORDINAL_FLAG) && pByName->Name[0] != 0 && stricmp((char*)pByName->Name, szImport) == 0) || (bOrdinal && IMAGE_ORDINAL(pOrigThunk->u1.Ordinal) == (DWORD)szImport))
					{
						PROC pOldProc;

						/*
							Retrieve memory information
						*/
						MEMORY_BASIC_INFORMATION mbi;
						if (VirtualQuery(pRealThunk, &mbi, sizeof(MEMORY_BASIC_INFORMATION)))
						{
							/*
								Give ourselves write access
							*/
							if (VirtualProtect(mbi.BaseAddress, mbi.RegionSize, PAGE_READWRITE, &mbi.Protect))
							{
								/*
									Store old function pointer for return value
								*/
								pOldProc = (PROC)pRealThunk->u1.Function;
								/*
									Only patch import table if the new function is different than the old one
								*/
								if (pNewProc != NULL && pOldProc != pNewProc) pRealThunk->u1.Function = (DWORD)pNewProc;

								/*
									Restore old protection value
								*/
								DWORD dwOldProtect;
								VirtualProtect(mbi.BaseAddress, mbi.RegionSize, mbi.Protect, &dwOldProtect);
							}

//							TCHAR szModuleFileName[MAX_PATH];
//							GetModuleFileName(hModule, szModuleFileName, sizeof(szModuleFileName));
//							dprintf(_T("HOOKED [%s => %s]\n"), szImport, szModuleFileName);
//							return pOldProc;
						}
					}
					pOrigThunk++;
					pRealThunk++;
				}
			}
			pImportDesc++;
		}
	}

	return NULL;
}

///////////////////////////////////////////////////////////////////////////////////////////
//
//	Retrieve the loaded modules for the specified process
//
//		dwProccessID	Process ID of the process whose loaded module list you wish to retrive.
//						Specify 0 for the current process.
//
//		pModules		Pointer to an array of MODULEINFO structures to store the module list.
//						Can be NULL.
//
//		puiCount		Specify 0 to retrieve the module count otherwise the maximum number of
//						entries in the pModule array to return.
//
//	Return Value
//
//		Returns FALSE if failed
//
///////////////////////////////////////////////////////////////////////////////////////////
BOOL GetModuleList(DWORD dwProcessID, PMODULEENTRY32 pModules, LPUINT puiCount)
{
//	dprintf(_T("[GetModuleList]\n"));

	HANDLE hSnapshot;
	MODULEENTRY32	cME32;
	UINT	uiCount = 0;

	if (puiCount == NULL)
	{
		SetLastErrorEx(ERROR_INVALID_PARAMETER, SLE_ERROR);
		return FALSE;
	}

	hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, dwProcessID);
	if (hSnapshot == INVALID_HANDLE_VALUE)
	{
		return FALSE;
	}

	ZeroMemory(&cME32, sizeof(cME32));
	cME32.dwSize = sizeof(MODULEENTRY32);
	if (Module32First(hSnapshot, &cME32) == TRUE)
	{
		do
		{
			if (pModules != NULL && *puiCount)
			{
				_tcsncpy(pModules[uiCount].szModule, cME32.szModule, MAX_PATH);
				pModules[uiCount].hModule = cME32.hModule;
			}
			uiCount++;
		} while (Module32Next(hSnapshot, &cME32) == TRUE && (*puiCount == 0 || uiCount < *puiCount));
	}

	CloseHandle(hSnapshot);

	*puiCount = uiCount;

	return TRUE;
}

///////////////////////////////////////////////////////////////////////////////////////////
//
//	Save pointers to old functions
//
///////////////////////////////////////////////////////////////////////////////////////////
void SaveImportHooks()
{
//	dprintf(_T("[SaveImportHooks]\n"));

	INT i;

	for (i = 0; pAPIHooks[i].szModule; i++)
	{
		/*
			Only store pointers for any functions we haven't taken care of yet so this may be called multiple times
		*/
		if (pAPIHooks[i].pOldProc == NULL)
		{
			pAPIHooks[i].pOldProc = GetProcAddress(GetModuleHandle(pAPIHooks[i].szModule), (LPCSTR)pAPIHooks[i].szImport);
//			dprintf(_T("Saved import for %s(%s) at 0x%08X\n"), pAPIHooks[i].szModule, pAPIHooks[i].szImport, pAPIHooks[i].pOldProc);
		}
	}
}

///////////////////////////////////////////////////////////////////////////////////////////
//
//	Patch import table of all loaded modules
//
///////////////////////////////////////////////////////////////////////////////////////////
void InstallImportHooks()
{
//	dprintf(_T("[InstallImportHooks]\n"));

	/*
		Retrieve number of loaded modules
	*/
	UINT uiCount = 0;
	INT i, j;
	if (GetModuleList(0, NULL, &uiCount))
	{
		/*
			Allocate temporary table of loaded modules
		*/
		PMODULEENTRY32 pModules = (PMODULEENTRY32)GlobalAlloc(GMEM_FIXED | GMEM_ZEROINIT, uiCount*sizeof(MODULEENTRY32));
		if (pModules != NULL)
		{
			/*
				Retrieve table of loaded modules
			*/
			if (GetModuleList(0, pModules, &uiCount))
			{
				/*
					Loop through each module and patch its import table
				*/
				for (i = 0; i < (INT)uiCount; i++)
				{
					/*
						Loop through each API hook and patch import table for this module
						HookModuleImport() takes care of dupe checking
					*/
					for (j = 0; pAPIHooks[j].szModule; j++)
					{
						if (_tcsicmp(pModules[i].szModule, _T("SNOWFLAKE.DLL")) && _tcsicmp(pModules[i].szModule, _T("FMOD.DLL")) && /*_tcsicmp(pModules[i].szModule, _T("NEWVIEW.EXE")) &&*/ _tcsicmp(pModules[i].szModule, pAPIHooks[j].szModule) && pAPIHooks[j].pNewProc != NULL && !IsBadCodePtr(pAPIHooks[j].pNewProc))
						{
							HookModuleImport(pModules[i].hModule, pAPIHooks[j].szModule, pAPIHooks[j].szImport, pAPIHooks[j].pNewProc, pAPIHooks[j].bOrdinal);
						}
					}
				}
			}
			GlobalFree(pModules);
		}
	}
}

///////////////////////////////////////////////////////////////////////////////////////////
//
//	Removes the import table for all loaded modules back to the original saved functions
//
///////////////////////////////////////////////////////////////////////////////////////////
void RemoveImportHooks()
{
	dprintf(_T("[RemoveImportHooks]\n"));

	/*
		Retrieve number of loaded modules
	*/
	UINT uiCount = 0;
	INT i, j;
	if (GetModuleList(0, NULL, &uiCount))
	{
		/*
			Allocate temporary table of loaded modules
		*/
		PMODULEENTRY32 pModules = (PMODULEENTRY32)GlobalAlloc(GMEM_FIXED | GMEM_ZEROINIT, uiCount*sizeof(MODULEENTRY32));
		if (pModules != NULL)
		{
			/*
				Retrieve table of loaded modules
			*/
			if (GetModuleList(0, pModules, &uiCount))
			{
				/*
					Loop through each module and patch its import table
				*/
				for (i = 0; i < (INT)uiCount; i++)
				{
					/*
						Loop through each API hook and restore import table for this module
					*/
//					dprintf("RESTORE [%s]\n", pModules[i].szModule);
					for (j = 0; pAPIHooks[j].szModule; j++)
					{
						if (_tcsicmp(pModules[i].szModule, _T("SNOWFLAKE.DLL")) && /*_tcsicmp(pModules[i].szModule, _T("NEWVIEW.EXE")) &&*/ _tcsicmp(pModules[i].szModule, pAPIHooks[j].szModule) && pAPIHooks[j].pOldProc != NULL && !IsBadCodePtr(pAPIHooks[j].pOldProc))
						{
//							dprintf(_T("REMOVE [%s => %s]\n"), pAPIHooks[j].szImport, pModules[i].szModule);
							HookModuleImport(pModules[i].hModule, pAPIHooks[j].szModule, pAPIHooks[j].szImport, pAPIHooks[j].pOldProc, pAPIHooks[j].bOrdinal);
						}
					}
				}

				for (j = 0; pAPIHooks[j].szModule; j++)
				{
					pAPIHooks[j].pOldProc = NULL;
				}
			}
			GlobalFree(pModules);
		}
	}
}

BOOL APIENTRY DllMain( HANDLE hModule, 
                       DWORD  ul_reason_for_call, 
                       LPVOID lpReserved
					 )
{
	if (ul_reason_for_call == DLL_PROCESS_ATTACH)
	{
		TCHAR szPath[MAX_PATH];

		g_hinstDLL = (HINSTANCE)hModule;
		_Module.Init(NULL, g_hinstDLL);
		DisableThreadLibraryCalls(g_hinstDLL);

		if (GetModuleFileName(GetModuleHandle(0), szPath, MAX_PATH))
		{
			_tcsupr(szPath);

			if (_tcsstr(szPath, _T("\\SECONDLIFE.EXE")))
			{
				if (!g_pConfig)
					g_pConfig = new CConfig();

#ifdef ECHO
				AllocConsole();
				
				if (!fpLog)
					fpLog = fopen(g_pConfig->m_pSnowcrashTxtPath, "w");
#endif
				dprintf(_T("[snowflake] %s\n"), szPath);

				decomm();

				SaveImportHooks();
				InstallImportHooks();
			}
		}
	} else if (ul_reason_for_call == DLL_PROCESS_DETACH)
	{
		RemoveSLHooks();
		RemoveImportHooks();
#ifdef ECHO
		FreeConsole();
		
		if (fpLog)
		{
			fclose(fpLog);
			fpLog = NULL;
		}
#endif
		_Module.Term();
	}

	return TRUE;
}

LRESULT CALLBACK CBTHookProc(int nCode, WPARAM wParam, LPARAM lParam)
{
	switch(nCode)
	{
		case HCBT_ACTIVATE:
			{
				if (!g_pMainFrm)
				{
					TCHAR szClass[128];
					HWND hwnd = (HWND)wParam;

					if (GetClassName(hwnd, szClass, sizeof(szClass)))
					{
						if (_tcsicmp(szClass, _T("Second Life")) == 0)
						{
							TCHAR szTitle[256];
							GetWindowText(hwnd, szTitle, sizeof(szTitle));

							if (_tcsstr(szTitle, "Second Life"))
							{
								dprintf(_T(">>> [%s][%s]\n"), szClass, szTitle);
								InstallSLHooks(hwnd);
							}
						}
						else
							dprintf(_T(">>> [%s]\n"), szClass);
					}
				}
			}
			break;

		case HCBT_CREATEWND:
			{
				TCHAR szClass[128];
				HWND hwnd = (HWND)wParam;
				LPCBT_CREATEWND lpcbt = (LPCBT_CREATEWND)lParam;

				if (GetClassName(hwnd, szClass, sizeof(szClass)))
				{
					//dprintf(_T(">>> [%s]\n"), szClass);
					//if (_tcsicmp(szClass, _T("OpenGL")) == 0)
					//{
						//dprintf(_T("[snowflake] SL window created\n"));
						//InstallSLHooks(hwnd);
					//}
				}
			}
			break;

		case HCBT_DESTROYWND:
			{
				TCHAR szClass[128];
				HWND hwnd = (HWND)wParam;

				if (GetClassName(hwnd, szClass, sizeof(szClass)))
				{
					if (g_pMainFrm && g_pMainFrm->m_hWnd == hwnd)
					{
						dprintf(_T("[snowflake] SL window destroyed\n"));
						RemoveSLHooks();
					}
				}
			}
			break;
	}
	return ::CallNextHookEx(h_hCBTHook, nCode, wParam, lParam);
}

SNOWFLAKE_API bool InstallSystemHook(void)
{
//	dprintf(_T("[InstallSystemHook]\n"));

	if (h_hCBTHook == NULL)
	{
		h_hCBTHook = ::SetWindowsHookEx(WH_CBT, CBTHookProc, g_hinstDLL, 0);
	}

	return (h_hCBTHook != NULL);
}

SNOWFLAKE_API BOOL RemoveSystemHook()
{
	dprintf(_T("[RemoveSystemHook]\n"));

	if (h_hCBTHook != NULL)
	{
		if (UnhookWindowsHookEx(h_hCBTHook))
		{
			h_hCBTHook = NULL;

			return TRUE;
		}
	}

	return FALSE;
}

bool InstallSLHooks(HWND hwnd)
{
	_Module.AddMessageLoop(&g_msgLoop);

	if (g_pMainFrm == NULL)
	{
		g_pMainFrm = new CMainFrame();

		if (g_pMainFrm->SubclassWindow(hwnd))
		{
			return true;
		}
	}

	return false;
}

bool RemoveSLHooks()
{
	if (g_pMainFrm != NULL)
	{
		dprintf(_T("RemoveSLHooks MainFrm\n"));
		if (g_pMainFrm->m_hWnd != NULL && ::IsWindow(*g_pMainFrm))
		{
			dprintf(_T("RemoveSLHooks MainFrm Unsub\n"));
			g_pMainFrm->UnsubclassWindow(TRUE);
			g_pMainFrm->m_hWnd = NULL;
		} else
			g_pMainFrm->m_hWnd = NULL;
		delete(g_pMainFrm);
		g_pMainFrm = NULL;

		return true;
	}

	return false;
}
