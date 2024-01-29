#pragma once

template<class Type>
static void SafeDelete( Type*& _pointer )
{
	if ( _pointer != nullptr )
	{
		delete _pointer;
		_pointer = nullptr;
	}
}

template<class Type>
static void SafeDeleteArray( Type*& _pointer )
{
	if ( _pointer != nullptr )
	{
		delete[] _pointer;
		_pointer = nullptr;
	}
}