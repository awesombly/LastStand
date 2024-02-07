#pragma once
#include "Global/Singleton.hpp"
#include "mysql.h"

struct UserData
{
	const char* nickname;
	const char* email;
	const char* password;
};

class Database : public Singleton<Database>
{
private:
	static const int MaxSentenceSize = 1024;

	MYSQL*     conn;
	MYSQL_RES* result;
	char sentence[MaxSentenceSize];

public:
	bool Initialize();

public:
	UserData Search( const char* _id );
	bool Insert( const UserData& _data );
	bool Update( const UserData& _data );
	bool Delete( const UserData& _data );

private:
	bool Query( const char* _sentence );

public:
	Database()          = default;
	virtual ~Database();
};