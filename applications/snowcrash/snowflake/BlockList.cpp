#include "StdAfx.h"
#include ".\Blocklist.h"
#include ".\keywords.h"

CBlockList::CBlockList(void)
{
	int m_nType = 0;
	m_lpBlocks = NULL;
	m_lpszBlock = NULL;

	m_lpPrev = NULL;
	m_lpNext = NULL;
}

CBlockList::~CBlockList(void)
{
	SAFE_FREE(m_lpszBlock);
}

void CBlockList::FreeBlocks(void)
{
	CBlock *block = m_lpBlocks;

	while (block)
	{
		if (block->m_lpNext)
		{
			block = block->m_lpNext;

			if (block->m_lpPrev)
				SAFE_DELETE(block->m_lpPrev);
		}
		else
			SAFE_DELETE(block);
	}

	m_lpBlocks = NULL;
}

bool CBlockList::AddBlock(CBlock *lpBlock)
{
	CBlock *block = NULL;

	if (m_lpBlocks)
	{
		block = m_lpBlocks;

		while (block->m_lpNext)
			block = block->m_lpNext;

		block->m_lpNext = lpBlock;

		if (!block->m_lpNext) return false;

		block->m_lpNext->m_lpNext = NULL;
		block->m_lpNext->m_lpPrev = block;

		return true;
	}
	else
	{
		block = lpBlock;

		if (!block) return false;

		m_lpBlocks = block;
		m_lpBlocks->m_lpNext = NULL;
		m_lpBlocks->m_lpPrev = NULL;

		return true;
	}

	return false;
}


void CBlockList::SetBlock(char *lpszBlock)
{
	if (lpszBlock)
	{
		size_t stLen = strlen(lpszBlock);

		if (stLen > 0)
		{
			SAFE_FREE(m_lpszBlock);
			m_lpszBlock = (char *)malloc(stLen + 1);

			if (m_lpszBlock)
			{
				strncpy(m_lpszBlock, lpszBlock, stLen);
				m_lpszBlock[stLen] = '\0';
			}
		}
	}			
}

void CBlockList::SetType(int nType)
{
	m_nType = nType;
}

CBlock *CBlockList::GetBlock(int nIndex)
{
	CBlock *block = m_lpBlocks;
	int i = 0;

	while (block)
	{
		if (i == nIndex)
			return block;
		
		block = block->m_lpNext;
		i++;
	}

	return NULL;
}

int CBlockList::CountBlock(void)
{
	CBlock *block = m_lpBlocks;
	int nCount = 0;

	while (block)
	{
		nCount++;
		block = block->m_lpNext;
	}

	return nCount;
}

void CBlockList::Dump(void)
{
	if (m_lpszBlock)
		dprintf("%s\n", m_lpszBlock);

	CBlock *block = m_lpBlocks;

	while (block)
	{
		block->Dump();
		block = block->m_lpNext;
	}
}


int CBlockList::Pack(LPBYTE lpData)
{
	int nTotalWrote = 0;
	int nWrote = 0;
	LPBYTE lpPtr = lpData;
	CBlock *block = m_lpBlocks;

	if (m_nType == LLTYPE_VARIABLE)
	{
		BYTE cItems;

		cItems = CountBlock();

		memcpy(lpPtr, &cItems, sizeof(cItems));
		nTotalWrote = sizeof(cItems);
		lpPtr += sizeof(cItems);
	}

	while (block)
	{
		nWrote = block->Pack(lpPtr);
		lpPtr += nWrote;
		nTotalWrote += nWrote;
		block = block->m_lpNext;
	}

	return nTotalWrote;
}