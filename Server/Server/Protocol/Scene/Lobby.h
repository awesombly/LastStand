#pragma once
#include "Managed/ProtocolSystem.h"
#include "Stage/Stage.h"

class Lobby : public IScene
{
private:
	static std::list<Stage*> stages;
	static std::list<STAGE_INFO> infos;

public:
	Lobby()          = default;
	virtual ~Lobby() = default;

private:
	static void CreateStage( const Packet& _packet );
	static void TakeLobbyInfo( const Packet& _packet );

public:
	virtual void Bind() override;
};