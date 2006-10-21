#pragma once

#include ".\Block.h"

class CBlockList
{
public:
	CBlockList(void);
	~CBlockList(void);

	CBlock	*m_lpBlocks;
	char *m_lpszBlock;
	int m_nType;
	BYTE cItems;
	
	CBlockList *m_lpNext;
	CBlockList *m_lpPrev;

	void FreeBlocks(void);
	bool AddBlock(CBlock *lpBlock);
	CBlock *GetBlock(int nIndex = 0);
	int CountBlock(void);
	void Dump(void);
	int Pack(LPBYTE lpData);
	void SetBlock(char *lpszBlock);
	void SetType(int nType);
//	bool GetString(char *lpszBlock, int nIndex, char *lpszVar, char &lpszStr);	
};
