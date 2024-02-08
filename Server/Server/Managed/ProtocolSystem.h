#pragma once
#include "Protocol/Protocol.hpp"
#include "Global/Singleton.hpp"
#include "Packet/Packet.hpp"

class ProtocolSystem : public Singleton<ProtocolSystem>
{
private:
	std::unordered_map<u_short/* Packet Type */, void(*)( const Packet& ) > protocols;

public:
	ProtocolSystem()          = default;
	virtual ~ProtocolSystem() = default;

public:
	void Initialize();
	void Process( const Packet& _packet );

private:
	void Regist( const IProtocol& _protocol, void( *_func )( const Packet& ) );

	// Protocol
	static void Broadcast( const Packet& _packet );
	static void BroadcastWithoutSelf( const Packet& _packet );

	// Login, Sign Up
	static void Login( const Packet& _packet );
	static void RequestSignUp( const Packet& _packet );
	static void RequestSignUpMail( const Packet& _packet );
};