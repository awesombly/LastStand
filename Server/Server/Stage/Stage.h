#pragma once
#include "Network/Session.h"


using ActorContainer = std::unordered_map<SerialType, ActorInfo*>;
class Stage
{
public:
	STAGE_INFO info;
	Session* host;

private:
	std::list<Session*> sessions;
	ActorContainer actors;

	int totalKill;
public:
	Stage( Session* _host, const STAGE_INFO& _info );
	virtual ~Stage() = default;

	bool Entry( Session* _session );
	bool Exit( Session* _session );

	void Clear();

	bool DeadActor( ActorInfo* _dead, const HitInfo& _hit );
	PlayerInfo* FindWinner() const;

	void RegistActor( ActorInfo* _actor );
	void UnregistActor( const ActorInfo* _actor );
	ActorInfo* GetActor( SerialType _serial ) const;
	ActorContainer& GetActors();
	std::list<Session*>& GetSessions();
	PlayerInfo* FindPlayer( SerialType _serial ) const;
	void ClearActors();

	void Broadcast( const UPacket& _packet ) const;
	void BroadcastWithoutSelf( Session* _session, const UPacket& _packet ) const;
	void Send( SOCKET _socket, const UPacket& _packet ) const;
};