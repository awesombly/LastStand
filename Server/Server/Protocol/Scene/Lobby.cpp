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

	// 데이터 생성
	STAGE_INFO stageData;
	stageData.serial = Global::GetNewSerial();
	stageData.title = data.title;
	stageData.targetKill = data.targetKill;
	stageData.personnel.maximum = data.personnel.maximum;
	stageData.personnel.current = 1;

	// 방 생성
	StageManager::Inst().Push( new Stage( session, stageData ) );

	// 대기실 : 갱신 패킷, 세션 : 입장 패킷 전송
	SessionManager::Inst().BroadcastWaitingRoom( UPacket( INSERT_STAGE_INFO, stageData ) );
	session->Send( UPacket( CREATE_STAGE_ACK, stageData ) );
}

void Lobby::AckEntryStage( const Packet& _packet )
{
	Session* session = _packet.session;
	const STAGE_INFO& data = FromJson<STAGE_INFO>( _packet );

	try
	{
		Stage* stage = StageManager::Inst().Find( data.serial );
		if ( stage == nullptr )
			 throw std::exception( "# This stage does not exist" );

		if ( stage->Entry( session ) )
		{
			Debug.Log( "# < ", session->loginInfo.nickname, " > has entered stage ", data.serial );
			session->Send( UPacket( ENTRY_STAGE_ACK, stage->info ) );
			SessionManager::Inst().BroadcastWaitingRoom( UPacket( UPDATE_STAGE_INFO, stage->info ) );
		}
	}
	catch ( std::exception _error )
	{
		Debug.LogWarning( _error.what() );
	}
}

void Lobby::AckStageList( const Packet& _packet )
{
	for ( Stage* stage : StageManager::Inst().GetStages() )
	{
		_packet.session->Send( UPacket( STAGE_INFO_ACK, stage->info ) );
	}
}