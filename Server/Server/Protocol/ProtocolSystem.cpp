#include "ProtocolSystem.h"

//u_short ProtocolSystem::GetPacketType( const char* _name )
//{
//	unsigned int hash = 0;
//
//	const size_t size = ::strlen( _name );
//	for ( size_t i = 0; i < size; i++ )
//	{
//		hash = _name[i] + ( hash << 6 ) + ( hash << 16 ) - hash;
//	}
//
//	return ( u_short )hash;
//}
//
//void ProtocolSystem::Bind() const
//{
//	RegisterProtocol( new Lobby() );
//}
//
//void ProtocolSystem::RegisterProtocol( IProtocol _protocol ) const
//{
//	
//	new Lobby()
//}