#pragma once

#include ".\SequenceList.h"

#define SERVER_TYPE_UNKNOWN		0
#define SERVER_TYPE_USER		1
#define SERVER_TYPE_DATA		2
#define SERVER_TYPE_SIMULATOR	3

class CServer
{
public:
	CServer(void);
	CServer(struct sockaddr_in *address);
	~CServer(void);
	void SetSimName(char *lpszSimName);

	struct sockaddr_in m_address;
	char *m_lpszSimName;
	int m_nType;
	ULONG m_ulX;
	ULONG m_ulY;
	ULONG m_ullHandle;
	
	CSequenceList m_sequencesSent;
	WORD m_wSequenceSentIndex;

	CSequenceList m_sequencesRecv;
	WORD m_wSequenceRecvIndex;

	CServer *m_lpNext;
	CServer *m_lpPrev;
};
