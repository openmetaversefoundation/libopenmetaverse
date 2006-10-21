#pragma once

class CVar
{
public:
	CVar(void);
	~CVar(void);

	char *m_lpszVar;
	int m_nType;
	int m_nTypeLen;
	int m_nLen;
	LPBYTE m_lpData;

	CVar *m_lpNext;
	CVar *m_lpPrev;
	void SetVar(char *lpszVar);
	void SetType(int nType, int nTypeLen = 0);
	int SetData(LPBYTE lpData);
	void GetString(char &lpszStr);
	void GetBool(bool &lpbBool);
	void Dump(void);
	int Pack(LPBYTE lpData);
};
