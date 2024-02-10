#pragma once
#include "Managed/ProtocolSystem.h"

class Login : public IScene
{
public:
	virtual void Bind() override;

private:
	static void ConfirmLogin( const Packet& _packet );
	static void RequestSignUp( const Packet& _packet );
	static void RequestSignUpMail( const Packet& _packet );
};