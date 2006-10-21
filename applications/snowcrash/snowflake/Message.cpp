#include "StdAfx.h"
#include ".\Message.h"

CMessage::CMessage(void)
{
	m_lpBlocks = NULL;
	m_lpszCommand = NULL;
}

CMessage::~CMessage(void)
{
	FreeBlocks();
	SAFE_FREE(m_lpszCommand);
}

void CMessage::FreeBlocks(void)
{
	CBlockList *blocks = m_lpBlocks;

	while (blocks)
	{
		if (blocks->m_lpNext)
		{
			blocks = blocks->m_lpNext;

			if (blocks->m_lpPrev)
				SAFE_DELETE(blocks->m_lpPrev);
		}
		else
			SAFE_DELETE(blocks);
	}

	m_lpBlocks = NULL;
}

bool CMessage::AddBlock(char *lpszBlock, int nType, CBlock *lpBlock)
{
	CBlockList *blocks = FindBlock(lpszBlock);

	if (blocks)
	{
		return blocks->AddBlock(lpBlock);
	}
	else
	{
		if (m_lpBlocks)
		{
			blocks = m_lpBlocks;

			while (blocks->m_lpNext)
				blocks = blocks->m_lpNext;

			blocks->m_lpNext = new CBlockList;

			if (!blocks->m_lpNext) return false;

			blocks->m_lpNext->m_lpNext = NULL;
			blocks->m_lpNext->m_lpPrev = blocks;
			blocks->m_lpNext->SetBlock(lpszBlock);
			blocks->m_lpNext->SetType(nType);
			blocks->m_lpNext->AddBlock(lpBlock);

			return true;
		}
		else
		{
			blocks = new CBlockList;

			if (!blocks) return false;

			m_lpBlocks = blocks;
			m_lpBlocks->m_lpNext = NULL;
			m_lpBlocks->m_lpPrev = NULL;
			m_lpBlocks->SetBlock(lpszBlock);
			m_lpBlocks->SetType(nType);
			m_lpBlocks->AddBlock(lpBlock);

			return true;
		}

		return false;
	}
}

CBlock *CMessage::GetBlock(char *lpszBlock, int nIndex)
{
	CBlockList *blocks = m_lpBlocks;

	while (blocks)
	{
		if (blocks->m_lpszBlock && !stricmp(lpszBlock, blocks->m_lpszBlock))
		{
			CBlock *block = blocks->GetBlock(nIndex);
			return block;
		}
		
		blocks = blocks->m_lpNext;
	}

	return NULL;
}

CBlockList *CMessage::FindBlock(char *lpszBlock)
{
	CBlockList *blocks = m_lpBlocks;

	while (blocks)
	{
		if (blocks->m_lpszBlock && !stricmp(lpszBlock, blocks->m_lpszBlock))
			return blocks;
		
		blocks = blocks->m_lpNext;
	}

	return NULL;
}

int CMessage::CountBlock(char *lpszBlock)
{
	CBlockList *blocks = FindBlock(lpszBlock);

	if (blocks)
	{
		return blocks->CountBlock();
	}

	return 0;
}

bool CMessage::GetString(char *lpszBlock, int nIndex, char *lpszVar, char &lpszStr)
{
	CBlock *block = GetBlock(lpszBlock, nIndex);

	if (block)
	{
		CVar *var = block->FindVar(lpszVar);

		if (var)
		{
			var->GetString(lpszStr);

			return true;
		}
	}

	return false;
}

bool CMessage::GetBool(char *lpszBlock, int nIndex, char *lpszVar, bool &lpbBool)
{
	CBlock *block = GetBlock(lpszBlock, nIndex);

	if (block)
	{
		CVar *var = block->FindVar(lpszVar);

		if (var)
		{
			var->GetBool(lpbBool);

			return true;
		}
	}

	return false;
}

void CMessage::SetCommand(char *lpszCommand)
{
	if (lpszCommand)
	{
		size_t stLen = strlen(lpszCommand);

		if (stLen > 0)
		{
			SAFE_FREE(m_lpszCommand);
			m_lpszCommand = (char *)malloc(stLen + 1);

			if (m_lpszCommand)
			{
				strncpy(m_lpszCommand, lpszCommand, stLen);
				m_lpszCommand[stLen] = '\0';
			}
		}
	}			
}

void CMessage::Dump(void)
{
	if (m_lpszCommand)
		dprintf("----- %s -----\n", m_lpszCommand);

	CBlockList *blocks = m_lpBlocks;

	while (blocks)
	{
		blocks->Dump();
		blocks = blocks->m_lpNext;
	}
}

int CMessage::Pack(LPBYTE lpData)
{
	int nTotalWrote = 0;
	int nWrote = 0;
	LPBYTE lpPtr = lpData;
	CBlockList *blocks = m_lpBlocks;

	while (blocks)
	{
		nWrote = blocks->Pack(lpPtr);
		lpPtr += nWrote;
		nTotalWrote += nWrote;
		blocks = blocks->m_lpNext;
	}

	return nTotalWrote;
}