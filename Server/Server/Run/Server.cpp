#include "Server.h"
#include "Managed/PacketSystem.h"
#include "Managed/IOCP.h"
#include "Database/Database.h"

Server::Server()
{
	kill = ::CreateEvent( NULL, FALSE, FALSE, _T( "ServerKillEvent" ) );
}

void Server::Start( const int _port, const char* _address )
{
	if ( !Database::Inst().Initialize() )
	{
		std::cout << "MySQL connect failed" << std::endl;
	}

	if ( !IOCP::Inst().Initialize() )
	{
		std::cout << "IOCP initialize failed" << std::endl;
	}

	if ( !PacketSystem::Inst().Initialize() )
	{
		std::cout << "Packet processing failed" << std::endl;
	}

	if ( !acceptor.Accept( _port, _address ) )
	{
		std::cout << "Accept failed" << std::endl;
	}

	if ( ::WaitForSingleObject( kill, INFINITE ) == WAIT_FAILED )
	{
		// LOG_WARNING << "KillEvent Wait Failed" << ELogType::EndLine;
	}
}