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
		// Unity - C++ Json ��ȯ �� ��ġ���� �ʴ� �κ� Unity �������� ����
		std::ostringstream stream;
		{
			cereal::JSONOutputArchive archive( stream, cereal::JSONOutputArchive::Options::NoIndent() );
			archive( cereal::make_nvp( _protocol.name, _protocol ) );
		}

		// ���ڵ� ��� : UTF8
		std::string json = ToUTF8( std::move( stream.str() ).c_str() );

		// Unity Json : ���鹮�� ����
		// C++   Json : ���鹮�� ����
		json.erase( remove( json.begin(), json.end(), '\n' ), json.end() );

		// Unity Json : ����ü �̸� ����
		// C++   Json : ����ü �̸� ����
		const size_t start      = json.find_first_of( ':' );
		const size_t end        = json.find_last_of(  '}' );
		std::string unifiedJson = json.substr( start + 2, end - start - 2 );

		// data �迭 �ʱ�ȭ ( �׿����� ���� ���� )
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