#include "Session.h"
#include "Management/PacketSystem.h"

const int   Session::MaxUnresponse       = 30;
const float Session::MinResponseWaitTime = 30.0f;
const float Session::RequestDelay        = 60.0f;

Session::Session( const SOCKET& _socket, const SOCKADDR_IN& _address )
				  : Network( _socket, _address ), packet( new Packet() ), 
	                buffer{}, startPos( 0 ), writePos( 0 ), readPos( 0 ),
					lastResponseTime( std::chrono::steady_clock::now() ),
					unresponse( 0 ), time( 0 ), stage( nullptr ), player( nullptr ), serial( 0 ) { }

Session::~Session()
{
	::shutdown( socket, SD_SEND );
}

void Session::Alive()
{
	lastResponseTime = std::chrono::steady_clock::now();
	unresponse = 0;
}

bool Session::CheckAlive()
{
	time = std::chrono::steady_clock::now() - lastResponseTime;
	if ( time.count() > MinResponseWaitTime + ( RequestDelay * unresponse ) )
	{
		if ( ++unresponse > MaxUnresponse )
 			 return false;

		std::cout << "Verify that the session is alive( " << GetPort() << " " << GetAddress() << " )" << std::endl;
		Send( UPacket( PACKET_HEARTBEAT ) );
	}

	return true;
}

void Session::Dispatch( const LPOVERLAPPED& _ov, DWORD _size )
{
	OVERLAPPEDEX* overalapped = ( OVERLAPPEDEX* )_ov;
	if ( overalapped->flag == OVERLAPPEDEX::MODE_RECV )
	{
		Alive();
		// ���ۿ� ������ ���� ���
		// ���۸� �ʱ�ȭ�ϰ� �������� �ܿ� �����͸� ���� ������ �̵�
		if ( writePos + _size > MaxReceiveSize )
		{
			byte remain[MaxReceiveSize] = { 0, };
			::memcpy( remain, &buffer[startPos], readPos );

			ZeroMemory( buffer, MaxReceiveSize * sizeof( byte ) );
			::memcpy( buffer, remain, readPos );

			startPos = 0;
			writePos = readPos;
		}

		// ���� �����͸� ���ۿ� �߰��Ѵ�.
		::memcpy( &buffer[writePos], wsaRecvBuffer.buf, _size * sizeof( byte ) );
		writePos += _size; // ������� ���ۿ� ���� ����
		readPos  += _size; // ���� �������� �������� ��

		// �о���ϴ� �����Ͱ� �ּ� ���( 4����Ʈ ) �̻��� �Ǿ���Ѵ�.
		if ( readPos < Global::HeaderSize )
		{
			ZeroMemory( &wsaRecvBuffer, sizeof( WSABUF ) );
			Recieve();
			return;
		}

		packet = ( UPacket* )&buffer[startPos];
		while ( readPos >= packet->size ) // �ϳ� �̻��� ��Ŷ�� �ϼ��Ǿ��� ��
		{
			Packet newPacket;
			::memcpy( &newPacket, packet, packet->size );
			newPacket.session = this;

			PacketSystem::Inst().Push( newPacket );

			readPos  -= packet->size;
			startPos += packet->size; // �о���ϴ� ���� ����

			// �ϼ��� ��Ŷ�� �� �ִ��� Ȯ��
			if ( readPos < Global::HeaderSize ) break;
			packet = ( UPacket* )&buffer[startPos];

			if ( packet->size <= 0 ) break;
		}

		ZeroMemory( &wsaRecvBuffer, sizeof( WSABUF ) );
		Recieve();
	}
	else if ( overalapped->flag == OVERLAPPEDEX::MODE_SEND )
	{
		
	}

	Global::Memory::SafeDelete( overalapped );
}