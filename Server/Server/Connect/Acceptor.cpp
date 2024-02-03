#include "Acceptor.h"
#include "Managed/SessionManager.h"
#include "Managed/IOCP.h"

Acceptor::~Acceptor()
{
	::WSACleanup();
}

bool Acceptor::Listen()
{
	if ( !SetSocketOption() ||
		 ::bind( socket, ( sockaddr* )&address, sizeof( address ) ) == SOCKET_ERROR ||
		 ::listen( socket, SOMAXCONN ) == SOCKET_ERROR )
	{
		ClosedSocket();

		return false;
	}

	std::cout << "Acceptor initialization completed" << std::endl;
	std::thread th( [&]() { Acceptor::WaitForClients(); } );
	th.detach();

	return true;

}

void Acceptor::WaitForClients()  const
{
	std::cout << "Wait for new session..." << std::endl;

	SOCKET clientSocket;
	SOCKADDR_IN addr {};
	int size = sizeof( addr );
	while ( true )
	{
		clientSocket = ::accept( socket, ( sockaddr* )&addr, &size );

		Session* session = new Session( clientSocket, addr );
		SessionManager::Inst().Push( session );
		IOCP::Inst().Bind( ( HANDLE )clientSocket, ( ULONG_PTR )session );
	}
}

bool Acceptor::SetSocketOption() const
{
	return true;
}