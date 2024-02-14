#include "Stage.h"
#include "Management/SessionManager.h"

Stage::Stage( Session* _host, const STAGE_INFO& _info ) : host( _host ), info( _info )
{
	std::cout << "# Stage " << _info.serial << " The host has been changed< " << host->loginInfo.nickname << " >" << std::endl;
	sessions.push_back( host );
}

bool Stage::Entry( Session* _session )
{
	if ( sessions.size() + 1 > info.personnel.maximum )
	{
		std::cout << "The stage is full of people" << std::endl;
		return false;
	}

	sessions.push_back( _session );
	info.personnel.current = ( int )sessions.size();

	return true;
}

bool Stage::Exit( Session* _session )
{
	if ( sessions.size() <= 0 )
		 throw std::exception( "There's no one in the stage" );

	sessions.erase( std::find( sessions.begin(), sessions.end(), _session ) );
	info.personnel.current = ( int )sessions.size();

	if ( sessions.size() > 0 && host->GetSocket() == _session->GetSocket() )
	{
		 host = *sessions.begin();
		 std::cout << "# Stage " << info.serial << " The host has been changed< " << host->loginInfo.nickname << " >" << std::endl;
	}

	return sessions.size() > 0;
}