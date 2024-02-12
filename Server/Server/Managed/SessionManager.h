#pragma once
#include "Global/Singleton.hpp"
#include "Network/Session.h"
#include "Synchronize/CriticalSection.h"

class SessionManager : public Singleton<SessionManager>
{
private:
	std::queue<Session*> unresponsiveSessions;
	std::unordered_map<SOCKET, Session*> sessions;
	std::mutex mtx;

public:
	SessionManager() = default;
	virtual ~SessionManager();

public:
	bool Initialize();
	void ConfirmDisconnect();

public:
	Session* Find( const SOCKET& _socket ) const;
	std::unordered_map<SOCKET, Session*> GetSessions() const;

	void Send( const SOCKET& _socket, const UPacket& _packet ) const;
	void Broadcast( const UPacket& _packet ) const;
	void BroadcastWithoutSelf( const SOCKET& _socket, const UPacket& _packet ) const;

public:
	void Push( Session* _session );
	void Erase( Session* _session );
};