#pragma once
#include "Managed/ProtocolSystem.h"
#include "Room.h"

class Lobby : public IScene
{
private:
	static u_short RoomUID;
	static std::list<Room*> rooms;
	static std::list<RoomInfo> roomInfos;

public:
	Lobby()          = default;
	virtual ~Lobby() = default;

private:
	static void CreateRoom( const Packet& _packet );

public:
	virtual void Bind() override;
};

struct ResTakeRoom
{
public:
	std::list<RoomInfo> rooms;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( rooms ) );
	}
};