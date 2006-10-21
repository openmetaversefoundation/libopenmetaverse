#pragma once

#include ".\Sequence.h"

class CSequenceList
{
public:
	CSequenceList(void);
	~CSequenceList(void);

	CSequence *m_lpSequences;
	void FreeSequences(void);
	bool AddSequence(CSequence *lpSequence);
	void RemoveSequence(CSequence *lpSequence);
	CSequence *FindSequenceByKey(WORD wKey);
	CSequence *FindSequenceByValue(WORD wValue);
	void WalkSequences(void);
	BYTE WriteAckedSequences(char *buf);
};
