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

bool Database::Query( const char* _sentence )
{
	Debug.Log( "# Query < ", sentence, " >" );
	if ( conn == nullptr || ( ::mysql_query( conn, sentence ) != NULL ) )
	{
		std::cout << ::mysql_error( conn ) << std::endl;
		return false;
	}

	::memset( sentence, 0, MaxSentenceSize );
	return true;
}


bool Database::CreateUserData( const std::string& _nickname, const std::string& _email, const std::string& _password )
{
	::sprintf( sentence, R"Q( Call CreateUser( '%s', '%s', '%s' ); )Q", _nickname.c_str(), _email.c_str(), _password.c_str() );
	return Query( sentence );
}

bool Database::DeleteUserData( int _uid )
{
	::sprintf( sentence, R"Q( Call DeleteUser( %d ); )Q", _uid );
	return Query( sentence );
}

LOGIN_DATA Database::GetLoginData( const std::string& _email )
{
	::sprintf( sentence, R"Q( SELECT * FROM LoginData WHERE Email = '%s'; )Q", _email.c_str() );
	if ( !Query( sentence ) || ( result = ::mysql_store_result( conn ) ) == nullptr )
	{
		std::cout << ::mysql_error( conn ) << std::endl;
		throw std::exception( "Invalid query statement" );
	}

	MYSQL_ROW row;
	if ( ( row = ::mysql_fetch_row( result ) ) == nullptr )
		throw std::exception( "The data was not found" );

	return LOGIN_DATA{ ::atoi( row[0] ), row[1], row[2], row[3] };
}

LOGIN_DATA Database::GetLoginData( int _uid )
{
	::sprintf( sentence, R"Q( SELECT * FROM LoginData WHERE uid = '%d'; )Q", _uid );
	if ( !Query( sentence ) || ( result = ::mysql_store_result( conn ) ) == nullptr )
	{
		std::cout << ::mysql_error( conn ) << std::endl;
		throw std::exception( "Invalid query statement" );
	}

	MYSQL_ROW row;
	if ( ( row = ::mysql_fetch_row( result ) ) == nullptr )
		throw std::exception( "The data was not found" );

	return LOGIN_DATA{ ::atoi( row[0] ), row[1], row[2], row[3] };
}

USER_DATA Database::GetUserData( int _uid )
{
	::sprintf( sentence, R"Q( SELECT * FROM UserData WHERE uid = '%d'; )Q", _uid );
	if ( !Query( sentence ) || ( result = ::mysql_store_result( conn ) ) == nullptr )
	{
		std::cout << ::mysql_error( conn ) << std::endl;
		throw std::exception( "Invalid query statement" );
	}

	MYSQL_ROW row;
	if ( ( row = ::mysql_fetch_row( result ) ) == nullptr )
		throw std::exception( "The data was not found" );

	return USER_DATA{ ::atoi( row[2] ), ( float )::atof(row[3]), ::atoi(row[4]), ::atoi(row[5]), ::atoi(row[6]), ::atoi(row[7]), ::atoi(row[8])};
}
bool Database::UpdateUserData( int _uid, const USER_DATA& _data )
{
	::sprintf( sentence, R"Q( Call UpdateUserData( %d, %d, %f, %d, %d, %d, %d, %d ); )Q", 
			                  _uid, _data.level, _data.exp, _data.playCount, _data.kill, _data.death, _data.bestKill, _data.bestDeath );
	return Query( sentence );
}

//LOGIN_DATA Database::Search( const std::string& _type, const std::string& _data )
//{
//	::sprintf( sentence, R"Q( SELECT * FROM userdata where %s = '%s';)Q", _type.c_str(), _data.c_str() );
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
//

// 찾기
// select * from userdata where nickname = 'wns';

// 삽입
// insert into UserData( nickname, email, password ) values( 'wns', 'wns', '0000' );
// insert into userdata values( 'taehong', 'th', 1111 );

// 수정
// update userdata set password ='0000' where nickname = 'wns';

// 삭제
// delete from userdata where nickname = 'wns';