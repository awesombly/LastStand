#include "IOCP.h"
#include "SessionManager.h"

bool IOCP::Initialize()
{
	handle = ::CreateIoCompletionPort( INVALID_HANDLE_VALUE, 0, 0, WorkerThreadCount );
	for ( int count = 0; count < WorkerThreadCount; ++count )
	{
		std::thread th( [&]() { IOCP::WaitCompletionStatus(); } );
		th.detach();
	}

	std::cout << "Make " << WorkerThreadCount << " WorkerThreads" << std::endl;

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
			if ( transferred != 0 )
			{
				if ( ov != NULL && session != nullptr )
				{
					session->Dispatch( ov, transferred );
				}
			}
			else
			{
				SessionManager::Inst().Erase( session );
			}
		}
		else
		{
			Session* session = ( Session* )key;
			if ( ::GetLastError() != ERROR_OPERATION_ABORTED )
			{
				if ( transferred == 0 && key != NULL )
				{
					SessionManager::Inst().Erase( session );
					continue;
				}
			}
		}
	}
}