#pragma once
#include "../Global/Header.h"

static const u_short HeaderSize  = 4;
static const u_short MaxDataSize = 2048;

#pragma pack( push, 1 )
struct UPacket
{
	u_short type;
	u_short size;
	byte data[MaxDataSize];

	UPacket() : type( 0 ), size( 0 ), data{} {}

	//UPacket( const IProtocol& _protocol )
	//{
	//	type = _protocol.type;
	//	//size = data
	//}
};
#pragma pack( pop )

struct Packet : public UPacket
{
	SOCKET socket;
};