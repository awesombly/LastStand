#pragma once

using SerialType = u_int;
namespace Global 
{
	static const u_short HeaderSize  = 4;
	static const u_short MaxDataSize = 2048;

	static const u_short MaxStagePersonnel = 4;

	static SerialType GetNewSerial()
	{
		static SerialType serial = 0;
		return serial++;
	}

	namespace DB
	{
		const static char* Host     = "localhost";
		const static char* User     = "root";
		const static char* Password = "0000";
		const static char* Schema   = "VSLike";
	}

	namespace Memory
	{
		template<class Type>
		static void SafeDelete( Type*& _data )
		{
			if ( _data != nullptr )
			{
				delete _data;
				_data = nullptr;
			}
		}

		template<typename Type>
		static void SafeDeleteArray( Type*& _data )
		{
			if ( _data != nullptr )
			{
				delete[] _data;
				_data = nullptr;
			}
		}
	}

	namespace String
	{
		static std::string RemoveAll( std::string& _data, const char _token )
		{
			_data.erase( std::remove( _data.begin(), _data.end(), _token ), _data.end() );

			return _data;
		}

		static std::string ReplaceAll( std::string& _data, const std::string& _from, const std::string& _to )
		{
			size_t pos = 0;
			while ( ( pos = _data.find( _from, pos ) ) != std::string::npos )
			{
				_data.replace( pos, _from.length(), _to );
			}

			return _data;
		}
	}

	namespace Text
	{
		static std::string ToUTF8( const std::string& _string )
		{
			int lengthUnicode = 0;
			int lengthUTF = 0;
			wchar_t* UnicodeBuffer = nullptr;
			char* multibyteBuffer = nullptr;

			if ( ( lengthUnicode = ::MultiByteToWideChar( CP_ACP, 0, _string.c_str(), ( int )::strlen( _string.c_str() ), NULL, 0) ) < 0 )
			{
				return std::string();
			}
			lengthUnicode = lengthUnicode + ( int )1;
			UnicodeBuffer = new wchar_t[lengthUnicode];
			::memset( UnicodeBuffer, 0x00, ( size_t )( sizeof( wchar_t ) * lengthUnicode ) );

			// Ansi -> Unicode
			lengthUnicode = ::MultiByteToWideChar( CP_ACP, 0, _string.c_str(), ( int )::strlen( _string.c_str() ), UnicodeBuffer, lengthUnicode );
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

		static std::string ToAnsi( const std::string& _string )
		{
			int lengthUnicode = 0;
			int lengthUTF = 0;
			wchar_t* UnicodeBuffer = nullptr;
			char* multibyteBuffer = nullptr;

			if ( ( lengthUnicode = ::MultiByteToWideChar( CP_UTF8, 0, _string.c_str(), ( int )::strlen( _string.c_str() ), NULL, 0) ) < 0 )
			{
				return std::string();
			}
			UnicodeBuffer = UnicodeBuffer + ( int )1;
			UnicodeBuffer = new wchar_t[lengthUnicode];
			::memset( UnicodeBuffer, 0x00, ( size_t )( sizeof( wchar_t ) * lengthUnicode ) );

			// UTF-8 -> Unicode
			lengthUnicode = ::MultiByteToWideChar( CP_UTF8, 0, _string.c_str(), ( int )::strlen( _string.c_str() ), UnicodeBuffer, lengthUnicode );
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
	}
}