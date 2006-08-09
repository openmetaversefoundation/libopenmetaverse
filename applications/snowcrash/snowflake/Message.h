#pragma once

#include ".\BlockList.h"

class CMessage
{
public:
	CMessage(void);
	~CMessage(void);

	CBlockList	*m_lpBlocks;
	char *m_lpszCommand;
	
	void FreeBlocks(void);
	bool AddBlock(char *lpszBlock, int nType, CBlock *lpBlock);
	CBlockList *FindBlock(char *lpszBlock);
	CBlock *GetBlock(char *lpszBlock, int nIndex);
	int CountBlock(char *lpszBlock);
	void SetCommand(char *lpszCommand);
	void Dump(void);
	int Pack(LPBYTE lpData);
	bool GetString(char *lpszBlock, int nIndex, char *lpszVar, char &lpszStr);
	bool GetBool(char *lpszBlock, int nIndex, char *lpszVar, bool &lpbBool);
};

