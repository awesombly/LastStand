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
		std::this_thread::sleep_for( std::chrono::milliseconds( 100 ) );
		std::lock_guard<std::mutex> lock( mtx );
		for ( const std::pair<SOCKET, Session*>& pair : sessions )
		{
			Session* session = pair.second;
			if ( !session->CheckAlive() )
			{
				Debug.Log( "# Remove unresponsive session ( ", session->GetPort(), ", ", session->GetAddress(), " )" );
				unAckSessions.push( session );
			}
		}

		while ( !unAckSessions.empty() )
		{
			Session* session = unAckSessions.front();
			if ( session->stage != nullptr )
				 ExitStage( session );

			sessions.erase( session->GetSocket() );
			session->ClosedSocket();
			Global::Memory::SafeDelete( session );
			unAckSessions.pop();
		}
	}
}
#pragma endregion

#pragma region Full Management
void SessionManager::Push( Session* _session )
{
	if ( _session == nullptr )
		 return;

	Debug.Log( "# Register a new session ( ", _session->GetPort(), ", ", _session->GetAddress(), " )" );

	std::lock_guard<std::mutex> lock( mtx );
	sessions[_session->GetSocket()] = _session;
}

void SessionManager::Erase( Session* _session )
{
	if ( _session == nullptr )
		return;

	Debug.Log( "# The session has left ( ", _session->GetPort(), ", ", _session->GetAddress(), " )" );
	
	std::lock_guard<std::mutex> lock( mtx );
	unAckSessions.push( _session );
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

void SessionManager::BroadcastWithoutSelf( Session* _session, const UPacket& _packet ) const
{
	for ( const std::pair<SOCKET, Session*>& pair : sessions )
	{
		Session* session = pair.second;
		if ( session->GetSocket() != _session->GetSocket() )
			 session->Send( _packet );
	}
}

void SessionManager::BroadcastWaitingRoom( const UPacket& _packet )
{
	for ( const std::pair<SOCKET, Session*>& pair : sessions )
	{
		Session* session = pair.second;
		if ( session->stage == nullptr )
			 session->Send( _packet );
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

void SessionManager::ExitStage( Session* _session )
{
	if ( _session == nullptr || _session->stage == nullptr )
	{
		Debug.LogError( "# The session has not entered the stage yet" );
		return;
	}

	Stage* stage      = _session->stage;
	SerialType serial = stage->info.serial;
	if ( !stages.contains( stage->info.serial ) )
	{
		Debug.LogError( "# This stage does not exist" );
		return;
	}

	if ( _session->player != nullptr )
	{
		stage->BroadcastWithoutSelf( _session, UPacket( REMOVE_PLAYER_ACK, *_session->player ) );
		_session->stage->UnregistActor( _session->player );
		Global::Memory::SafeDelete( _session->player );
	}

	if ( !stage->Exit( _session ) )
	{
		// 방에 아무도 없을 때
		Debug.Log( "# Stage ", serial, " has been removed" );
		BroadcastWaitingRoom( UPacket( DELETE_STAGE_INFO, stage->info ) );
		Global::Memory::SafeDelete( stage );
		stages.erase( serial );
	}
	else
	{
		BroadcastWaitingRoom( UPacket( UPDATE_STAGE_INFO, stage->info ) );
	}
	_session->stage = nullptr;
}

// return : 변경 및 생성된 스테이지
Stage* SessionManager::EntryStage( Session* _session, const STAGE_INFO& _info )
{
	Stage* stage = nullptr;
	SerialType serial = _info.serial;
	if ( !stages.contains( serial ) )
	{
		// 방 생성하고 입장
		Debug.Log( "# Stage ", serial, " has been created" );
		stage = new Stage( _session, _info );
		stages[serial]  = stage;
		_session->stage = stage;
		BroadcastWaitingRoom( UPacket( INSERT_STAGE_INFO, stage->info ) );
	}
	else
	{
		// 이미 존재하는 방에 입장( 풀방일 땐 입장하지않음 )
		stage = stages[serial];
		if ( stage->Entry( _session ) )
		{
			Debug.Log( "# < ", _session->loginInfo.nickname, " > has entered stage ", serial );
			_session->stage = stage;
			_session->Send( UPacket( ENTRY_STAGE_ACK, stage->info ) );
			BroadcastWaitingRoom( UPacket( UPDATE_STAGE_INFO, stage->info ) );
		}
	}

	return stage;
}
#pragma endregion