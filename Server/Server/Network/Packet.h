#include "Protocol.h"

#pragma pack( push, 1 )
struct UPACKET
{
	u_short length;
	PacketType type;
	byte data[MaxDataSize];

	UPACKET() : length( 0 ), type( 0 ), data{} {}

};
#pragma pack( pop )

struct PACKET
{
	SOCKET  socket;
	UPACKET packet;

	PACKET() : socket{}, packet{} { }
};