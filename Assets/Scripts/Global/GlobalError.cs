
public enum Result : ushort
{
    OK = 0,
    DB_ERR_DISCONNECTED,   // ������ �����������
    DB_ERR_INVALID_QUERY,  // ���� ������ ��ȿ��������
    DB_ERR_DUPLICATE_DATA, // UNIQUE�� ������ �����Ͱ� �̹� ������

    ERR_NOT_EXIST_DATA,    // �����Ͱ� ������������
    ERR_INVALID_DATA,      // �����Ͱ� ��ȿ��������
    ERR_UNABLE_PROCESS,    // ��û�� ������ �� ����
}