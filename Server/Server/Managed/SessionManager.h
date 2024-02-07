#pragma once
#include "Global/Singleton.hpp"
#include "Connect/Session.h"
#include "Synchronize/CriticalSection.h"

class SessionManager : public Singleton<SessionManager>
{
private:
	std::unordered_map<SOCKET, Session*> sessions;
	CriticalSection cs;

public:
	SessionManager() = default;
	virtual ~SessionManager();

public:
	void Send( const SOCKET& _socket, const UPacket& _packet ) const;
	Session* Find( const SOCKET& _socket ) const;
	std::unordered_map<SOCKET, Session*> GetSessions() const;

public:
	void Push( Session* _session );
	void Erase( Session* _session );
};