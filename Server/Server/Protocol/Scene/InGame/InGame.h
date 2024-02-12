#pragma once
#include "Managed/ProtocolSystem.h"

class InGame : public IScene
{
public:
	InGame()          = default;
	virtual ~InGame() = default;

public:
	virtual void Bind() override;
};