#pragma once
#include "Global/Header.h"

#define CONSTRUCTOR() u_short GetPacketType() const override          \
{																	  \
	std::string name = typeid( *this ).name();						  \
	size_t pos = name.find_first_of( " " );							  \
																	  \
	if ( pos != std::string::npos )									  \
		 name = name.substr( pos + 1, name.length() );				  \
																	  \
	unsigned int hash = 0;											  \
	const size_t size = ::strlen( name.c_str() );					  \
	for ( size_t i = 0; i < size; i++ )								  \
	{																  \
		hash = name[i] + ( hash << 6 ) + ( hash << 16 ) - hash;		  \
	}																  \
																	  \
	return ( u_short )hash;											  \
}																	  \

interface IProtocol
{
public:
	virtual u_short GetPacketType() const = 0;
};

// 기본 프로토콜 구조
struct ChatMessage : public IProtocol
{
public:
	CONSTRUCTOR()

	std::string message = "";

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( message ) );
	}
};

struct ConnectMessage : public IProtocol
{
public:
	CONSTRUCTOR()

	std::string message = "";

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( message ) );
	}
};

struct SampleProtocol : public IProtocol
{
public:
	CONSTRUCTOR()
	
	int money = 0;
	float speed = 0;
	std::string name = "";
	
	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( money ) );
		ar( CEREAL_NVP( speed ) );
		ar( CEREAL_NVP( name ) );
	}
};