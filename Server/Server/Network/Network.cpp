#include "Network.h"

Network::Network( const SOCKET& _socket, const SOCKADDR_IN& _address ) 
	: socket( _socket ), address( _address ), wsaBuffer{}, ov{}, buffer{} { }

bool Network::Initialize( int _port, const char* _ip )
{
	WSADATA wsa;
	switch ( ::WSAStartup( MAKEWORD( 2, 2 ), &wsa ) )
	{
		case WSASYSNOTREADY:
			std::cout << "��Ʈ��ũ ��ſ� ���� �غ� ���� �ʾҽ��ϴ�." << std::endl;
			break;

		case WSAVERNOTSUPPORTED:
			std::cout << "��û�� ��������� ���������� �������� �ʽ��ϴ�." << std::endl;
			break;

		case WSAEINPROGRESS:
			std::cout << "��������� 1.1�۾��� ���� ���Դϴ�." << std::endl;
			break;

		case WSAEPROCLIM:
			std::cout << "��������� �������� �����ϴ� �۾� ���� ���ѿ� �����߽��ϴ�." << std::endl;
			break;

		case WSAEFAULT:
			std::cout << "WSAData�� ��ȿ���� �ʽ��ϴ�." << std::endl;
			break;
	}

	// INET : ���ͳ� ��������
	// AF   : �ּ� ü�踦 ������ �� ���     ( Address Family )
	// PF   : �������� ü�踦 ������ �� ��� ( Protocol Family )
	// �ƹ��ų� ����ص� ��������ϴ�. ( ������ �Ŵ��� ������������ AF�� �����ϴ� ���� ���� )
	socket = ::socket( AF_INET/* IPv4 ���ͳ� �������� */, SOCK_STREAM/* TCP �������� ���� ��� */, 0);

	ZeroMemory( &address, sizeof( address ) );
	address.sin_family = AF_INET; // ���� ���� �ÿ� ����

	// Little-Endian to Big-Endian
	// htons : host to network short
	// htonl : host to network long
	// 0x12345678 -> 0x12, 0x34, 0x56, 0x78 ���� ��Ʈ���� ����Ʈ ������ ���� ( �� ����� )
	
	// Big-Endian to Little-Endian
	// ntohs : network to host short
	// ntohl : network to host long
	// 0x12345678 -> 0x78, 0x56, 0x34, 0x12 ���� ��Ʈ���� ����Ʈ ������ ���� ( ��Ʋ ����� )

	if ( _ip == nullptr )
	{
		address.sin_addr.S_un.S_addr = ::htonl( INADDR_ANY );
	}
	else
	{
		address.sin_addr.S_un.S_addr = ::inet_addr( _ip );
	}
	address.sin_port = ::htons( ( u_short )_port );

	return true;
}

bool Network::ClosedSocket()
{
	return ::closesocket( socket ) != SOCKET_ERROR;
}

bool Network::Connect() const
{
	return ::connect( socket, ( sockaddr* )&address, sizeof( address ) ) != SOCKET_ERROR;
}

bool Network::Send( const Packet& _packet )
{
	return ::send( socket, ( char* )&_packet, _packet.size, 0 ) != SOCKET_ERROR;
}

void Network::Recieve()
{
	DWORD flag = 0;
	DWORD transferred = 0;
	ov.flag = OVERLAPPEDEX::MODE_RECV;
	wsaBuffer.buf = ( char* )buffer;
	wsaBuffer.len = HeaderSize + MaxDataSize;
	if ( ::WSARecv( socket, &wsaBuffer, 1, &transferred, &flag, ( LPOVERLAPPED )&ov, NULL ) == SOCKET_ERROR )
	{
		if ( ::WSAGetLastError() != WSA_IO_PENDING )
		{
			// �α�
		}
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
