#pragma once
#include "../../Connect/Session.h"

class Stage
{
private:
	STAGE_INFO info;
	Session* host;
	std::list<Session*> sessions;

public:
	Stage( const SOCKET& _host, const STAGE_INFO& _info );
	virtual ~Stage() = default;

public:
	const STAGE_INFO& GetInfo() const;
};