#include "Database.h"

sql::Driver* Database::driver = nullptr;

bool Database::Initialize() const
{
	//std::lock_guard<std::mutex> lock( mtx );
	// 쓰레드로 안전하지않음
	driver = get_driver_instance();

	sql::Connection* connection = driver->connect( "tcp://127.0.0.1:3306", "root", "0000" );
	connection->setSchema( "VSLike" );

	return true;
}