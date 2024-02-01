#pragma once
#include "../Global/Header.h"

#define CONSTRUCTOR( _class ) _class() { name = #_class; type = GetPacketType( name.c_str() ); }

u_short GetPacketType( const char* _name );

interface IProtocol
{
public:
	std::string name;
	u_short type;
};

// 기본 프로토콜 구조
struct ChatMessage : public IProtocol
{
public:
	CONSTRUCTOR( ChatMessage )

	std::string message = "";
};

struct ConnectMessage : public IProtocol
{
public:
	CONSTRUCTOR( ConnectMessage )

	std::string message = "";
};

struct SampleProtocol : public IProtocol
{
public:
	CONSTRUCTOR( SampleProtocol )

	int money = 0;
	float speed = 0;
	std::string name = "";
};