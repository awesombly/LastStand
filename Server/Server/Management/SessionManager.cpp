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
		SOCKET socket = _session->GetSocket();
		_session->ClosedSocket();
		Global::Memory::SafeDelete( _session );
		sessions.erase( socket );
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
		if ( login.nickname.empty() && !session->isPlaying )
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

void SessionManager::AddStage( Stage* _stage )
{
	STAGE_INFO data = _stage->info;
	if ( stages.contains( data.serial ) )
		 return;

	Session* host = _stage->host;
	std::cout << "Create a new Stage( " << data.serial << " )" << std::endl;
	std::lock_guard<std::mutex> lock( mtx );
	{
		stages[data.serial] = _stage;
		host->isPlaying = true;
	}

	BroadcastWaitingRoom( UPacket( INSERT_STAGE_INFO, data ) );
}

void SessionManager::UpdateStage( const SerialType& _serial, Session* _session )
{
	if ( !stages.contains( _serial ) )
	{
		std::cout << "This stage does not exist" << std::endl;
		return;
	}

	Stage* stage = stages[_serial];
	std::cout << "The session has entered Stage " << _serial << "( " << _session->GetPort() << ", " << _session->GetAddress() << " )" << std::endl;
	std::lock_guard<std::mutex> lock( mtx );
	{
		stage->Entry( _session );
		_session->isPlaying = true;
	}

	BroadcastWaitingRoom( UPacket( UPDATE_STAGE_INFO, stage->info ) );
}
#pragma endregion