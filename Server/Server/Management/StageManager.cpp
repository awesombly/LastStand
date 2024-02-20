#include "StageManager.h"
#include "SessionManager.h"

void StageManager::Push( Stage* _stage )
{
	if ( _stage == nullptr )
		 return;

	Debug.Log( "# Stage ", _stage->info.serial, " has been created" );

	std::lock_guard<std::mutex> lock( mtx );
	stages.push_back( _stage );
}

void StageManager::Erase( Stage* _stage )
{
	if ( _stage == nullptr )
		 return;

	Debug.Log( "# Stage ", _stage->info.serial, " has been removed" );

	std::lock_guard<std::mutex> lock( mtx );
	for ( std::list<Stage*>::const_iterator iter = stages.begin(); iter != stages.end(); iter++ )
	{
		if ( ( *iter )->info.serial == _stage->info.serial )
		{
			stages.erase( iter );
			break;
		}
	}

	Global::Memory::SafeDelete( _stage );
}

const std::list<Stage*>& StageManager::GetStages() const
{
	return stages;
}

bool StageManager::Contains( SerialType _serial ) const
{
	for ( Stage* stage : stages )
	{
		if ( stage->info.serial == _serial )
			return true;
	}

	return false;
}

Stage* StageManager::Find( SerialType _serial ) const
{
	for ( Stage* stage : stages )
	{
		if ( stage->info.serial == _serial )
			return stage;
	}

	return nullptr;
}