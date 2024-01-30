#pragma once
#include "../Global/Header.h"

static const u_short HeaderSize  = 4;
static const u_short MaxDataSize = 2048;

#pragma pack( push, 1 )
struct Packet
{
	u_short type;
	u_short size;
	byte data[MaxDataSize];

	Packet() : type( 0 ), size( 0 ), data{} {}

};
#pragma pack( pop )