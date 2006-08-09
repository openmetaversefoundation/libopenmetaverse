#pragma once

class CConfig  
{
public:
	CString m_pCommDatPath;
	CString m_pMessageTemplatePath;
	CString m_pSnowcrashTxtPath;

	unsigned int GetConfigInt(LPSTR lpSection, LPSTR lpSubKey, UINT iDefault);
	BOOL GetConfigBool(LPSTR lpSection, LPSTR lpSubKey, BOOL bDefault);
	void GetConfigString(LPSTR lpSection, LPSTR lpSubKey, LPSTR lpBuffer, UINT *len, LPSTR lpDefault);
	void LoadConfig();
	CConfig();
	virtual ~CConfig();

};
