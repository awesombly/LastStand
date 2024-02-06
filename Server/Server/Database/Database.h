#pragma once
#include "mysql.h"
#include "Global/Singleton.hpp"

class Database : public Singleton<Database>
{
private:


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