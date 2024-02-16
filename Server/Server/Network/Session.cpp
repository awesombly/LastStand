#include "Session.h"
#include "Management/PacketSystem.h"

const int   Session::MaxUnresponse       = 30;
const float Session::MinResponseWaitTime = 30.0f;
const float Session::RequestDelay        = 60.0f;

Session::Session( const SOCKET& _socket, const SOCKADDR_IN& _address )
				  : Network( _socket, _address ), packet( new Packet() ), 
	                buffer{}, startPos( 0 ), writePos( 0 ), readPos( 0 ),
					lastResponseTime( std::chrono::steady_clock::now() ),
					unresponse( 0 ), time( 0 ), stage( nullptr ), player( nullptr ) { }

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

		std::cout << "Verify that the session is alive( " << GetPort() << "  " << GetAddress() << " )" << std::endl;
		Send( UPacket( PACKET_HEARTBEAT, EMPTY(/* �� �������� */ ) ) );
	}

	return true;
}

void Session::Dispatch( const LPOVERLAPPED& _ov, DWORD _size )
{
	Alive();
	OVERLAPPEDEX* overalapped = ( OVERLAPPEDEX* )_ov;
	if ( overalapped->flag == OVERLAPPEDEX::MODE_RECV )
	{
		// ��Ŷ�� ������ ����( temp )�� ������ ���� ��� �ʱ�ȭ
		if ( writePos + _size > MaxReceiveSize )
		{
			// �ܿ� ��Ŷ ����
			char remain[MaxReceiveSize] = { 0, };
			::memcpy( remain, &buffer[startPos], readPos );

			// �ܿ� ��Ŷ�� ó������ �����ϰ� �� �޸� �ʱ�ȭ
			ZeroMemory( buffer, sizeof( char ) * MaxReceiveSize );
			::memcpy( buffer, remain, readPos );

			packet   = ( UPacket* )buffer;
			startPos = 0;
			writePos = readPos;
		}

		// wsaBuffer�� ���� ��ŭ ū ���ۿ� �ű��. ( �ѹ��� �ϳ��� ��Ŷ�� ���� ���� ���� ���� )
		::memcpy( &buffer[writePos], wsaBuffer.buf, sizeof( char ) * _size );
		packet    = ( UPacket* )&buffer[startPos];
		writePos += _size; // ���� ������ ��( ��ġ )
		readPos  += _size;

		// ��Ŷ�� �ϼ��Ǿ��� ��
		if ( readPos >= packet->size )
		{
			do
			{
				Packet newPacket;
				::memcpy( &newPacket, packet, packet->size );
				newPacket.session = this;
				
				PacketSystem::Inst().Push( newPacket );
				
				readPos  -= packet->size;
				startPos += packet->size;

				// �ϴ� �ϳ��� ��Ŷ�� �ϼ��Ǿ��ٴ� ���� �˰�
				// �� �� ��Ŷ�� �ϼ��� ���·� ������ ���� �ֱ⿡ do-while������ �ѹ��� ó���Ѵ�.
				packet = ( UPacket* )&buffer[startPos];
				if ( packet->size <= 0 ) break;

			} while ( readPos >= packet->size );
		}

		ZeroMemory( &wsaBuffer,  sizeof( WSABUF ) );
	}

	Recieve();
}