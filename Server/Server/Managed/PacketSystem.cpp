#include "PacketSystem.h"
#include "ProtocolSystem.h"

bool PacketSystem::Initialize()
{
	ProtocolSystem::Inst().Initialize();

	std::cout << "Create thread for packet processing" << std::endl;
	std::cout << "Waiting for received data to be converted into packets" << std::endl;
	std::thread th( [&]() { PacketSystem::Process(); } );
	th.detach();

	return true;
}

void PacketSystem::Push( const Packet& _packet )
{
	std::lock_guard<std::mutex> lock( mtx );
	packets.push( _packet );
	cv.notify_one();
}
		 
void PacketSystem::Process()
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