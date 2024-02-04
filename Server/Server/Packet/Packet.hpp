#pragma once
#include "Global/Header.h"
#include "Global/Global.hpp"
#include "Protocol/Protocol.hpp"


#pragma pack( push, 1 )
struct UPacket
{
	u_short type;
	u_short size;
	byte data[MaxDataSize];

	UPacket() : type( 0 ), size( 0 ), data{} {}

	template<typename Type>
	UPacket( Type _protocol )
	{
		// Json 직렬화
		std::ostringstream stream;
		{
			cereal::JSONOutputArchive archive( stream, cereal::JSONOutputArchive::Options::NoIndent() );
			_protocol.serialize( archive );
		}

		// 인코딩 방식 : UTF8
		std::string json = ToUTF8( std::move( stream.str().c_str() ) );

		// 송수신 Byte 등 확인하기 편하도록 Unity와 Json 형식 통일
		json.erase( remove( json.begin(), json.end(), '\n' ), json.end() );
		json.replace( json.find( "\": " ), 3, "\":" );

		// data 초기화
		::memset( data, 0, sizeof( byte ) * MaxDataSize );
		::memcpy( data, json.c_str(), json.length() );

		type = _protocol.GetPacketType();
		size = ( u_short )json.length() + HeaderSize;
	}
};
#pragma pack( pop )

struct Packet : public UPacket
{
	SOCKET socket;
};

template<typename Type>
Type FromJson( const Packet& _data )
{
	std::string json;
	std::string data = ToUTF8( ( const char* )_data.data );
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