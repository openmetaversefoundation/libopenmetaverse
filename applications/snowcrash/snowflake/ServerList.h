#pragma once

#include ".\Server.h"

class CServerList
{
public:
	CServerList(void);
	~CServerList(void);

	CServer	*m_lpServers;
	void FreeServers(void);
	bool AddServer(CServer *lpServer);
	CServer *FindServer(struct sockaddr_in *address);
	CServer *FindServer(int nType);
	bool m_bAddedUserServer;
};
