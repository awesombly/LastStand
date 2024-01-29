#pragma once
#include "../Global/Singleton.hpp"
#include "../Packet/Packet.h"

class PacketManager : public Singleton<PacketManager>
{
private:
	std::queue<PACKET> packets;
	std::condition_variable cv;
	std::mutex mtx;

	// std::unordered_map<u_short/* Packet Type */, std::function<void(const PACKET&)>> protocols;

public:
	PacketManager() = default;
	virtual ~PacketManager() = default;

public:
	bool Initialize();
	void Push( const PACKET& _packet );

private:
	void Process();
	void BindProtocols();
};