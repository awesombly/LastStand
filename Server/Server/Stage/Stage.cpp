#include "Stage.h"
#include "Management/SessionManager.h"

Stage::Stage( const SOCKET& _host, const STAGE_INFO& _info ) : info( _info )
{
	host = SessionManager::Inst().Find( _host );
	sessions.push_back( host );
}

const STAGE_INFO& Stage::GetInfo() const { return info; }

void Stage::Entry( Session* _session )
{
	if ( sessions.size() + 1 <= Global::MaxStagePersonnel )
	{
		std::cout << "The stage is full of people" << std::endl;
		return;
	}

	sessions.push_back( _session );
	info.personnel.current = ( int )sessions.size();
}