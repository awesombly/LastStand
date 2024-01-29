#pragma once
#include "../Synchronize/CriticalSection.h"

template<class Type>
class Singleton
{
public:
	Singleton() = default;
	virtual ~Singleton() = default;

public:
	static Type& Inst()
	{
		std::call_once( flag, [&]()
		{
			CriticalSection cs;
			cs.Lock();
			if ( inst.get() == nullptr )
			{
				inst = std::make_shared<Type>();
			}
			cs.UnLock();
		} );

		return *inst.get();
	}

private:
	static std::once_flag flag;
	static std::shared_ptr<Type> inst;
};

template<class Type>
std::once_flag Singleton<Type>::flag;

template<class Type>
std::shared_ptr<Type> Singleton<Type>::inst;