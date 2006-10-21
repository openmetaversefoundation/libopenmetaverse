#include "StdAfx.h"
#include ".\Server.h"

CServer::CServer(void)
{
	ZeroMemory(&m_address, sizeof(m_address));
	m_lpszSimName = NULL;
	m_nType = SERVER_TYPE_UNKNOWN;
	m_ulX = 0;
	m_ulY = 0;
	m_ullHandle = 0;

	m_wSequenceSentIndex = 1;
	m_wSequenceRecvIndex = 1;

	m_lpPrev = NULL;
	m_lpNext = NULL;
}

CServer::CServer(struct sockaddr_in *address)
{
	memcpy(&m_address, address, sizeof(m_address));
	m_lpszSimName = NULL;
	m_nType = SERVER_TYPE_UNKNOWN;
	m_ulX = 0;
	m_ulY = 0;
	m_ullHandle = 0;

	m_wSequenceSentIndex = 1;
	m_wSequenceRecvIndex = 1;

	m_lpPrev = NULL;
	m_lpNext = NULL;
}

CServer::~CServer(void)
{
	SAFE_FREE(m_lpszSimName);
}

void CServer::SetSimName(char *lpszSimName)
{
	SAFE_FREE(m_lpszSimName);

	size_t stLen = strlen(lpszSimName);
	
	if (stLen > 0)
	{
		m_lpszSimName = (char *)malloc(stLen + 1);

		if (m_lpszSimName)
			memcpy(m_lpszSimName, lpszSimName, stLen + 1);
	}
}