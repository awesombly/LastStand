#include "SessionManager.h"
#include "ProtocolSystem.h"
#include "Protocol/Protocol.hpp"

SessionManager::~SessionManager()
{
	auto pair( std::begin( sessions ) );
	while ( pair++ != std::end( sessions ) )
	{
		Global::Memory::SafeDelete( pair->second );
	}

	sessions.clear();
}

#pragma region Default
bool SessionManager::Initialize()
{
	std::cout << "Create thread for unresponsive session processing" << std::endl;
	std::cout << "Check for unresponsive sessions" << std::endl;
	std::thread th( [&]() { ConfirmDisconnect(); } );
	th.detach();

	return true;
}

void SessionManager::ConfirmDisconnect()
{
	while ( true )
	{
		for ( const std::pair<SOCKET, Session*>& pair : sessions )
		{
			Session* session = pair.second;
			if ( !session->CheckAlive() )
			{
				std::cout << "Remove unresponsive session( " << session->GetPort() << ", " << session->GetAddress() << " )" << std::endl;
				unAckSessions.push( session );
			}
		}

		std::lock_guard<std::mutex> lock( mtx );
		{
			while ( !unAckSessions.empty() )
			{
				Session* session = unAckSessions.front();
					
				sessions.erase( session->GetSocket() );
				session->ClosedSocket();
				Global::Memory::SafeDelete( session );
				unAckSessions.pop();
			}
		}
	}
}
#pragma endregion

#pragma region Full Management
void SessionManager::Push( Session* _session )
{
	if ( _session == nullptr )
		 return;

	std::cout << "Register a new session( " << _session->GetPort() << ", " << _session->GetAddress() << " )" << std::endl;
	std::lock_guard<std::mutex> lock( mtx );
	{
		sessions[_session->GetSocket()] = _session;
	}
}

void SessionManager::Erase( Session* _session )
{
	if ( _session == nullptr )
		return;

	std::cout << "The session has left( " << _session->GetPort() << ", " << _session->GetAddress() << " )" << std::endl;
	std::lock_guard<std::mutex> lock( mtx );
	{
		unAckSessions.push( _session );
		// SOCKET socket = _session->GetSocket();
		// _session->ClosedSocket();
		// Global::Memory::SafeDelete( _session );
		// sessions.erase( socket );
	}
}

Session* SessionManager::Find( const SOCKET& _socket ) const
{
	const auto& iter = sessions.find( _socket );
	if ( iter == std::cend( sessions ) )
		 return nullptr;

	return iter->second;
}

void SessionManager::Broadcast( const UPacket& _packet ) const
{
	for ( const std::pair<SOCKET, Session*>& pair : sessions )
	{
		Session* session = pair.second;
		session->Send( _packet );
	}
}

void SessionManager::BroadcastWithoutSelf( const SOCKET& _socket, const UPacket& _packet ) const
{
	for ( const std::pair<SOCKET, Session*>& pair : sessions )
	{
		Session* session = pair.second;
		if ( session->GetSocket() != _socket )
			 session->Send( _packet );
	}
}

void SessionManager::BroadcastWaitingRoom( const UPacket& _packet )
{
	for ( const std::pair<SOCKET, Session*>& pair : sessions )
	{
		Session* session = pair.second;

		LOGIN_INFO login = session->loginInfo;
		if ( !session->isPlaying ) //login.nickname.empty() && !session->isPlaying )
		{
			session->Send( _packet );
		}
	}
}

std::unordered_map<SOCKET, Session*> SessionManager::GetSessions() const
{
	return sessions;
}
#pragma endregion

#pragma region Stage Management
const std::unordered_map<SerialType, Stage*>& SessionManager::GetStages() const
{
	return stages;
}

void SessionManager::ExitStage( Session* _session, const STAGE_INFO& _info )
{
	SerialType serial = _info.serial;
	if ( !stages.contains( serial ) )
	{
		std::cout << "This stage does not exist" << std::endl;
		return;
	}

	std::cout << "\"" << _session->loginInfo.nickname << "\" exit stage " << serial << std::endl;
	Stage* stage = stages[serial];
	std::lock_guard<std::mutex> lock( mtx );
	{
		if ( !stage->Exit( _session ) )
		{
			// 방에 아무도 없을 때
			BroadcastWaitingRoom( UPacket( DELETE_STAGE_INFO, stage->info ) );
			Global::Memory::SafeDelete( stage );
			stages.erase( serial );
		}
		else
		{
			BroadcastWaitingRoom( UPacket( UPDATE_STAGE_INFO, stage->info ) );
		}
	}
	_session->isPlaying = false;
}

// return : 변경 및 생성된 스테이지
Stage* SessionManager::EntryStage( Session* _session, const STAGE_INFO& _info )
{
	Stage* stage = nullptr;
	SerialType serial = _info.serial;
	_session->isPlaying = true;
	if ( !stages.contains( serial ) )
	{
		// 방 생성하고 입장
		std::cout << "\"" << _session->loginInfo.nickname << "\" created stage " << serial << std::endl;
		std::lock_guard<std::mutex> lock( mtx );
		stage = new Stage( _session, _info );
		stages[serial] = stage;
		BroadcastWaitingRoom( UPacket( INSERT_STAGE_INFO, stage->info ) );
	}
	else
	{
		// 이미 존재하는 방에 입장
		stage = stages[serial];
		std::cout << "\"" << _session->loginInfo.nickname << "\" entered stage " << serial << std::endl;
		std::lock_guard<std::mutex> lock( mtx );
		stage->Entry( _session );
		BroadcastWaitingRoom( UPacket( UPDATE_STAGE_INFO, stage->info ) );
	}

	return stage;
}
#pragma endregion