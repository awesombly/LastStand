#pragma once
#include "Managed/ProtocolSystem.h"
#include "Room.h"

class Lobby : public IScene
{
private:
	static u_short RoomUID;
	static std::list<Room*> rooms;
	static std::list<RoomData> roomDatas;

public:
	Lobby()          = default;
	virtual ~Lobby() = default;

private:
	static void MakeRoom( const Packet& _packet );

public:
	virtual void Bind() override;
};

struct ResMakeRoom
{
public:
	u_short uid;
	bool isCompleted;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( uid ) );
		ar( CEREAL_NVP( isCompleted ) );
	}
};

struct ResTakeRoom
{
public:
	std::list<RoomData> rooms;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( rooms ) );
	}
};