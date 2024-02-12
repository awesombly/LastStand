#include "Lobby.h"
#include "Management/ProtocolSystem.h"
#include "Management/SessionManager.h"

void Lobby::Bind()
{
	ProtocolSystem::Inst().Regist( CREATE_STAGE_REQ, CreateStage );
	ProtocolSystem::Inst().Regist( LOBBY_INFO_REQ,   TakeLobbyInfo );
	ProtocolSystem::Inst().Regist( ENTRY_STAGE_REQ,  EntryStage );
}

void Lobby::CreateStage( const Packet& _packet )
{
	STAGE_INFO data = FromJson<STAGE_INFO>( _packet );
	CONFIRM confirm;
	confirm.isCompleted = data.title.size() > 0;
	if ( confirm.isCompleted )
	{
		STAGE_INFO stageData;
		stageData.serial            = Global::GetNewSerial();
		stageData.title             = data.title;
		stageData.personnel.maximum = data.personnel.maximum;
		stageData.personnel.current = 1;

		SessionManager::Inst().CreateStage( _packet.socket, stageData );
		
		SessionManager::Inst().Broadcast( UPacket( INSERT_STAGE_INFO, stageData ) );
	}

	SessionManager::Inst().Send( _packet.socket, UPacket( CREATE_STAGE_ACK, confirm ) );
}

void Lobby::TakeLobbyInfo( const Packet& _packet )
{
	for ( const std::pair<SerialType, Stage*>& pair : SessionManager::Inst().GetStages() )
	{
		SessionManager::Inst().Send( _packet.socket, UPacket( LOBBY_INFO_ACK, pair.second->GetInfo() ) );
		::Sleep( 1 );
	}
}

void Lobby::EntryStage( const Packet& _packet )
{
	STAGE_INFO data = FromJson<STAGE_INFO>( _packet );
	
	// 바뀐 스테이지
	SessionManager::Inst().EntryStage( _packet.socket, data );
}