#pragma once

#include ".\Var.h"

class CBlock
{
public:
	CBlock(void);
	~CBlock(void);

	BYTE cItems;
	
	CVar *m_lpVars;

	CBlock *m_lpNext;
	CBlock *m_lpPrev;

	void FreeVars(void);
	bool AddVar(CVar *lpVar);
	CVar *FindVar(char *lpszVar);
	int CountVar(char *lpszVar);
	void Dump(void);
	int Pack(LPBYTE lpData);
};
