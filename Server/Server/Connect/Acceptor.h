#pragma once
#include "../Network/Network.h"

class Acceptor : public Network
{
public:
	Acceptor() = default;
	virtual ~Acceptor();

public:
	bool Listen();

private:
	void WaitForClients()  const;
	bool SetSocketOption() const;
};