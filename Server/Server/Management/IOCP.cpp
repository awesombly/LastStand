#include "IOCP.h"
#include "SessionManager.h"

IOCP::IOCP() : handle( nullptr ) { }

bool IOCP::Initialize()
{
	std::cout << "Create threads for IOCP" << std::endl;
	std::cout << "Wait for data to be entered in the IO Completion Queue" << std::endl;
	handle = ::CreateIoCompletionPort( INVALID_HANDLE_VALUE, 0, 0, Global::WorkerThreadCount );
	for ( int count = 0; count < Global::WorkerThreadCount; count++ )
	{
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
			// �Ϲ������� ���� ������ �������� �� 0byte�� read �Ѵ�.
			if ( transferred == 0 )
			{
				if ( session != nullptr )
					 SessionManager::Inst().Erase( session );
			}
			else
			{
				if ( session != nullptr )
				{
					if ( ov != NULL )
						 session->Dispatch( ov, transferred );
				}
			}
		}
		else
		{
			Session* session = ( Session* )key;
			switch ( DWORD error = ::GetLastError() )
			{
				default:
				{
					std::cout << "Queue LastError : " << error << std::endl;
					// �۾��� ��ҵǾ��� �� �߻��ϴ� ����
					if ( error != ERROR_OPERATION_ABORTED )
					{
						// �Ϲ������� ���� ������ �������� �� 0byte�� read �Ѵ�.
						if ( session != nullptr && transferred == 0 )
							 SessionManager::Inst().Erase( session );
					}
				} break;

				// �������� ������ϴ� Ŭ���̾�Ʈ ��������� ��
				case ERROR_CONNECTION_ABORTED:
				{
					continue;
				} break;

				// ��ȭ��, �����, ���� ���� �������� ���� ��Ʈ��ũ ������ �߻����� ��
				case WAIT_TIMEOUT:
				{
					// ������ ������ ����Ѵ�.
					continue;
				} break;

				// ������ closesocket, shutdown�� ȣ�������ʰ� �������� �� 0byte read�� �߻����� �ʴ´�.
				// 0byte read�� �߻����� ���� �������� Send/Recieve �۾��� �õ����� ��
				// ������ �̹� ������ ���·� �ش� ������ �߻��Ѵ�.
				case ERROR_NETNAME_DELETED:
				{
					if ( session != nullptr )
						 SessionManager::Inst().Erase( session );
				} break;

			}
		}
	}
}