#pragma once
#include "../Global/Header.h"

#define CONSTRUCTOR( _class ) _class() { name = #_class; id = GetPacketID( name.c_str() ); }

u_short GetPacketID( const char* _name );


interface IProtocol
{
public:
	std::string name;
	u_short id;
};

struct SampleProtocol : public IProtocol
{
	CONSTRUCTOR( SampleProtocol )
};

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