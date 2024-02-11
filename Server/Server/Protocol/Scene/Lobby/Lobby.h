#pragma once
#include "Managed/ProtocolSystem.h"
#include "Room.h"

class Lobby : public IScene
{
private:
	static u_short RoomUID;
	static std::list<Room*> rooms;
	static std::list<RoomInfo> infos;

public:
	Lobby()          = default;
	virtual ~Lobby() = default;

private:
	static void CreateRoom( const Packet& _packet );
	static void TakeLobbyInfo( const Packet& _packet );

public:
	virtual void Bind() override;
};

typedef struct LobbyInfo
{
public:
	std::list<ROOM_INFO> infos;

	template <class Archive>
	void serialize( Archive& ar )
	{
		ar( CEREAL_NVP( infos ) );
	}
} LOBBY_INFO;