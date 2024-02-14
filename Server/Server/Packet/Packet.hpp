#pragma once
#include "Global/Header.h"
#include "Protocol/Protocol.hpp"

#pragma pack( push, 1 )
struct UPacket
{
	PacketType type;
	u_short size;
	byte data[Global::MaxDataSize];

	UPacket() : type( PacketType::NONE ), size( 0 ), data{} {}

	template<typename Type>
	UPacket( const PacketType& _type, Type _protocol )
	{
		// Json 직렬화
		std::ostringstream stream;
		{
			cereal::JSONOutputArchive archive( stream, cereal::JSONOutputArchive::Options::NoIndent() );
			_protocol.serialize( archive );
		}

		// 인코딩 방식 : UTF8
		std::string json = Global::Text::ToUTF8( Global::Text::ToAnsi( stream.str() ) );

		// Json 크기 축소
		Global::String::RemoveAll( json, '\n' );
		Global::String::ReplaceAll( json, "\": ", "\":" );
		
		// data 초기화
		::memset( data, 0, sizeof( byte ) * Global::MaxDataSize );
		::memcpy( data, json.c_str(), json.length() );

		type = _type;
		size = ( u_short )json.length() + Global::HeaderSize;
	}
};
#pragma pack( pop )

class Session;
struct Packet : public UPacket
{
	Session* session;
};

template<typename Type>
Type FromJson( const Packet& _data )
{
	std::string json;
	std::string data = ( char* )_data.data;
	size_t pos = data.find( "{\"value0\": " );
	if ( pos == std::string::npos ) json.append( "{\"value0\": " ).append( data ).append( " }" );
	else                   		    json = data;

	Type ret;
	std::istringstream stream( json );
	{
		cereal::JSONInputArchive archive( stream );
		archive( ret );
	}
	
	return ret;
}