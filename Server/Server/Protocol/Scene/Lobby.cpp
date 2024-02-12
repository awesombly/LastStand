#include "Lobby.h"
#include "Managed/ProtocolSystem.h"
#include "Managed/SessionManager.h"

u_short Lobby::StageUID = 0;
std::list<Stage*> Lobby::stages;
std::list<STAGE_INFO> Lobby::infos;

void Lobby::Bind()
{
	ProtocolSystem::Inst().Regist( CREATE_STAGE_REQ, CreateStage );
	ProtocolSystem::Inst().Regist( LOBBY_INFO_REQ,  TakeLobbyInfo );
}

void Lobby::CreateStage( const Packet& _packet )
{
	STAGE_INFO data = FromJson<STAGE_INFO>( _packet );
	CONFIRM confirm;
	confirm.isCompleted = data.title.size() > 0;
	if ( confirm.isCompleted )
	{
		STAGE_INFO stageData;
		stageData.uid       = StageUID++;
		stageData.title     = data.title;
		stageData.personnel.maximum = data.personnel.maximum;
		stageData.personnel.current = 1;

		stages.push_back( new Stage( _packet.socket, stageData ) );
		infos.push_back( stageData );

		SessionManager::Inst().Broadcast( _packet.socket, UPacket( INSERT_STAGE_INFO, stageData ) );
	}

	SessionManager::Inst().Send( _packet.socket, UPacket( CREATE_STAGE_ACK, confirm ) );
}

void Lobby::TakeLobbyInfo( const Packet& _packet )
{
	LOBBY_INFO info;
	info.infos = infos;

	SessionManager::Inst().Send( _packet.socket, UPacket( LOBBY_INFO_ACK, info ) );
}