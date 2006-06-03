#include "PacketBuilder.h"

PacketPtr PacketAck(ProtocolManager* protocol, std::vector<unsigned int> IDList)
{
	PacketPtr packet(new Packet("PacketAck", protocol));
	size_t length = IDList.size() * 4 + 1;
	byte* bytePtr = (byte*)malloc(length);
	std::vector<unsigned int>::iterator ID;
	unsigned int currentID;
	size_t pos = 1;

	bytePtr[0] = (byte)IDList.size();

	for (ID = IDList.begin(); ID != IDList.end(); ++ID) {
		currentID = *ID;
		memcpy(bytePtr + pos, &currentID, 4);
		pos += 4;
	}

	packet->payload(bytePtr, length);
	packet->flags(0);

	free(bytePtr);
	return packet;
}

PacketPtr PacketAck(ProtocolManager* protocol, unsigned int ID)
{
	PacketPtr packet(new Packet("PacketAck", protocol));
	byte bytePtr[5];

	bytePtr[0] = 1;

	memcpy(bytePtr + 1, &ID, 4);

	packet->payload(bytePtr, 5);
	packet->flags(0);

	return packet;
}

PacketPtr UseCircuitCode(ProtocolManager* protocol, SimpleLLUUID AgentID, SimpleLLUUID SessionID, unsigned int Code)
{
	PacketPtr packet(new Packet("UseCircuitCode", protocol));
	byte bytePtr[36];
	unsigned short flags = MSG_RELIABLE;

	memcpy(bytePtr, AgentID.data, 16);
	memcpy(bytePtr + 16, SessionID.data, 16);
	memcpy(bytePtr + 32, &Code, 4);

	packet->payload(bytePtr, 36);
	packet->flags(htons(flags));

	return packet;
}

PacketPtr LogoutRequest(ProtocolManager* protocol, SimpleLLUUID AgentID, SimpleLLUUID SessionID)
{
	PacketPtr packet(new Packet("LogoutRequest", protocol));
	byte bytePtr[32];
	unsigned short flags = MSG_RELIABLE & MSG_ZEROCODED;

	memcpy(bytePtr, AgentID.data, 16);
	memcpy(bytePtr + 16, SessionID.data, 16);

	packet->payload(bytePtr, 32);
	packet->flags(htons(flags));

	return packet;
}

PacketPtr CompleteAgentMovement(ProtocolManager* protocol, SimpleLLUUID AgentID, SimpleLLUUID SessionID, unsigned int CircuitCode)
{
	PacketPtr packet(new Packet("CompleteAgentMovement", protocol));
	byte bytePtr[36];
	unsigned short flags = MSG_RELIABLE;

	memcpy(bytePtr, AgentID.data, 16);
	memcpy(bytePtr + 16, SessionID.data, 16);
	memcpy(bytePtr + 32, &CircuitCode, 4);

	packet->payload(bytePtr, 36);
	packet->flags(htons(flags));

	return packet;
}

PacketPtr RegionHandshakeReply(ProtocolManager* protocol, unsigned int Flags)
{
	PacketPtr packet(new Packet("RegionHandshakeReply", protocol));
	packet->payload((byte*)&Flags, 4);
	packet->flags(0);

	return packet;
}

PacketPtr CompletePingCheck(ProtocolManager* protocol, byte PingID)
{
	PacketPtr packet(new Packet("CompletePingCheck", protocol));
	packet->payload(&PingID, 1);
	packet->flags(0);

	return packet;
}

PacketPtr DirLandQuery(ProtocolManager* protocol, bool ReservedNewbie, bool ForSale, SimpleLLUUID QueryID, bool Auction, 
					   unsigned int QueryFlags, SimpleLLUUID AgentID, SimpleLLUUID SessionID)
{
	PacketPtr packet(new Packet("DirLandQuery", protocol));
	byte bytePtr[55];

	bytePtr[0] = ReservedNewbie;
	bytePtr[1] = ForSale;
	memcpy(bytePtr + 2, QueryID.data, 16);
	bytePtr[18] = Auction;
	memcpy(bytePtr + 19, &QueryFlags, 4);
	memcpy(bytePtr + 23, AgentID.data, 16);
	memcpy(bytePtr + 39, SessionID.data, 16);

	packet->payload(bytePtr, 55);
	packet->flags(0);

	return packet;
}

PacketPtr UUIDGroupNameRequest(ProtocolManager* protocol, SimpleLLUUID ID)
{
	PacketPtr packet(new Packet("UUIDGroupNameRequest", protocol));
	unsigned short flags = MSG_RELIABLE & MSG_ZEROCODED;
	byte bytePtr[17];
	bytePtr[0] = 1;
	memcpy(bytePtr + 1, ID.data, 16);

	packet->payload(bytePtr, 17);
	packet->flags(htons(flags));

	return packet;
}
