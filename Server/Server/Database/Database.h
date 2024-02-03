#pragma once
//#include "Global/Header.h"
#include "Global/Singleton.hpp"

#include "mysql/jdbc.h"

//#include "jdbc/cppconn/driver.h"
//#include "jdbc/cppconn/connection.h"
//#include "jdbc/cppconn/statement.h"
//#include "jdbc/cppconn/resultset.h"
//#include "jdbc/cppconn/sqlstring.h"
//#include "jdbc/cppconn/exception.h"

class Database : public Singleton<Database>
{
private:
	sql::Driver* driver;
	sql::Connection* connection;
	sql::Statement* statement;
	sql::ResultSet* result;

	//std::mutex mtx;

public:
	bool Initialize();

public:
	Database()          = default;
	virtual ~Database() = default;
};

struct UserData
{
	std::string Name;
	std::string ID;
	std::string PW;
};