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

	static void AckSpawnActor( const Packet& _packet );
	static void AckSpawnPlayer( const Packet& _packet );
	static void AckSpawnBullet( const Packet& _packet );
	static void AckRemoveActors( const Packet& _packet );
	
	static void AckSyncMovement( const Packet& _packet );
	static void AckSyncReload( const Packet& _packet );
	static void AckSyncLook( const Packet& _packet );
	static void AckSyncDodgeAction( const Packet& _packet );
	static void AckSyncSwapWeapon( const Packet& _packet );
	static void AckSyncInteraction( const Packet& _packet );
	static void AckHitActors( const Packet& _packet );

	static void AckInitSceneActors( const Packet& _packet );
	static void AckInGameLoadData( const Packet& _packet );
	static void AckUpdateResultData( const Packet& _packet );

public:
	virtual void Bind() override;
};