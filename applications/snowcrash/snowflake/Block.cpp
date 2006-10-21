#include "StdAfx.h"
#include ".\Block.h"
#include ".\keywords.h"

CBlock::CBlock(void)
{
	m_lpVars = NULL;

	m_lpPrev = NULL;
	m_lpNext = NULL;
}

CBlock::~CBlock(void)
{
}

void CBlock::FreeVars(void)
{
	CVar *var = m_lpVars;

	while (var)
	{
		if (var->m_lpNext)
		{
			var = var->m_lpNext;

			if (var->m_lpPrev)
				SAFE_DELETE(var->m_lpPrev);
		}
		else
			SAFE_DELETE(var);
	}

	m_lpVars = NULL;
}

bool CBlock::AddVar(CVar *lpVar)
{
	CVar *var = NULL;

	if (m_lpVars)
	{
		var = m_lpVars;

		while (var->m_lpNext)
			var = var->m_lpNext;

		var->m_lpNext = lpVar;

		if (!var->m_lpNext) return false;

		var->m_lpNext->m_lpNext = NULL;
		var->m_lpNext->m_lpPrev = var;

		return true;
	}
	else
	{
		var = lpVar;

		if (!var) return false;

		m_lpVars = var;
		m_lpVars->m_lpNext = NULL;
		m_lpVars->m_lpPrev = NULL;

		return true;
	}

	return false;
}

CVar *CBlock::FindVar(char *lpszVar)
{
	CVar *var= m_lpVars;

	while (var)
	{
		if (var->m_lpszVar && !stricmp(lpszVar, var->m_lpszVar))
			return var;
		
		var = var->m_lpNext;
	}

	return NULL;
}

int CBlock::CountVar(char *lpszVar)
{
	CVar *var= m_lpVars;
	int nCount = 0;

	while (var)
	{
		if (var->m_lpszVar && !stricmp(lpszVar, var->m_lpszVar))
			nCount++;
		
		var = var->m_lpNext;
	}

	return nCount;
}

void CBlock::Dump(void)
{
	CVar *var = m_lpVars;

	while (var)
	{
		var->Dump();
		var = var->m_lpNext;
	}
}

int CBlock::Pack(LPBYTE lpData)
{
	int nTotalWrote = 0;
	int nWrote = 0;
	LPBYTE lpPtr = lpData;
	CVar *var = m_lpVars;

	while (var)
	{
		nWrote = var->Pack(lpPtr);
		lpPtr += nWrote;
		nTotalWrote += nWrote;
		var = var->m_lpNext;
	}

	return nTotalWrote;
}