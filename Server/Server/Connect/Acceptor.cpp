#include "Acceptor.h"
#include "Managed/SessionManager.h"
#include "Managed/IOCP.h"

Acceptor::~Acceptor()
{
	::WSACleanup();
}

bool Acceptor::Accept( int _port, const char* _ip )
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
	// ������ �Ŵ��󿡼� AF ���� ����
	socket = ::socket( AF_INET/* IPv4 ���ͳ� �������� */, SOCK_STREAM/* TCP �������� ���� ��� */, 0 );
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

	if ( _ip == nullptr ) address.sin_addr.S_un.S_addr = ::htonl( INADDR_ANY );
	else                  address.sin_addr.S_un.S_addr = ::inet_addr( _ip );

	address.sin_port = ::htons( ( u_short )_port );

	std::cout << "Acceptor setup completed" << std::endl;
	Listen();

	return true;
}

bool Acceptor::Listen()
{
	if ( ::bind( socket, ( sockaddr* )&address, sizeof( address ) ) == SOCKET_ERROR ||
		 ::listen( socket, SOMAXCONN ) == SOCKET_ERROR )
	{
		ClosedSocket();
		return false;
	}

	std::cout << "Create thread for listening" << std::endl;
	std::thread th( [&]() { Acceptor::WaitForClients(); } );
	th.detach();

	return true;

}

void Acceptor::WaitForClients()  const
{
	std::cout << "Waiting for a new session..." << std::endl;

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