#pragma once
#include "../Global/Singleton.hpp"
#include "../Packet/Packet.h"

class PacketManager : public Singleton<PacketManager>
{
private:
	std::queue<Packet> packets;
	std::condition_variable cv;
	std::mutex mtx;

public:
	PacketManager() = default;
	virtual ~PacketManager() = default;

public:
	bool Initialize();
	void Push( const Packet& _packet );

private:
	void Process();
};