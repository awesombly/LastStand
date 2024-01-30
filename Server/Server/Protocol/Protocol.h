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

// �⺻ �������� ����
struct ChatMessage : public IProtocol
{
public:
	CONSTRUCTOR( ChatMessage )

	std::string message;
};