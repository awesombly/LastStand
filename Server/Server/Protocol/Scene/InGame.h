#pragma once
#include "Management/ProtocolSystem.h"

class InGame : public IScene
{
public:
	InGame()          = default;
	virtual ~InGame() = default;

private:
	static void SpawnPlayer( const Packet& _packet );
	static void SpawnActor( const Packet& _packet );
	static void SynkMovement( const Packet& _packet );

public:
	virtual void Bind() override;
};