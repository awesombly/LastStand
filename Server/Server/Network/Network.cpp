#include "Network.h"

Network::Network( const SOCKET& _socket, const SOCKADDR_IN& _address ) 
	: socket( _socket ), address( _address ), wsaBuffer{}, ov{}, buffer{} { }

Network::~Network()
{
	std::cout << "Network Destructor" << std::endl;
	ClosedSocket();
}

bool Network::ClosedSocket()
{
	return ::closesocket( socket ) != SOCKET_ERROR;
}

bool Network::Connect() const
{
	return ::connect( socket, ( sockaddr* )&address, sizeof( address ) ) != SOCKET_ERROR;
}

void Network::Send( const UPacket& _packet )
{
	if ( _packet.type != Global::AliveProtocolType )
		 std::cout << "Send( " << _packet.type << ", " << _packet.size << "bytes" << " ) " << _packet.data << std::endl;

	if ( ::send( socket, ( const char* )&_packet, _packet.size, 0 ) == SOCKET_ERROR )
	{
		if ( ::WSAGetLastError() != WSA_IO_PENDING )
	 		 std::cout << "Send LastError : " << ::WSAGetLastError() << std::endl;
	}
}

void Network::Recieve()
{
	DWORD flag = 0;
	DWORD transferred = 0;
	ov.flag = OVERLAPPEDEX::MODE_RECV;
	wsaBuffer.buf = ( char* )buffer;
	wsaBuffer.len = Global::HeaderSize + Global::MaxDataSize;
	if ( ::WSARecv( socket, &wsaBuffer, 1, &transferred, &flag, ( LPOVERLAPPED )&ov, NULL ) == SOCKET_ERROR )
	{
		if ( ::WSAGetLastError() != WSA_IO_PENDING )
			 std::cout << "Recv LastError : " << ::WSAGetLastError() << std::endl;
	}
}

const SOCKET& Network::GetSocket()
{
	return socket;
}

std::string Network::GetAddress() const
{
	return ::inet_ntoa( address.sin_addr );
}

std::string Network::GetPort() const
{
	return std::to_string( ::ntohs( address.sin_port ) );
}
