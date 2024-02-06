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
	::memset( sentence, 0, 256 );
	::sprintf( sentence, R"Q(select * from userdata where id = '%s';)Q", _id );

	if ( !Query( sentence ) )
		 return UserData();

	UserData data;
	MYSQL_ROW row = ::mysql_fetch_row( result );
	data.nickname = row[0];
	data.id       = row[1];
	data.pw       = row[2];
	
	std::cout << "# Search( " << data.nickname << " " << data.id << " " << data.pw << " )" << std::endl;
	return data;
}

bool Database::Query( const char* _sentence )
{
	if ( conn == nullptr || ( ::mysql_query( conn, _sentence ) != NULL ) )
	{
		std::cout << ::mysql_error( conn ) << std::endl;
		return false;
	}

	if ( ( result = ::mysql_store_result( conn ) ) == nullptr )
	{
		std::cout << ::mysql_error( conn ) << std::endl;
		return false;
	}

	return true;
}

// 찾기
// select * from userdata where nickname = 'wns';

// 삽입
// insert into UserData( nickname, id, pw ) values( 'wns', 'wns', '0000' );
// insert into userdata values( 'taehong', 'th', 1111 );

// 수정
// update userdata set pw ='0000' where nickname = 'wns';

// 삭제
// delete from userdata where nickname = 'wns';