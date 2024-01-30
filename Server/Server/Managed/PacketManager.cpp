#include "PacketManager.h"
#include "ProtocolSystem.h"

bool PacketManager::Initialize()
{
	ProtocolSystem::Inst().Initialize();

	std::cout << "Start packet processing" << std::endl;
	std::thread th( [&]() { PacketManager::Process(); } );
	th.detach();

	return true;
}

void PacketManager::Push( const Packet& _packet )
{
	std::lock_guard<std::mutex> lock( mtx );
	packets.push( _packet );
	cv.notify_one();
}
		 
void PacketManager::Process()
{
	while ( true )
	{
		std::unique_lock<std::mutex> lock( mtx );
		cv.wait( lock, [&]() { return !packets.empty(); } );

		Packet packet = packets.front();
		std::cout << "Receive( " << packet.type << ", " << packet.size << "bytes" << " ) " << packet.data << std::endl;
		ProtocolSystem::Inst().Process( packet );
		packets.pop();
	}
}