#include "StdAfx.h"
#include ".\ServerList.h"

CServerList::CServerList(void)
{
	m_lpServers = NULL;
	m_bAddedUserServer = false;
}

CServerList::~CServerList(void)
{
	FreeServers();
}

void CServerList::FreeServers(void)
{
	CServer *server = m_lpServers;

	while (server)
	{
		if (server->m_lpNext)
		{
			server = server->m_lpNext;

			if (server->m_lpPrev)
				SAFE_DELETE(server->m_lpPrev);
		}
		else
			SAFE_DELETE(server);
	}

	m_lpServers = NULL;
}

bool CServerList::AddServer(CServer *lpServer)
{
	CServer *server = NULL;

	if (m_lpServers)
	{
		server = m_lpServers;

		while (server->m_lpNext)
			server = server->m_lpNext;

		server->m_lpNext = lpServer;

		if (!server->m_lpNext) return false;

		server->m_lpNext->m_lpNext = NULL;
		server->m_lpNext->m_lpPrev = server;

		if (!m_bAddedUserServer)
		{
			m_bAddedUserServer = true;
			lpServer->m_nType = SERVER_TYPE_USER;
			lpServer->SetSimName("User Server");
		}

		return true;
	}
	else
	{
		server = lpServer;

		if (!server) return false;

		m_lpServers = server;
		m_lpServers->m_lpNext = NULL;
		m_lpServers->m_lpPrev = NULL;

		if (!m_bAddedUserServer)
		{
			m_bAddedUserServer = true;
			lpServer->m_nType = SERVER_TYPE_USER;
			lpServer->SetSimName("User Server");
		}

		return true;
	}

	return false;
}

CServer *CServerList::FindServer(struct sockaddr_in *address)
{
	CServer *server = m_lpServers;

	while (server)
	{
		if (!memcmp(&server->m_address, address, sizeof(server->m_address)))
			return server;
		server = server->m_lpNext;
	}

	return NULL;
}

CServer *CServerList::FindServer(int nType)
{
	CServer *server = m_lpServers;

	while (server)
	{
		if (server->m_nType == nType)
			return server;
		server = server->m_lpNext;
	}

	return NULL;
}