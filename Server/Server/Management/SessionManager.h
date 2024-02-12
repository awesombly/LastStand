#pragma once
#include "Global/Singleton.hpp"
#include "Network/Session.h"
#include "Synchronize/CriticalSection.h"
#include "Stage/Stage.h"

class SessionManager : public Singleton<SessionManager>
{
private:
	std::queue<Session*> unresponsiveSessions;
	std::unordered_map<SOCKET, Session*> sessions;
	std::unordered_map<SerialType, Stage*> stages;
	std::mutex mtx;

public:
	SessionManager() = default;
	virtual ~SessionManager();

public:
	// Default
	bool Initialize();
	void ConfirmDisconnect();
	void Send( const SOCKET& _socket, const UPacket& _packet ) const;

	// Full Management
	void Push( Session* _session );
	void Erase( Session* _session );
	Session* Find( const SOCKET& _socket ) const;

	void Broadcast( const UPacket& _packet ) const;
	void BroadcastWithoutSelf( const SOCKET& _socket, const UPacket& _packet ) const;

	std::unordered_map<SOCKET, Session*> GetSessions() const;

	// Stage Management
	void CreateStage( const SOCKET& _host, const STAGE_INFO& _info );
	void EntryStage( const SOCKET& _user, const STAGE_INFO& _info );

	std::unordered_map<SerialType, Stage*> GetStages() const;
};