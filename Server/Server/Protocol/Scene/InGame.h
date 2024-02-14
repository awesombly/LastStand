#pragma once
#include "Management/ProtocolSystem.h"

class InGame : public IScene
{
public:
	InGame()          = default;
	virtual ~InGame() = default;

private:
	static void AckChatMessage( const Packet& _packet );
	static void AckExitStage( const Packet& _packet );

	static void AckSpawnPlayer( const Packet& _packet );
	static void AckSpawnActor( const Packet& _packet );
	static void AckSynkMovement( const Packet& _packet );
	static void AckInGameLoadData( const Packet& _packet );

public:
	virtual void Bind() override;
};