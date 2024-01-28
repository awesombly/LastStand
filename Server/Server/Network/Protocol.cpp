#include "Protocol.h"

PacketType GetPacketType( const char* _name )
{
	unsigned int hash = 0;

	const size_t size = ::strlen( _name );
	for ( size_t i = 0; i < size; i++ )
	{
		hash = _name[i] + ( hash << 6 ) + ( hash << 16 ) - hash;
	}

	return ( PacketType )hash;
}