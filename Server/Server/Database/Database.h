#pragma once
#include "Global/Singleton.hpp"
#include "mysql.h"
#include "Protocol/Protocol.hpp"
#include "Global/LogText.hpp"

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

private:
	bool Query( const char* _sentence );

public:
	bool CreateUserData( const std::string& _nickname, const std::string& _email, const std::string& _password );
	bool DeleteUserData( int _uid );

	LOGIN_DATA GetLoginData( const std::string& _email );
	LOGIN_DATA GetLoginData( int _uid );
	USER_DATA  GetUserData( int _uid );

	//LOGIN_DATA Search( const std::string& _type, const std::string& _data );
	//bool Insert( const LOGIN_DATA& _data );
	//bool Update( const LOGIN_DATA& _data );
	//bool Delete( const LOGIN_DATA& _data );

public:
	Database() = default;
	virtual ~Database();
};