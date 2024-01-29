#include "PacketManager.h"

bool PacketManager::Initialize()
{
	BindProtocols();

	std::cout << "Start packet processing" << std::endl;
	std::thread th( [&]() { PacketManager::Process(); } );
	th.detach();

	return true;
}

void PacketManager::Push( const PACKET& _packet )
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

		PACKET* packet = &packets.front();
		//auto iter = protocols.find( packet->type );
		//if ( iter != protocols.cend() && iter->second != nullptr )
		//{
		//	protocols[packet->type]( *packet );
		//}

		std::cout << packet->type << " " << packet->length << "bytes" << " " << packet->data << std::endl;

		packets.pop();
	}
}

void PacketManager::BindProtocols()
{

}