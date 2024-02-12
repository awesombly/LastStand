#include "Stage.h"
#include "Managed/SessionManager.h"

Stage::Stage( const SOCKET& _host, const STAGE_INFO& _info ) : info( _info )
{
	host = SessionManager::Inst().Find( _host );
	sessions.push_back( host );
}

const STAGE_INFO& Stage::GetInfo() const { return info; }