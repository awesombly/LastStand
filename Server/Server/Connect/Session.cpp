#include "Session.h"
#include "../Managed/PacketManager.h"

Session::Session( const SOCKET& _socket, const SOCKADDR_IN& _address )
	: Network( _socket, _address ), packet( new PACKET() ), temp{}, startPos( 0 ), writePos( 0 ), readPos( 0 ) { }

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
			// ��Ŷ�� ������ ����( temp )�� ������ ���� ��� �ʱ�ȭ
			if ( writePos + _size > MaxReceiveSize )
			{
				// �ܿ� ��Ŷ ����
				char remain[MaxReceiveSize] = { 0, };
				::memcpy( remain, &temp[startPos], readPos );

				// �ܿ� ��Ŷ�� ó������ �����ϰ� �� �޸� �ʱ�ȭ
				ZeroMemory( temp, sizeof( char ) * MaxReceiveSize );
				::memcpy( temp, remain, readPos );

				packet   = ( PACKET* )temp;
				startPos = 0;
				writePos = readPos;
			}

			// wsaBuffer�� ���� ��ŭ ū ���ۿ� �ű��. ( �ѹ��� �ϳ��� ��Ŷ�� ���� ���� ���� ���� )
			::memcpy( &temp[writePos], wsaBuffer.buf, sizeof( char ) * _size );
			packet    = ( PACKET* )&temp[startPos];
			writePos += _size; // ���� ������ ��( ��ġ )
			readPos  += _size;

			// ��Ŷ�� �ϼ��Ǿ��� ��
			if ( readPos >= packet->length )
			{
				do
				{
					PACKET newPacket;
					::memcpy( &newPacket, packet, packet->length );
					PacketManager::Inst().Push( newPacket );

					readPos  -= packet->length;
					startPos += packet->length;

					// �ϴ� �ϳ��� ��Ŷ�� �ϼ��Ǿ��ٴ� ���� �˰�
					// �� �� ��Ŷ�� �ϼ��� ���·� ������ ���� �ֱ⿡ do-while������ �ѹ��� ó���Ѵ�.
					packet = ( PACKET* )&temp[startPos];
					if ( packet->length <= 0 ) break;

				} while ( readPos >= packet->length );
			}

			ZeroMemory( &wsaBuffer,  sizeof( WSABUF ) );
		}

		Recieve();
	}