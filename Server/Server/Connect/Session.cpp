#include "Session.h"
#include "../Managed/PacketSystem.h"

Session::Session( const SOCKET& _socket, const SOCKADDR_IN& _address )
				  : Network( _socket, _address ), packet( new Packet() ), buffer{}, startPos( 0 ), writePos( 0 ), readPos( 0 ) { }

Session::~Session()
{
	::shutdown( socket, SD_SEND );
	::closesocket( socket );
}


void Session::Dispatch( const LPOVERLAPPED& _ov, DWORD _size )
{
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
				newPacket.socket = socket;
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