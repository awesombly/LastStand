#pragma once
#include "../Global/Header.h"
#include "../Global/Global.hpp"
#include "../Protocol/Protocol.h"
#include <cereal/cereal.hpp>
#include <cereal/archives/json.hpp>

static const u_short HeaderSize  = 4;
static const u_short MaxDataSize = 2048;

#pragma pack( push, 1 )
struct UPacket
{
	u_short type;
	u_short size;
	byte data[MaxDataSize];

	UPacket() : type( 0 ), size( 0 ), data{} {}

	template<typename Type>
	UPacket( const Type& _protocol )
	{
		// Unity - C++ Json 변환 중 일치하지 않는 부분 Unity 기준으로 통일
		std::ostringstream stream;
		{
			cereal::JSONOutputArchive archive( stream, cereal::JSONOutputArchive::Options::NoIndent() );
			archive( cereal::make_nvp( _protocol.name, _protocol ) );
		}

		// 인코딩 방식 : UTF8
		std::string json = ToUTF8( std::move( stream.str() ).c_str() );

		// Unity Json : 공백문자 제외
		// C++   Json : 공백문자 포함
		json.erase( remove( json.begin(), json.end(), '\n' ), json.end() );

		// Unity Json : 구조체 이름 제외
		// C++   Json : 구조체 이름 포함
		const size_t start      = json.find_first_of( ':' );
		const size_t end        = json.find_last_of(  '}' );
		std::string unifiedJson = json.substr( start + 2, end - start - 2 );

		// data 배열 초기화 ( 잉여문자 생성 방지 )
		::memset( data, 0, sizeof( byte ) * MaxDataSize );
		::memcpy( data, unifiedJson.c_str(), unifiedJson.length() );

		type = _protocol.type;
		size = ( u_short )unifiedJson.length() + HeaderSize;
	}
};
#pragma pack( pop )

struct Packet : public UPacket
{
	SOCKET socket;
};