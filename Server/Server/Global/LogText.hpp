#pragma once
#include "Singleton.hpp"

#define Debug ( LogText::Inst() << __FUNCTION__ << "( " << std::to_string( __LINE__ ) << " )" )
class LogText : public Singleton<LogText>
{
private:
	enum LogType : short { _Log, _Warning, _Error, };

	static const u_short MaxLogSize = 2048;
	std::ofstream os;

	size_t pos;
	char   data[MaxLogSize];

	size_t infoPos;
	char   info[MaxLogSize];

public:
	LogText()
	{
		Clear();

		char date[21] = { 0, };
		const std::time_t now = std::chrono::system_clock::to_time_t( std::chrono::system_clock::now() );
		std::strftime( &date[0], 21, "%Y-%m-%d_%H-%M-%S", std::localtime( &now ) );

		std::string path;
		path.append( "../Log/" ).append( date ).append( ".txt" );
		os.open( path, std::ios::out | std::ios::trunc );
		if ( !os.is_open() )
			 std::cout << "File open failed" << std::endl;
	}
	~LogText()
	{
		os.close();
	}

	void Log()     { }
	void LogWarning() { }
	void LogError()   { }

	template<typename T, typename... Args>
	void Log( T type, Args... _args )
	{
		Copy( type );
		Log( _args... );

		Write( LogType::_Log );
	}

	template<typename T, typename... Args>
	void LogWarning( T type, Args... _args )
	{
		Copy( type );
		LogWarning( _args... );

		Write( LogType::_Warning );
	}

	template<typename T, typename... Args>
	void LogError( T type, Args... _args )
	{
		Copy( type );
		LogError( _args... );

		Write( LogType::_Error );
	}

	void Clear()
	{
		::memset( data, 0, MaxLogSize );
		::memset( info, 0, MaxLogSize );
		pos = infoPos = 0;
	}

	void Write( LogType _type )
	{
		if ( pos == 0 || !os.is_open() )
		{
			Clear();
			return;
		}

		switch ( _type )
		{
			case LogType::_Log:
			{
				os << "# Log < " << info << " > #" << std::endl;
			}
			break;

			case LogType::_Warning:
			{
				os << "# Warning < " << info << " > #" << std::endl;
			} break;

			case LogType::_Error:
			{
				os << "### Error < " << info << " > ###" << std::endl;
			} break;
		}

		os        << data << std::endl << std::endl;
		std::cout << data << std::endl;
		Clear();
	}

	std::string ToString( float _value, int _maxDecimalPoint = 3 )
	{
		std::ostringstream out;
		out << std::setprecision( _maxDecimalPoint ) << _value;

		return out.str();
	}
	LogText& operator << ( const std::string& _arg )
	{
		std::copy( std::begin( _arg ), std::end( _arg ), &info[infoPos] );
		infoPos += _arg.size();

		return *this;
	}
	LogText& operator << ( const char* _arg )
	{
		size_t size = ::strlen( _arg );
		std::copy( &_arg[0], &_arg[size], &info[infoPos] );
		infoPos += size;

		return *this;
	}
	void Copy( int                  _arg )
	{
		std::string str = std::to_string( _arg );
		size_t size = str.size();
		std::copy( &str[0], &str[size], &data[pos] );
		pos += size;

	}
	void Copy( unsigned int         _arg )
	{
		std::string str = std::to_string( _arg );
		size_t size = str.size();
		std::copy( &str[0], &str[size], &data[pos] );
		pos += size;

	}
	void Copy( short                _arg )
	{
		std::string str = std::to_string( _arg );
		size_t size = str.size();
		std::copy( &str[0], &str[size], &data[pos] );
		pos += size;

	}
	void Copy( unsigned short       _arg )
	{
		std::string str = std::to_string( _arg );
		size_t size = str.size();
		std::copy( &str[0], &str[size], &data[pos] );
		pos += size;

	}
	void Copy( long                 _arg )
	{
		std::string str = std::to_string( _arg );
		size_t size = str.size();
		std::copy( &str[0], &str[size], &data[pos] );
		pos += size;

	}
	void Copy( unsigned long        _arg )
	{
		std::string str = std::to_string( _arg );
		size_t size = str.size();
		std::copy( &str[0], &str[size], &data[pos] );
		pos += size;

	}
	void Copy( long long            _arg )
	{
		std::string str = std::to_string( _arg );
		size_t size = str.size();
		std::copy( &str[0], &str[size], &data[pos] );
		pos += size;

	}
	void Copy( unsigned long long   _arg )
	{
		std::string str = std::to_string( _arg );
		size_t size = str.size();
		std::copy( &str[0], &str[size], &data[pos] );
		pos += size;

	}
	void Copy( float                _arg )
	{
		std::string str = std::to_string( _arg );
		size_t size = str.size();
		std::copy( &str[0], &str[size], &data[pos] );
		pos += size;

	}
	void Copy( double               _arg )
	{
		std::string str = std::to_string( _arg );
		size_t size = str.size();
		std::copy( &str[0], &str[size], &data[pos] );
		pos += size;

	}
	void Copy( const char*          _arg )
	{
		size_t size = ::strlen( _arg );
		std::copy( &_arg[0], &_arg[size], &data[pos] );
		pos += size;
	}
	void Copy( const unsigned char* _arg )
	{
		size_t size = ::strlen( ( char* )_arg );
		std::copy( &_arg[0], &_arg[size], &data[pos] );
		pos += size;
	}
	void Copy( const std::string&   _arg )
	{
		std::copy( std::begin( _arg ), std::end( _arg ), &data[pos] );
		pos += _arg.size();
	}
	void Copy( const Vector2&       _arg )
	{
		std::string str;
		str.reserve( 128 );
		str.append( "( " );
		str.append( ToString( _arg.x ) ).append( ", " );
		str.append( ToString( _arg.y ) ).append( " )" );

		size_t size = str.size();
		std::copy( &str[0], &str[size], &data[pos] );
		pos += size;
	}
	void Copy( const Vector3&       _arg )
	{
		std::string str;
		str.reserve( 128 );
		str.append( "( " );
		str.append( ToString( _arg.x ) ).append( ", " );
		str.append( ToString( _arg.y ) ).append( ", " );
		str.append( ToString( _arg.z ) ).append( " )" );

		size_t size = str.size();
		std::copy( &str[0], &str[size], &data[pos] );
		pos += size;
	}
	void Copy( const Vector4&       _arg )
	{
		std::string str;
		str.reserve( 128 );
		str.append( "( " );
		str.append( ToString( _arg.x ) ).append( ", " );
		str.append( ToString( _arg.y ) ).append( ", " );
		str.append( ToString( _arg.z ) ).append( ", " );
		str.append( ToString( _arg.w ) ).append( " )" );

		size_t size = str.size();
		std::copy( &str[0], &str[size], &data[pos] );
		pos += size;
	}
};

