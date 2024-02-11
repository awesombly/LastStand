#pragma once
#include "../../Connect/Session.h"

struct RoomData
{
public:
	std::string title;
	int maxPersonnel;

public:
	RoomData() : title( "" ), maxPersonnel( 0 ) {}
	RoomData( const std::string& _title, int _maxPersonnel ) : 
	          title( _title ), maxPersonnel( _maxPersonnel ) { }

public:
	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( title ), CEREAL_NVP( maxPersonnel ) );
	}
};

class Room
{
private:
	u_short uid;
	RoomData data;
	Session* host;
	std::list<Session*> sessions;

public:
	Room( u_short _uid, const SOCKET& _host, const RoomData& _data );
	virtual ~Room() = default;

public:
	const RoomData& GetData() const;
	const u_short& GetUID() const;
};

struct ReqMakeRoom
{
public:
	std::string title;
	int maxPersonnel;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( title ) );
		ar( CEREAL_NVP( maxPersonnel ) );
	}
};