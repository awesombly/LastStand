#pragma once
#include "../Global/Header.h"

#include <cereal/types/array.hpp>
#include <cereal/types/atomic.hpp>
#include <cereal/types/valarray.hpp>
#include <cereal/types/vector.hpp>
#include <cereal/types/deque.hpp>
#include <cereal/types/forward_list.hpp>
#include <cereal/types/list.hpp>
#include <cereal/types/string.hpp>
#include <cereal/types/map.hpp>
#include <cereal/types/queue.hpp>
#include <cereal/types/set.hpp>
#include <cereal/types/stack.hpp>
#include <cereal/types/unordered_map.hpp>
#include <cereal/types/unordered_set.hpp>
#include <cereal/types/utility.hpp>
#include <cereal/types/tuple.hpp>
#include <cereal/types/bitset.hpp>
#include <cereal/types/complex.hpp>
#include <cereal/types/chrono.hpp>
#include <cereal/types/polymorphic.hpp>

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

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( message ) );
	}
};

struct ConnectMessage : public IProtocol
{
public:
	CONSTRUCTOR( ConnectMessage )

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
	CONSTRUCTOR( SampleProtocol )

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