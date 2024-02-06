#pragma once
#include "Global/Singleton.hpp"
#include "mysql.h"

struct UserData
{
	std::string nickname;
	std::string id;
	std::string pw;
};

class Database : public Singleton<Database>
{
private:
	MYSQL*     conn;
	MYSQL_RES* result;
	char sentence[256];

public:
	bool Initialize();

public:
	UserData Search( const char* _id );

private:
	bool Query( const char* _query );

public:
	Database()          = default;
	virtual ~Database();
};