#pragma once
enum class Result : unsigned short
{
    OK = 0,
    DB_ERR_DISCONNECTED,   // DB ������ �����������
    DB_ERR_INVALID_QUERY,  // ���� ������ ��ȿ��������
    DB_ERR_INVALID_DATA,   // DB�� ������ �����Ͱ� ��ȿ��������
    DB_ERR_NOT_EXIST_DATA, // DB�ȿ� �����Ͱ� ������������
    DB_ERR_DUPLICATE_DATA, // UNIQUE ������ �����Ͱ� ������
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