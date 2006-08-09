#include "StdAfx.h"
#include ".\Config.h"

CConfig::CConfig()
{
	LoadConfig();
}

CConfig::~CConfig()
{

}

void CConfig::LoadConfig()
{
	TCHAR szItem[1024];
	TCHAR lpszPath[MAX_PATH] = {0};

	HKEY hKey;
	if (RegOpenKey(HKEY_LOCAL_MACHINE, "SOFTWARE\\Linden Research, Inc.\\SecondLife", &hKey) == ERROR_SUCCESS)
	{
		DWORD dwType = REG_SZ, dwLength = sizeof(szItem);
		if (RegQueryValueEx(hKey, NULL, NULL, &dwType, (LPBYTE)&szItem, &dwLength) == ERROR_SUCCESS)
		{
			::PathCombine(lpszPath, szItem, "app_settings\\comm.dat");
			m_pCommDatPath = lpszPath;
			
			::PathCombine(lpszPath, szItem, "app_settings\\message_template.msg");
			m_pMessageTemplatePath = lpszPath;

			::PathCombine(lpszPath, szItem, "snowcrash.txt");
			m_pSnowcrashTxtPath = lpszPath;
		}
		RegCloseKey(hKey);
	}
}

void CConfig::GetConfigString(LPSTR lpSection, LPSTR lpSubKey, LPSTR lpBuffer, UINT *len, LPSTR lpDefault)
{
	HKEY hKey;
	char szPath[256];

	strncpy(lpBuffer, lpDefault, *len);
	wsprintf(szPath, "Software\\thirty4 interactive\\Snowcrash\\%s", lpSection);
	if (RegOpenKeyEx(HKEY_CURRENT_USER, szPath, 0, KEY_QUERY_VALUE, &hKey) == ERROR_SUCCESS)
	{
		RegQueryValueEx(hKey, lpSubKey, NULL, NULL, (BYTE *)lpBuffer, (unsigned long *)len);
		RegCloseKey(hKey);
	}
}

BOOL CConfig::GetConfigBool(LPSTR lpSection, LPSTR lpSubKey, BOOL bDefault)
{
	char szBuffer[256];
	UINT len;
	len = sizeof(szBuffer);
	ZeroMemory(&szBuffer, sizeof(szBuffer));
	GetConfigString(lpSection, lpSubKey, szBuffer, &len, bDefault==TRUE?"Yes":"No");
	return stricmp(szBuffer, "Yes")==0?TRUE:FALSE;
}

unsigned int CConfig::GetConfigInt(LPSTR lpSection, LPSTR lpSubKey, UINT iDefault)
{
	char szBuffer[256], szDefault[20];
	UINT len;
	len = sizeof(szBuffer);
	ZeroMemory(&szBuffer, sizeof(szBuffer));
	ZeroMemory(&szDefault, sizeof(szDefault));
	wsprintf(szDefault, "%d", iDefault);
	GetConfigString(lpSection, lpSubKey, szBuffer, &len, szDefault);

	return atoi(szBuffer);
}
