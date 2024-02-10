#include "IOCP.h"
#include "SessionManager.h"

bool IOCP::Initialize()
{
	std::cout << "Create " << WorkerThreadCount << " threads for IOCP" << std::endl;
	handle = ::CreateIoCompletionPort( INVALID_HANDLE_VALUE, 0, 0, WorkerThreadCount );
	for ( int count = 0; count < WorkerThreadCount; count++ )
	{
		std::cout << "Wait for data to be entered in the IO Completion Queue" << std::endl;
		std::thread th( [&]() { IOCP::WaitCompletionStatus(); } );
		th.detach();
	}

	return true;
}

void IOCP::Bind( const HANDLE& _socket, const ULONG_PTR _key ) const
{
	::CreateIoCompletionPort( _socket, handle, _key, 0 );

	Session* session = ( Session* )_key;
	session->Recieve();
}

void IOCP::WaitCompletionStatus() const
{
	ULONG_PTR key;
	LPOVERLAPPED ov;
	DWORD transferred;
	
	while ( true )
	{
		if ( ::GetQueuedCompletionStatus( handle, &transferred, &key, &ov, INFINITE ) == TRUE )
		{
			Session* session = ( Session* )key;
			if ( transferred == 0 )
			{
				// 일반적으로 상대방 소켓이 끊어졌을 때 0byte를 read 한다.
				if ( session != nullptr )
					 SessionManager::Inst().Erase( session );
			}
			else
			{
				if ( session != nullptr && ov != NULL )
					 session->Dispatch( ov, transferred );
			}
		}
		else
		{
			Session* session = ( Session* )key;
			DWORD error = ::GetLastError();
			std::cout << "Queue LastError : " << ::GetLastError() << std::endl;
			switch ( error )
			{
				default:
				{
					// 작업이 취소되었을 때 발생하는 오류
					if ( error != ERROR_OPERATION_ABORTED )
					{
						// 일반적으로 상대방 소켓이 끊어졌을 때 0byte를 read 한다.
						if ( session != nullptr && transferred == 0 )
							 SessionManager::Inst().Erase( session );
					}
				} break;

				// 방화벽, 라우터, 랜뽑 등의 제한으로 인한 네트워크 단절이 발생했을 때
				case WAIT_TIMEOUT:
				{
					// 고쳐질 때까지 대기한다.
					continue;
				} break;

				// 상대방이 closesocket, shutdown을 호출하지않고 종료했을 때 0byte read가 발생하지 않는다.
				// 0byte read가 발생하지 않은 시점에서 IO 작업을 시도했을 때
				// 상대방은 이미 종료 되었으므로 해당 오류가 발생한다. ( 우아한 종료와 상반되는 개념 )
				case ERROR_NETNAME_DELETED:
				{
					if ( session != nullptr )
						 SessionManager::Inst().Erase( session );
				} break;
			}
		}
	}
}