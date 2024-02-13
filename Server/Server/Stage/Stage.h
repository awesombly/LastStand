#pragma once
#include "Network/Session.h"

class Stage
{
public:
	STAGE_INFO info;
	Session* host;

private:
	std::list<Session*> sessions;

public:
	Stage( Session* _host, const STAGE_INFO& _info );
	virtual ~Stage() = default;

public:
	void Entry( Session* _session );
};