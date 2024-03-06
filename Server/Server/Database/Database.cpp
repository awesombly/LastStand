#include "Database.h"

Database::~Database()
{
	::mysql_close( conn );
	Global::Memory::SafeDelete( conn );

	::mysql_free_result( result );
	Global::Memory::SafeDelete( result );
}

bool Database::Initialize()
{
	if ( ::mysql_init( &driver ) == nullptr )
	 	 return false;

	conn = ::mysql_real_connect( &driver, Global::DB::Host,     Global::DB::User,
									      Global::DB::Password, Global::DB::Schema, 3306, ( const char* )NULL, 0 );

	if ( conn == nullptr || ( ::mysql_select_db( conn, Global::DB::Schema ) != NULL ) )
	{
		std::cout << ::mysql_error( conn ) << std::endl;
		Global::Memory::SafeDelete( conn );
		return false;
	}

	std::cout << "Connected MySQL Server( " << conn->host << ":" << conn->port << " " << 
		                                       conn->user << " " << "****" << " " << conn->db << " )" << std::endl;

	return true;
}

void Database::Query( const char* _sentence, ... )
{
	if ( conn == nullptr )
		 throw Result::DB_ERR_DISCONNECTED;

	char sentence[1024] { 0, };
	size_t pos = 0;

	va_list vl;
	va_start( vl, _sentence );
	for ( int i = 0; _sentence[i] != '\0'; i++ )
	{
		if ( _sentence[i] == '%' )
		{
			switch ( _sentence[++i] )
			{
				default: 
				{
					sentence[pos]     = '%';
					sentence[pos + 1] = _sentence[i];
					pos += 2;
				} break;

				case 'd': 
				{
					std::string str = std::to_string( va_arg( vl, int ) );
					size_t size     = str.size();

					std::copy( &str[0], &str[size], &sentence[pos] );
					pos += size;
				} break;

				case 'f': 
				{
					std::string str = std::to_string( va_arg( vl, double ) );
					size_t size = str.size();

					std::copy( &str[0], &str[size], &sentence[pos] );
					pos += size;
				}
				break;

				case 'c': 
				{
					char* str = va_arg( vl, char* );
					size_t size = ::strlen( str );

					std::copy( &str[0], &str[size], &sentence[pos] );
					pos += size;
				}
				break;

				case 's':
				{
					std::string str = va_arg( vl, std::string );
					size_t size = str.size();
					
					std::copy( &str[0], &str[size], &sentence[pos] );
					pos += size;
				}
				break;
			}
		}
		else
		{
			sentence[pos] = _sentence[i];
			pos += 1;
		}
	}
	va_end( vl );

	Debug.Log( "# Query < ", sentence, " >" );
	if ( ::mysql_query( conn, sentence ) != NULL )
	{
		std::cout << ::mysql_error( conn ) << std::endl;
		throw Result::DB_ERR_INVALID_QUERY;
	}
}

bool Database::ExistLoginData( const LOGIN_DATA& _data )
{
	Query( R"Q( SELECT * FROM LoginData WHERE Email = '%s'; )Q", _data.email );
	if ( ( result = ::mysql_store_result( conn ) ) == nullptr )
	{
		std::cout << ::mysql_error( conn ) << std::endl;
		return false;
	}

	return ::mysql_fetch_row( result ) != nullptr;
}

void Database::CreateUserData( const std::string& _nickname, const std::string& _email, const std::string& _password )
{
	Query( R"Q( Call CreateUser( '%s', '%s', '%s' ); )Q", _nickname, _email, _password );
}

void Database::DeleteUserData( int _uid )
{
	Query( R"Q( Call DeleteUser( %d ); )Q", _uid );
}

void Database::UpdateUserData( int _uid, const USER_DATA& _data )
{
	Query( R"Q( Call UpdateUserData( %d, %d, %f, %d, %d, %d, %d, %d ); )Q",
							  _uid, _data.level, _data.exp, _data.playCount, _data.kill, _data.death, _data.bestKill, _data.bestDeath );
}

LOGIN_DATA Database::GetLoginData( const std::string& _email )
{
	Query( R"Q( SELECT * FROM LoginData WHERE Email = '%s'; )Q", _email );
	if ( ( result = ::mysql_store_result( conn ) ) == nullptr )
	{
		std::cout << ::mysql_error( conn ) << std::endl;
		throw Result::DB_ERR_NOT_EXIST_DATA;
	}

	MYSQL_ROW row;
	if ( ( row = ::mysql_fetch_row( result ) ) == nullptr )
		throw Result::DB_ERR_NOT_EXIST_DATA;

	return LOGIN_DATA{ ::atoi( row[0] ), row[1], row[2], row[3] };
}

LOGIN_DATA Database::GetLoginData( int _uid )
{
	Query( R"Q( SELECT * FROM LoginData WHERE uid = '%d'; )Q", _uid );
	if ( ( result = ::mysql_store_result( conn ) ) == nullptr )
	{
		std::cout << ::mysql_error( conn ) << std::endl;
		throw Result::DB_ERR_INVALID_QUERY;
	}

	MYSQL_ROW row;
	if ( ( row = ::mysql_fetch_row( result ) ) == nullptr )
		throw Result::DB_ERR_INVALID_QUERY;

	return LOGIN_DATA{ ::atoi( row[0] ), row[1], row[2], row[3] };
}

USER_DATA Database::GetUserData( int _uid )
{
	Query( R"Q( SELECT * FROM UserData WHERE uid = '%d'; )Q", _uid );

	if ( ( result = ::mysql_store_result( conn ) ) == nullptr )
	{
		std::cout << ::mysql_error( conn ) << std::endl;
		throw Result::DB_ERR_INVALID_QUERY;
	}

	MYSQL_ROW row;
	if ( ( row = ::mysql_fetch_row( result ) ) == nullptr )
		throw std::exception( "The data was not found" );

	return USER_DATA{ ::atoi( row[2] ), ( float )::atof(row[3]), ::atoi(row[4]), ::atoi(row[5]), ::atoi(row[6]), ::atoi(row[7]), ::atoi(row[8])};
}


//LOGIN_DATA Database::Search( const std::string& _type, const std::string& _data )
//{
//	::sprintf( sentence, R"Q( SELECT * FROM userdata WHERE %s = '%s';)Q", _type.c_str(), _data.c_str() );
//	if ( !Query( sentence ) || ( result = ::mysql_store_result( conn ) ) == nullptr )
//	{
//		std::cout << ::mysql_error( conn ) << std::endl;
//		throw std::exception( "Invalid query statement" );
//	}
//
//	MYSQL_ROW row;
//	if ( ( row = ::mysql_fetch_row( result ) ) == nullptr )
//		throw std::exception( "The data was not found" );
//
//	return LOGIN_DATA{ ::atoi( row[0] ), row[1], row[2], row[3]};
//}
//
//bool Database::Insert( const LOGIN_DATA& _data )
//{
//	::sprintf( sentence, R"Q( insert into userdata values( '%s', '%s', '%s' ); )Q", _data.nickname.c_str(), _data.email.c_str(), _data.password.c_str() );
//	
//	return Query( sentence );
//}
//
//bool Database::Update( const LOGIN_DATA& _data )
//{
//	::sprintf( sentence, R"Q(update userdata set nickname = '%s', password = '%s' where email = '%s';)Q", _data.nickname.c_str(), _data.password.c_str(), _data.email.c_str() );
//	return Query( sentence );
//}
//
//bool Database::Delete( const LOGIN_DATA& _data )
//{
//	::sprintf( sentence, R"Q(delete from userdata where email = '%s';)Q", _data.email.c_str() );
//	return Query( sentence );
//}


// ã��
// select * from userdata where nickname = 'wns';

// ����
// insert into UserData( nickname, email, password ) values( 'wns', 'wns', '0000' );
// insert into userdata values( 'taehong', 'th', 1111 );

// ����
// update userdata set password ='0000' where nickname = 'wns';

// ����
// delete from userdata where nickname = 'wns';