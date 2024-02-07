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
	MYSQL driver;
	if ( ::mysql_init( &driver ) == nullptr )
	 	 return false;

	conn = ::mysql_real_connect( &driver, Global::DB::Host,     Global::DB::User,
									      Global::DB::Password, Global::DB::Schema, 3306, ( const char* )NULL, 0 );

	if ( conn == nullptr || ( ::mysql_select_db(conn, Global::DB::Schema) != NULL ) )
	{
		std::cout << ::mysql_error( conn ) << std::endl;
		Global::Memory::SafeDelete( conn );
		return false;
	}
	
	std::cout << "Connected MySQL Server( " << conn->host << ":" << conn->port << " " << 
		                                       conn->user << " " << "****" << " " << conn->db << " )" << std::endl;

	return true;
}

UserData Database::Search( const char* _id )
{
	::sprintf( sentence, R"Q(select * from userdata where email = '%s';)Q", _id );

	if ( !Query( sentence ) || ( result = ::mysql_store_result( conn ) ) == nullptr )
	{
		std::cout << ::mysql_error( conn ) << std::endl;
		return UserData();
	}
	MYSQL_ROW row = ::mysql_fetch_row( result );
	
	std::cout << "# Search( " << row[0] << " " << row[1] << " " << row[2] << " )" << std::endl;
	return UserData{ row[0], row[1], row[2] };
}

bool Database::Insert( const UserData& _data )
{
	::sprintf( sentence, R"Q(insert into userdata values( '%s', '%s', '%s' );)Q", _data.nickname, _data.email, _data.password );
	
	return Query( sentence );
}

bool Database::Update( const UserData& _data )
{
	::sprintf( sentence, R"Q(update userdata set nickname = '%s', password = '%s' where email = '%s';)Q", _data.nickname, _data.password, _data.email );
	return Query( sentence );
}

bool Database::Delete( const UserData& _data )
{
	::sprintf( sentence, R"Q(delete from userdata where email = '%s';)Q", _data.email );
	return Query( sentence );
}

bool Database::Query( const char* _sentence )
{
	std::cout << "# Query : " << sentence << std::endl;
	if ( conn == nullptr || ( ::mysql_query( conn, sentence ) != NULL ) )
	{
		std::cout << ::mysql_error( conn ) << std::endl;
		return false;
	}

	::memset( sentence, 0, MaxSentenceSize );
	return true;
}

// 찾기
// select * from userdata where nickname = 'wns';

// 삽입
// insert into UserData( nickname, email, password ) values( 'wns', 'wns', '0000' );
// insert into userdata values( 'taehong', 'th', 1111 );

// 수정
// update userdata set password ='0000' where nickname = 'wns';

// 삭제
// delete from userdata where nickname = 'wns';