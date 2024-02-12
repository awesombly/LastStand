#pragma once
#include "Global/Singleton.hpp"
#include "Network/Session.h"
#include "Stage/Stage.h"

class StageManager : public Singleton<StageManager>
{
private:
	std::unordered_map<SerialType, Stage*> stages;
	std::mutex mtx;

public:
	StageManager()          = default;
	virtual ~StageManager() = default;

public:
	std::unordered_map<SerialType, Stage*> GetStages() const;

public:
	void CreateStage( const SOCKET& _host, const STAGE_INFO& _info );
};