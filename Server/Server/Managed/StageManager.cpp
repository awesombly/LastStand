#include "StageManager.h"
#include "SessionManager.h"

std::unordered_map<SerialType, Stage*> StageManager::GetStages() const
{
	return stages;
}

void StageManager::CreateStage( const SOCKET& _host, const STAGE_INFO& _info )
{
	if ( stages.contains( _info.serial ) )
	{
		std::cout << " The stage already exists" << std::endl;
	}

	Session* host = SessionManager::Inst().Find( _host );
	std::cout << "Create a new Stage( " << host->GetPort() << ", " << host->GetAddress() << " )" << std::endl;
	std::lock_guard<std::mutex> lock( mtx );
	{
		stages[_info.serial] = new Stage( _host, _info );
	}
}