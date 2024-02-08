#pragma once
#include "Global/Singleton.hpp"
#include "mysql.h"

struct UserData
{
	std::string nickname;
	std::string email;
	std::string password;
};

class Database : public Singleton<Database>
{
private:
	static const int MaxSentenceSize = 1024;

	MYSQL driver;
	MYSQL* conn;
	MYSQL_RES* result;
	char sentence[MaxSentenceSize];

public:
	bool Initialize();

public:
	UserData Search( const std::string& _type, const std::string& _data );
	bool Insert( const UserData& _data );
	bool Update( const UserData& _data );
	bool Delete( const UserData& _data );

private:
	bool Query( const char* _sentence );

public:
	Database()          = default;
	virtual ~Database();
};