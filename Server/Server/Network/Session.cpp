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
		Send( UPacket( PACKET_HEARTBEAT, EMPTY(/* 빈 프로토콜 */ ) ) );
	}

	return true;
}

void Session::Dispatch( const LPOVERLAPPED& _ov, DWORD _size )
{
	Alive();
	OVERLAPPEDEX* overalapped = ( OVERLAPPEDEX* )_ov;
	if ( overalapped->flag == OVERLAPPEDEX::MODE_RECV )
	{
		// 패킷을 저장할 버퍼( temp )에 여분이 없을 경우 초기화
		if ( writePos + _size > MaxReceiveSize )
		{
			// 잔여 패킷 복사
			char remain[MaxReceiveSize] = { 0, };
			::memcpy( remain, &buffer[startPos], readPos );

			// 잔여 패킷은 처음부터 시작하고 뒷 메모리 초기화
			ZeroMemory( buffer, sizeof( char ) * MaxReceiveSize );
			::memcpy( buffer, remain, readPos );

			packet   = ( UPacket* )buffer;
			startPos = 0;
			writePos = readPos;
		}

		// wsaBuffer로 받은 만큼 큰 버퍼에 옮긴다. ( 한번에 하나의 패킷이 오지 않을 수도 있음 )
		::memcpy( &buffer[writePos], wsaBuffer.buf, sizeof( char ) * _size );
		packet    = ( UPacket* )&buffer[startPos];
		writePos += _size; // 사용된 버퍼의 양( 위치 )
		readPos  += _size;

		// 패킷이 완성되었을 때
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

				// 일단 하나의 패킷은 완성되었다는 것을 알고
				// 이 후 패킷이 완성된 상태로 들어왔을 수도 있기에 do-while문으로 한번에 처리한다.
				packet = ( UPacket* )&buffer[startPos];
				if ( packet->size <= 0 ) break;

			} while ( readPos >= packet->size );
		}

		ZeroMemory( &wsaBuffer,  sizeof( WSABUF ) );
	}

	Recieve();
}