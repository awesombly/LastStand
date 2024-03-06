#pragma once
enum class Result : unsigned short
{
    OK = 0,
    DB_ERR_DISCONNECTED,   // DB 서버에 연결되지않음
    DB_ERR_INVALID_QUERY,  // 쿼리 구문이 유효하지않음
    DB_ERR_INVALID_DATA,   // DB에 전달할 데이터가 유효하지않음
    DB_ERR_NOT_EXIST_DATA, // DB안에 데이터가 존재하지않음
    DB_ERR_DUPLICATE_DATA, // UNIQUE 설정된 데이터가 존재함
};

class Error
{
public:
    static const char* String( Result _result )
    {
        switch ( _result )
        {
            default:                            return "Unknown Error";
            case Result::OK:                    return "OK";
            case Result::DB_ERR_DISCONNECTED:   return "Not connected to database";
            case Result::DB_ERR_INVALID_QUERY:  return "Invalid query syntax";
            case Result::DB_ERR_INVALID_DATA:   return "Invalid data to pass to database";
            case Result::DB_ERR_NOT_EXIST_DATA: return "Data does not exist in database";
                 //return Database::Inst().GetLastError();
        }
    }
};