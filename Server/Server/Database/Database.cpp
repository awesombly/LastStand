#include "Database.h"

bool Database::Initialize()
{
	//{
		// get_driver_instance는 쓰레드로 안전하지않음
		//std::lock_guard<std::mutex> lock( mtx );
	//}

	try
	{
		driver = sql::mysql::get_driver_instance();
		connection = driver->connect( "tcp://127.0.0.1:3306", "root", "0000" );
	}
	catch ( std::exception _error )
	{
		std::cout << _error.what() << std::endl;
	}

	//connection = driver->connect( "tcp://127.0.0.1:3306", "localhost", "0000" );
	connection->setSchema( "VSLike" );

	if ( !connection->isValid() )
	{
		std::cout << "Database connection failed" << std::endl;
		return false;
	}
	std::cout << "Database connected" << std::endl;

	//statement = connection->createStatement();

	//// DB의 Column은 1부터 시작.
	//result = statement->executeQuery( "select * from user" );

	//
	//while ( result->next() )
	//{
	//	std::cout << result->getString("Name") << " " << 
	//		         result->getString("ID")   << " " << 
	//				 result->getString("PW")   << " " << std::endl;
	//}


	//delete statement;
	//delete result;
	delete connection;

	return true;
}