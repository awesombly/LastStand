#pragma once
#include "Global/Header.h"
#include "Global/Singleton.hpp"
#include "jdbc/mysql_driver.h"
#include "jdbc/mysql_connection.h"
#include "jdbc/mysql_error.h"

class Database : Singleton<Database>
{
public:
	static sql::Driver* driver;
	std::mutex mtx;

public:
	bool Initialize() const;

public:
	Database()          = default;
	virtual ~Database() = default;
};