#include "Lobby.h"
#include "Management/ProtocolSystem.h"
#include "Management/SessionManager.h"
#include "Management/StageManager.h"

void Lobby::Bind()
{
	ProtocolSystem::Inst().Regist( STAGE_INFO_REQ,   AckStageList );
	ProtocolSystem::Inst().Regist( CREATE_STAGE_REQ, AckCreateStage );
	ProtocolSystem::Inst().Regist( ENTRY_STAGE_REQ,  AckEntryStage );
}

void Lobby::AckCreateStage( const Packet& _packet )
{
	const STAGE_INFO& data = FromJson<STAGE_INFO>( _packet );
	Session* session = _packet.session;

	// ������ ����
	STAGE_INFO stageData;
	stageData.stageSerial = Global::GetNewSerial();
	stageData.hostSerial = session->serial;
	stageData.title = data.title;
	stageData.targetKill = data.targetKill;
	stageData.currentKill = 0;
	stageData.personnel.maximum = data.personnel.maximum;
	stageData.personnel.current = 1;

	// �� ����
	StageManager::Inst().Push( new Stage( session, stageData ) );

	// ���� : ���� ��Ŷ, ���� : ���� ��Ŷ ����
	SessionManager::Inst().BroadcastWaitingRoom( UPacket( INSERT_STAGE_INFO, stageData ) );
	session->Send( UPacket( CREATE_STAGE_ACK, stageData ) );
}

void Lobby::AckEntryStage( const Packet& _packet )
{
	Session* session       = _packet.session;
	try
	{
		const STAGE_INFO& data = FromJson<STAGE_INFO>( _packet );

		Stage* stage = StageManager::Inst().Find( data.stageSerial );
		stage->Entry( session );

		session->Send( UPacket( ENTRY_STAGE_ACK, stage->info ) );
		SessionManager::Inst().BroadcastWaitingRoom( UPacket( UPDATE_STAGE_INFO, stage->info ) );
	}
	catch ( Result _error )
	{
		session->Send( UPacket( _error, ENTRY_STAGE_ACK ) );
	}
}

void Lobby::AckStageList( const Packet& _packet )
{
	for ( Stage* stage : StageManager::Inst().GetStages() )
	{
		_packet.session->Send( UPacket( STAGE_INFO_ACK, stage->info ) );
	}
}