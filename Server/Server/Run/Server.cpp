#include "Server.h"
#include "Managed/PacketSystem.h"
#include "Managed/IOCP.h"

Server::Server()
{
	kill = ::CreateEvent( NULL, FALSE, FALSE, _T( "ServerKillEvent" ) );
}

void Server::Start( const int _port, const char* _address )
{
	if ( !IOCP::Inst().Initialize() )
	{
		std::cout << "IOCP thread failed" << std::endl;
	}

	if ( !PacketSystem::Inst().Initialize() )
	{
		std::cout << "Packet processing failed" << std::endl;
	}

	if ( !acceptor.Initialize( _port, _address ) || !acceptor.Listen() )
	{
		std::cout << "Listen failed" << std::endl;
	}

	if ( ::WaitForSingleObject( kill, INFINITE ) == WAIT_FAILED )
	{
		// LOG_WARNING << "KillEvent Wait Failed" << ELogType::EndLine;
	}
}