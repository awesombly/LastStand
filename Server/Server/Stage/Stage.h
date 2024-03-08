#pragma once
#include "Network/Session.h"


using ActorContainer = std::unordered_map<SerialType, ActorInfo*>;
class Stage
{
public:
	STAGE_INFO info;
	Session* host;
	bool isGameOver;

private:
	std::list<Session*> sessions;
	ActorContainer actors;

public:
	Stage( Session* _host, const STAGE_INFO& _info );
	virtual ~Stage() = default;

	inline bool IsExist() { return sessions.size() > 0; }
	void Entry( Session* _session );
	void Exit( Session* _session );

	void Clear();

	bool DeadActor( ActorInfo* _dead, const HitInfo& _hit );
	PlayerInfo* FindWinner() const;

	void RegistActor( ActorInfo* _actor );
	void UnregistActor( const ActorInfo* _actor );
	ActorInfo* GetActor( SerialType _serial, bool _useLoging = true ) const;
	ActorContainer& GetActors();
	std::list<Session*>& GetSessions();
	PlayerInfo* FindPlayer( SerialType _serial ) const;
	void ClearActors();

	void Broadcast( const UPacket& _packet ) const;
	void BroadcastWithoutSelf( Session* _session, const UPacket& _packet ) const;
	void Send( SOCKET _socket, const UPacket& _packet ) const;
};