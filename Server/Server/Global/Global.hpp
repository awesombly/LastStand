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

static std::string ToUFT8( const char* _string )
{
	int lengthUnicode = 0;
	int lengthUTF = 0;
	wchar_t* UnicodeBuffer = nullptr;
	char* multibyteBuffer = nullptr;

	if ( ( lengthUnicode = ::MultiByteToWideChar( CP_ACP, 0, _string, ( int )::strlen( _string ), NULL, 0 ) ) < 0 )
	{
		return std::string();
	}
	lengthUnicode = lengthUnicode + ( int )1;
	UnicodeBuffer = new wchar_t[lengthUnicode];
	::memset( UnicodeBuffer, 0x00, ( size_t )( sizeof( wchar_t ) * lengthUnicode ) );

	// Ansi -> Unicode
	lengthUnicode = ::MultiByteToWideChar( CP_ACP, 0, _string, ( int )::strlen( _string ), UnicodeBuffer, lengthUnicode );
	if ( ( lengthUTF = ::WideCharToMultiByte( CP_UTF8, 0, UnicodeBuffer, lengthUnicode, NULL, 0, NULL, NULL ) ) < 0 )
	{
		delete[] UnicodeBuffer;

		return std::string();
	}
	lengthUTF = lengthUTF + ( int )1;
	multibyteBuffer = new char[lengthUTF];
	::memset( multibyteBuffer, 0x00, ( size_t )( sizeof( char ) * lengthUTF ) );

	// Unicode -> UTF-8
	lengthUTF = ::WideCharToMultiByte( CP_UTF8, 0, UnicodeBuffer, lengthUnicode, multibyteBuffer, lengthUTF, NULL, NULL );
	multibyteBuffer[lengthUTF] = 0;
	std::string result( multibyteBuffer );

	delete[] UnicodeBuffer;
	delete[] multibyteBuffer;

	return result;
}

static std::string ToAnsi( const char* _string )
{
	int lengthUnicode = 0;
	int lengthUTF = 0;
	wchar_t* UnicodeBuffer = nullptr;
	char* multibyteBuffer = nullptr;

	if ( ( lengthUnicode = ::MultiByteToWideChar( CP_UTF8, 0, _string, ( int )::strlen( _string ), NULL, 0 ) ) < 0 )
	{
		return std::string();
	}
	UnicodeBuffer = UnicodeBuffer + ( int )1;
	UnicodeBuffer = new wchar_t[lengthUnicode];
	::memset( UnicodeBuffer, 0x00, ( size_t )( sizeof( wchar_t ) * lengthUnicode ) );

	// UTF-8 -> Unicode
	lengthUnicode = ::MultiByteToWideChar( CP_UTF8, 0, _string, ( int )::strlen( _string ), UnicodeBuffer, lengthUnicode );
	if ( ( lengthUTF = ::WideCharToMultiByte( CP_ACP, 0, UnicodeBuffer, lengthUnicode, NULL, 0, NULL, NULL ) ) < 0 )
	{
		delete[] UnicodeBuffer;

		return std::string();
	}
	lengthUTF = lengthUTF + ( int )1;
	multibyteBuffer = new char[lengthUTF];
	::memset( multibyteBuffer, 0x00, ( size_t )( sizeof( char ) * lengthUTF ) );

	// Unicode -> Ansi
	lengthUTF = ::WideCharToMultiByte( CP_ACP, 0, UnicodeBuffer, lengthUnicode, multibyteBuffer, lengthUTF, NULL, NULL );
	multibyteBuffer[lengthUTF] = 0;
	std::string result( multibyteBuffer );

	delete[] UnicodeBuffer;
	delete[] multibyteBuffer;

	return result;
}