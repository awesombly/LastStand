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

	bool Entry( Session* _session );
	bool Exit( Session* _session );

	void Broadcast( const UPacket& _packet ) const;
	void BroadcastWithoutSelf( Session* _session, const UPacket& _packet ) const;
};