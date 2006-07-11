#include "StdAfx.h"
#include ".\SequenceList.h"

CSequenceList::CSequenceList(void)
{
	m_lpSequences = NULL;
}

CSequenceList::~CSequenceList(void)
{
	FreeSequences();
}

void CSequenceList::FreeSequences(void)
{
	CSequence *sequence = m_lpSequences;

	while (sequence)
	{
		if (sequence->m_lpNext)
		{
			sequence = sequence->m_lpNext;

			if (sequence->m_lpPrev)
				SAFE_DELETE(sequence->m_lpPrev);
		}
		else
			SAFE_DELETE(sequence);
	}

	m_lpSequences = NULL;
}

bool CSequenceList::AddSequence(CSequence *lpSequence)
{
	CSequence *sequence = NULL;

	if (m_lpSequences)
	{
		sequence = m_lpSequences;

		while (sequence->m_lpNext)
			sequence = sequence->m_lpNext;

		sequence->m_lpNext = lpSequence;

		if (!sequence->m_lpNext) return false;

		sequence->m_lpNext->m_lpNext = NULL;
		sequence->m_lpNext->m_lpPrev = sequence;

		return true;
	}
	else
	{
		sequence = lpSequence;

		if (!sequence) return false;

		m_lpSequences = sequence;
		m_lpSequences->m_lpNext = NULL;
		m_lpSequences->m_lpPrev = NULL;

		return true;
	}

	return false;
}

void CSequenceList::RemoveSequence(CSequence *lpSequence)
{
	if (lpSequence->m_lpNext)
	{
		if (lpSequence->m_lpPrev)
		{
			lpSequence->m_lpPrev->m_lpNext = lpSequence->m_lpNext;
			lpSequence->m_lpNext->m_lpPrev = lpSequence->m_lpPrev;
		}
		else
		{
			lpSequence->m_lpNext->m_lpPrev = NULL;
			m_lpSequences = lpSequence->m_lpNext;
		}
	}
	else
	{
		if (lpSequence->m_lpPrev)
		{
			lpSequence->m_lpPrev->m_lpNext = NULL;
		}
		else
		{
			m_lpSequences = NULL;
		}
	}

	SAFE_DELETE(lpSequence);
}

CSequence *CSequenceList::FindSequenceByKey(WORD wKey)
{
	CSequence *sequence = m_lpSequences;

	while (sequence)
	{
		if (sequence->m_wKey == wKey)
			return sequence;
		sequence = sequence->m_lpNext;
	}

	return NULL;
}

CSequence *CSequenceList::FindSequenceByValue(WORD wValue)
{
	CSequence *sequence = m_lpSequences;

	while (sequence)
	{
		if (sequence->m_wValue == wValue)
			return sequence;
		sequence = sequence->m_lpNext;
	}

	return NULL;
}

void CSequenceList::WalkSequences(void)
{
	CSequence *sequence = m_lpSequences;

	while (sequence)
	{
		dprintf("SEQ WALK: %hu ==> %hu\n", sequence->m_wKey, sequence->m_wValue);
		sequence = sequence->m_lpNext;
	}
}

BYTE CSequenceList::WriteAckedSequences(char *buf)
{
	CSequence *sequence = m_lpSequences;
	BYTE cAcked = 0;

	while (sequence)
	{
		if (sequence->m_wState == SEQ_STATE_ACKED)
		{
			DWORD dwID = (DWORD)sequence->m_wKey;
			dprintf("WROTE ACKED SEQ: %lu\n", dwID);
			memcpy(&buf[cAcked * sizeof(dwID)], &dwID, sizeof(dwID));
			cAcked++;
			CSequence *remove = sequence;
			sequence = sequence->m_lpNext;
			RemoveSequence(remove);
		}
		else
			sequence = sequence->m_lpNext;
	}

	return cAcked;
}