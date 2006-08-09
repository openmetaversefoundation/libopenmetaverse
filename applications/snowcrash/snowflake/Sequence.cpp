#include "StdAfx.h"
#include ".\Sequence.h"

CSequence::CSequence(void)
{
	m_wKey = 0;
	m_wValue = 0;
	m_wState = SEQ_STATE_UNACKED;

	m_lpPrev = NULL;
	m_lpNext = NULL;
}

CSequence::~CSequence(void)
{
}
