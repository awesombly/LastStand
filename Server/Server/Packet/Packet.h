#pragma once
#include "../Network/Protocol.h"

#pragma pack( push, 1 )
struct PACKET
{
	u_short length;
	u_short type;
	byte data[MaxDataSize];

	PACKET() : length( 0 ), type( 0 ), data{} {}

};
#pragma pack( pop )