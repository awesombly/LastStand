#pragma once
#include "../Global/Header.h"

static const u_short HeaderSize = 4;
static const u_short MaxDataSize = 2048;
static const u_short MaxReceiveSize = 10000;

//#define HEADER static const std::string name; static const PacketType type;
//#define BODY ( _name ) const std::string _name::name = #_name; const PacketType _name::type = GetPacketType( _name::name.c_str() );

u_short GetPacketType( const char* _name );

//interface IProtocol
//{
//	std::string name;
//	u_short type;
//
//	IProtocol( const std::string& _name ) : name( _name )
//	{
//		type = GetPacketType( name.c_str() );
//	}
//};
//
//struct ChatMessage : IProtocol
//{
//	ChatMessage( const std::string& _name ) : IProtocol( _name ) { }
//};