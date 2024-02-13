#include "Lobby.h"
#include "Management/ProtocolSystem.h"
#include "Management/SessionManager.h"

void Lobby::Bind()
{
	ProtocolSystem::Inst().Regist( STAGE_INFO_REQ,   AckStageList );
	ProtocolSystem::Inst().Regist( CREATE_STAGE_REQ, AckCreateStage );
	ProtocolSystem::Inst().Regist( ENTRY_STAGE_REQ,  AckEntryStage );
}

void Lobby::AckCreateStage( const Packet& _packet )
{
	STAGE_INFO data = FromJson<STAGE_INFO>( _packet );
	CONFIRM confirm;
	confirm.isCompleted = data.title.size() > 0;

	Session* session = _packet.session;
	if ( confirm.isCompleted )
	{
		STAGE_INFO stageData;
		stageData.serial = Global::GetNewSerial();
		stageData.title = data.title;
		stageData.personnel.maximum = data.personnel.maximum;
		stageData.personnel.current = 1;

		SessionManager::Inst().AddStage( new Stage( session, stageData ) );
	}

	session->Send( UPacket( CREATE_STAGE_ACK, confirm ) );
}

void Lobby::AckEntryStage( const Packet& _packet )
{
	STAGE_INFO data = FromJson<STAGE_INFO>( _packet );
	SessionManager::Inst().UpdateStage( data.serial, _packet.session );

	_packet.session->Send( UPacket( ENTRY_STAGE_ACK, CONFIRM( true ) ) );
}

void Lobby::AckStageList( const Packet& _packet )
{
	for ( const std::pair<SerialType, Stage*>& pair : SessionManager::Inst().GetStages() )
	{
		_packet.session->Send( UPacket( STAGE_INFO_ACK, pair.second->info ) );
	}
}