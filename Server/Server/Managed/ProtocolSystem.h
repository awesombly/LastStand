#pragma once
#include "Protocol/Protocol.hpp"
#include "Global/Singleton.hpp"
#include "Packet/Packet.hpp"

class IScene
{
public:
	virtual void Bind() = 0;
};

class ProtocolSystem : public Singleton<ProtocolSystem>
{
private:
	std::unordered_map<u_short/* Packet Type */, void(*)( const Packet& ) > protocols;
	std::list<IScene*> scenes;

public:
	ProtocolSystem() = default;
	virtual ~ProtocolSystem();

public:
	void Initialize();
	void Process( const Packet& _packet );
	void Regist( const IProtocol& _protocol, void( *_func )( const Packet& ) );

	static void Broadcast( const Packet& _packet );
	static void BroadcastWithoutSelf( const Packet& _packet );

private:
	void Bind();
};