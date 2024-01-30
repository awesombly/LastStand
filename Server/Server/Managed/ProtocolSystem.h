#pragma once
#include "../Protocol/Protocol.h"
#include "../Global/Singleton.hpp"
#include "../Packet/Packet.h"

class ProtocolSystem : public Singleton<ProtocolSystem>
{
private:
	std::unordered_map<u_short/* Packet Type */, void(*)( const Packet& ) > protocols;

public:
	void Process( const Packet& _packet );

private:
	void Regist( const IProtocol& _protocol, void( *_func )( const Packet& ) );
	void Bind();

	// Protocol
	static void Broadcast( const Packet& _packet );
	static void BroadcastWithoutSelf( const Packet& _packet );

public:
	ProtocolSystem()          = default;
	virtual ~ProtocolSystem() = default;

};