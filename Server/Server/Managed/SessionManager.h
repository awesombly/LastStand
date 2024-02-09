#pragma once
#include "Global/Singleton.hpp"
#include "Connect/Session.h"
#include "Synchronize/CriticalSection.h"

class SessionManager : public Singleton<SessionManager>
{
private:
	std::queue<Session*> unresponsiveSessions;
	std::unordered_map<SOCKET, Session*> sessions;
	//CriticalSection cs;
	std::mutex mtx;

public:
	SessionManager() = default;
	virtual ~SessionManager();

public:
	bool Initialize();
	void ConfirmDisconnect();

public:
	void Send( const SOCKET& _socket, const UPacket& _packet ) const;
	Session* Find( const SOCKET& _socket ) const;
	std::unordered_map<SOCKET, Session*> GetSessions() const;

public:
	void Push( Session* _session );
	void Erase( Session* _session );
};