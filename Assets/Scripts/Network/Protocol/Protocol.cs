using System;
using System.Collections.Generic;
using UnityEngine;

// null�� ���Ե� �����͸� JSON���� ����� ������ �����ϴ�.
// string ���� Ŭ������ �� �ʱ�ȭ ���ּ���.

// �������� ��ŶŸ���� ���ڿ��� ����ϱ�����
// Ÿ���� 0���� ������� �����ǵ��� �մϴ�.
public enum PacketType : ushort
{
    NONE = 0,
    PACKET_HEARTBEAT,              // �ֱ����� ����� ���� ��Ŷ
    PACKET_CHAT_MSG,               // ä�� �޼���

    // Login
    CONFIRM_LOGIN_REQ,             // �α��� ��û
    CONFIRM_LOGIN_ACK,             // �α��� ����
    CONFIRM_ACCOUNT_REQ,           // ���� ���� ��û
    CONFIRM_ACCOUNT_ACK,           // ���� ���� ����
    DUPLICATE_EMAIL_REQ,           // �̸��� �ߺ�Ȯ�� ��û
    DUPLICATE_EMAIL_ACK,           // �̸��� �ߺ�Ȯ�� ����

    // Stage
    STAGE_INFO_REQ,                // �� ���� ��û
    STAGE_INFO_ACK,                // �� ���� ����
    CREATE_STAGE_REQ,              // �� ���� ��û
    CREATE_STAGE_ACK,              // �� ���� ����
    UPDATE_STAGE_INFO,             // �� ������ ���ŵ�
    INSERT_STAGE_INFO,             // �� ������ �߰���
    DELETE_STAGE_INFO,             // �� ������ ������
    ENTRY_STAGE_REQ,               // �� ���� ��û
    ENTRY_STAGE_ACK,               // �� ���� ����
    EXIT_STAGE_REQ,                // �� ���� ��û
    EXIT_STAGE_ACK,                // �� ���� ����

    // Actor
    SPAWN_ACTOR_REQ,               // Actor ���� ��û
    SPAWN_ACTOR_ACK,               // Actor ���� ����
    SPAWN_PLAYER_REQ,              // Player ���� ��û
    SPAWN_PLAYER_ACK,              // Player ���� ����
    SPAWN_BULLET_REQ,              // Bullet ���� ��û
    SPAWN_BULLET_ACK,              // Bullet ���� ����
    REMOVE_ACTORS_REQ,             // Actor�� ���� ��û
    REMOVE_ACTORS_ACK,             // Actor�� ���� ����
    INIT_SCENE_ACTORS_REQ,         // ���� ��ġ�� Actor�� �ʱ�ȭ ��û
    INIT_SCENE_ACTORS_ACK,         // ���� ��ġ�� Actor�� �ʱ�ȭ ����

    SYNC_MOVEMENT_REQ,             // Actor �̵� ����ȭ ��û
	SYNC_MOVEMENT_ACK,             // Actor �̵� ����ȭ ����
	SYNC_RELOAD_REQ,               // ������ ����ȭ ��û
	SYNC_RELOAD_ACK,               // ������ ����ȭ ����
	SYNC_LOOK_ANGLE_REQ,           // Player �ü� ����ȭ ��û
	SYNC_LOOK_ANGLE_ACK,           // Player �ü� ����ȭ ����
	SYNC_DODGE_ACTION_REQ,         // Player ȸ�� ����ȭ ��û
	SYNC_DODGE_ACTION_ACK,         // Player ȸ�� ����ȭ ����
	SYNC_SWAP_WEAPON_REQ,          // Player ���� ��ü ����ȭ ��û
	SYNC_SWAP_WEAPON_ACK,          // Player ���� ��ü ����ȭ ����
	HIT_ACTORS_REQ,                // �ǰݵ� Actor�� ����ȭ ��û
	HIT_ACTORS_ACK,                // �ǰݵ� Actor�� ����ȭ ����

	INGAME_LOAD_DATA_REQ,          // InGame ����� ������ ��û
    GAME_OVER_ACK,                 // ���� ���� ����
};

public struct VECTOR3
{
    public float x, y, z;

    public VECTOR3( Vector3 _vector3 )
    {
        x = System.MathF.Round( _vector3.x, Global.RoundDigit );
        y = System.MathF.Round( _vector3.y, Global.RoundDigit );
        z = System.MathF.Round( _vector3.z, Global.RoundDigit );
    }

    public Vector3 To()
    {
        return new Vector3( x, y, z );
    }
}

public struct VECTOR2
{
    public float x, y;

    public VECTOR2( Vector2 _vector2 )
    {
        x = System.MathF.Round( _vector2.x, Global.RoundDigit );
        y = System.MathF.Round( _vector2.y, Global.RoundDigit );
    }

    public Vector2 To()
    {
        return new Vector2( x, y );
    }
}

public struct QUATERNION
{
    public float x, y, z, w;

    public QUATERNION( Quaternion _quaternion )
    {
        x = System.MathF.Round( _quaternion.x, Global.RoundDigit );
        y = System.MathF.Round( _quaternion.y, Global.RoundDigit );
        z = System.MathF.Round( _quaternion.z, Global.RoundDigit );
        w = System.MathF.Round( _quaternion.w, Global.RoundDigit );
    }

    public Quaternion To()
    {
        return new Quaternion( x, y, z, w );
    }
}

public interface IProtocol { }
// Both 
public struct EMPTY : IProtocol { }
public struct MESSAGE : IProtocol { public string message; }
public struct CONFIRM : IProtocol { public bool isCompleted; }
public struct SERIAL_INFO : IProtocol { public uint serial; }
public struct SERIALS_INFO : IProtocol { public List<uint> serials; }
public struct INDEX_INFO : IProtocol { public uint serial; public int index; }
public struct LOOK_INFO : IProtocol { public uint serial; public float angle; }

public struct Personnel { public int current, maximum; }
public struct STAGE_INFO : IProtocol
{
    public ushort serial;
    public string title;
    public int targetKill;
    public Personnel personnel;
}

#region Database
public struct LOGIN_INFO : IProtocol
{
    public int uid;
    public string nickname;
    public string email;
    public string password;
}

public struct USER_INFO : IProtocol
{
    public int kill, death;
    public int bestKill, bestDeath;
}
#endregion

public struct ACTOR_INFO : IProtocol
{
    public int prefab;
    public bool isLocal;
    public uint serial;
    public VECTOR2 pos;
    public VECTOR2 vel;
    public float hp;
}

public struct ACTORS_INFO : IProtocol
{
    public List<ACTOR_INFO> actors;
}

public struct PLAYER_INFO : IProtocol
{
    public ACTOR_INFO actorInfo;
    public string nickname;
    public bool isDead;
    public float angle;
    public int weapon;
    public int kill;
    public int death;
}

public struct BULLET_INFO
{
    public uint serial;
    public float angle;
    public float rate;
}

public struct BULLET_SHOT_INFO : IProtocol
{
    public int prefab;
    public bool isLocal;
    public uint owner;
    public VECTOR2 pos;
    public float look;
    public float damage;
    public List<BULLET_INFO> bullets;
}

public struct HIT_INFO
{
    public bool needRelease;
    public uint bullet;
    public uint attacker;
    public uint defender;
    public VECTOR2 pos;
    public float hp;
}

public struct HITS_INFO : IProtocol
{
    public List<HIT_INFO> hits;
}

public struct MOVEMENT_INFO : IProtocol
{
    public uint serial;
    public VECTOR2 pos;
    public VECTOR2 vel;
}

public struct DODGE_INFO : IProtocol
{
    public uint serial;
    public bool useCollision;
    public VECTOR2 pos;
    public VECTOR2 dir;
    public float dur;
}

public struct CHAT_MESSAGE : IProtocol
{
    public uint   serial;
    public string nickname;
    public string message;
}