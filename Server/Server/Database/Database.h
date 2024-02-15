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

public:
	LOGIN_INFO Search( const std::string& _type, const std::string& _data );
	bool Insert( const LOGIN_INFO& _data );
	bool Update( const LOGIN_INFO& _data );
	bool Delete( const LOGIN_INFO& _data );

private:
	bool Query( const char* _sentence );

public:
	Database() = default;
	virtual ~Database();
};