#include "Stage.h"
#include "Management/SessionManager.h"

Stage::Stage( Session* _host, const STAGE_INFO& _info ) : host( _host ), info( _info )
{
	sessions.push_back( host );
}

void Stage::Entry( Session* _session )
{
	if ( sessions.size() + 1 >= Global::MaxStagePersonnel )
	{
		std::cout << "The stage is full of people" << std::endl;
		return;
	}

	sessions.push_back( _session );
	info.personnel.current = ( int )sessions.size();
}