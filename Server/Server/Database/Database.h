#pragma once
#include "Global/Singleton.hpp"
#include "mysql.h"

class Database : public Singleton<Database>
{
private:
	MYSQL*     conn;
	MYSQL_RES* data;

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