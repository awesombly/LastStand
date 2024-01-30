#include "Protocol.h"

//template<typename Type>
//u_short GetPacketID( const Type& _t )
//{
//	unsigned int hash = 0;
//	std::string name = Split( typeid( _t ).name(), ' ' )[1];
//	const size_t size = ::strlen( name.c_str() );
//
//	for ( size_t i = 0; i < size; i++ )
//	{
//		hash = name[i] + ( hash << 6 ) + ( hash << 16 ) - hash;
//	}
//
//	std::cout << name << " " << hash << std::endl;
//
//	return ( u_short )hash;
//}

u_short GetPacketID( const char* _name )
{
	unsigned int hash = 0;

	const size_t size = ::strlen( _name );
	for ( size_t i = 0; i < size; i++ )
	{
		hash = _name[i] + ( hash << 6 ) + ( hash << 16 ) - hash;
	}

	return ( u_short )hash;
}

std::vector<std::string> Split( const std::string& _data, char _delimiter )
{
	std::vector<std::string> ret;
	std::stringstream ss( _data );
	std::string token;
	while ( std::getline( ss, token, _delimiter ) )
	{
		ret.push_back( token );
	}

	return ret;
}