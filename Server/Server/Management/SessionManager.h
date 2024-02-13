#pragma once
#include "Global/Singleton.hpp"
#include "Network/Session.h"
#include "Synchronize/CriticalSection.h"
#include "Stage/Stage.h"

class SessionManager : public Singleton<SessionManager>
{
private:
	std::queue<Session*> unAckSessions;
	std::unordered_map<SOCKET, Session*> sessions;
	std::unordered_map<SerialType, Stage*> stages;
	std::mutex mtx;

public:
	SessionManager() = default;
	virtual ~SessionManager();

private:
	void ConfirmDisconnect();

public:
	bool Initialize();

	// Full Management
	void Push( Session* _session );
	void Erase( Session* _session );
	Session* Find( const SOCKET& _socket ) const;

	void Broadcast( const UPacket& _packet ) const;
	void BroadcastWithoutSelf( const SOCKET& _socket, const UPacket& _packet ) const;
	void BroadcastWaitingRoom( const UPacket& _packet );
	std::unordered_map<SOCKET, Session*> GetSessions() const;

	// Stage Management
	void EntryStage( Session* _session, const STAGE_INFO& _serial );
	void ExitStage( Session* _session, const STAGE_INFO& _serial );
	const std::unordered_map<SerialType, Stage*>& GetStages() const;
};