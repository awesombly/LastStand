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
				// �Ϲ������� ���� ������ �������� �� 0byte�� read �Ѵ�.
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
					// �۾��� ��ҵǾ��� �� �߻��ϴ� ����
					if ( error != ERROR_OPERATION_ABORTED )
					{
						// �Ϲ������� ���� ������ �������� �� 0byte�� read �Ѵ�.
						if ( session != nullptr && transferred == 0 )
							 SessionManager::Inst().Erase( session );
					}
				} break;

				// ��ȭ��, �����, ���� ���� �������� ���� ��Ʈ��ũ ������ �߻����� ��
				case WAIT_TIMEOUT:
				{
					// ������ ������ ����Ѵ�.
					continue;
				} break;

				// ������ closesocket, shutdown�� ȣ�������ʰ� �������� �� 0byte read�� �߻����� �ʴ´�.
				// 0byte read�� �߻����� ���� �������� IO �۾��� �õ����� ��
				// ������ �̹� ���� �Ǿ����Ƿ� �ش� ������ �߻��Ѵ�. ( ����� ����� ��ݵǴ� ���� )
				case ERROR_NETNAME_DELETED:
				{
					if ( session != nullptr )
						 SessionManager::Inst().Erase( session );
				} break;
			}
		}
	}
}