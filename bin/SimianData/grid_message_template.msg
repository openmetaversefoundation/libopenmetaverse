%%enable_client

->
{
	agent_id: uuid,
	session_id: uuid,
	secure_session_id: uuid,
	circuit_code: integer,
	first_name: string,
	last_name: string,
	callback_uri: uri
}

<-
{
	success: boolean,
	message: string
}

%%enable_client_complete

->
{
	agent_id: uuid,
	seed_capability: uri
}

<-
{
	success: boolean,
	message: string
}
