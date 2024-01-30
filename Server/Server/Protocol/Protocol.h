#pragma once
#include "../Global/Header.h"

#define CONSTRUCTOR( _class ) _class() { name = #_class; type = GetPacketID( name.c_str() ); }

u_short GetPacketID( const char* _name );

interface IProtocol
{
public:
	std::string name;
	u_short type;
};

struct SampleProtocol : public IProtocol
{
	CONSTRUCTOR( SampleProtocol )
	
	// 필요한 데이터 생성
	// int hp;
	// int speed; ...
};

struct ChatMessage : public IProtocol
{
	CONSTRUCTOR( ChatMessage )
};