#pragma once

#define SEQ_STATE_UNACKED	0
#define SEQ_STATE_ACKED		1
#define SEQ_STATE_RESENT	2

class CSequence
{
public:
	CSequence(void);
	~CSequence(void);

	WORD m_wKey;
	WORD m_wValue;
	WORD m_wState;

	CSequence *m_lpPrev;
	CSequence *m_lpNext;
};
