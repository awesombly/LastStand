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
	void Query( const char* _sentence );

public:
	void CreateUserData( const std::string& _nickname, const std::string& _email, const std::string& _password );
	void DeleteUserData( int _uid );

	bool ExistEmail( const std::string& _email );
	LOGIN_DATA GetLoginData( const std::string& _email );
	LOGIN_DATA GetLoginData( int _uid );
	USER_DATA  GetUserData( int _uid );
	void UpdateUserData( int _uid, const USER_DATA& _data );

	//LOGIN_DATA Search( const std::string& _type, const std::string& _data );
	//bool Insert( const LOGIN_DATA& _data );
	//bool Update( const LOGIN_DATA& _data );
	//bool Delete( const LOGIN_DATA& _data );

public:
	Database() = default;
	virtual ~Database();
};