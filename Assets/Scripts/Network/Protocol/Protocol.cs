using System;
using System.Collections.Generic;
using UnityEngine;

// null이 포함된 데이터를 JSON으로 만들면 서버가 뻗습니다.
// string 같은 클래스는 꼭 초기화 해주세요.

// 서버에서 패킷타입을 문자열로 출력하기위해
// 타입은 0부터 순서대로 지정되도록 합니다.
public enum PacketType : ushort
{
    NONE = 0,
    PACKET_HEARTBEAT,              // 주기적인 통신을 위한 패킷
    PACKET_CHAT_MSG,               // 채팅 메세지

    // Login
    CONFIRM_LOGIN_REQ,             // 로그인 요청
    CONFIRM_LOGIN_ACK,             // 로그인 응답
    CONFIRM_ACCOUNT_REQ,           // 계정 생성 요청
    CONFIRM_ACCOUNT_ACK,           // 계정 생성 응답
    DUPLICATE_EMAIL_REQ,           // 이메일 중복확인 요청
    DUPLICATE_EMAIL_ACK,           // 이메일 중복확인 응답

    // Stage
    STAGE_INFO_REQ,                // 방 정보 요청
    STAGE_INFO_ACK,                // 방 정보 응답
    CREATE_STAGE_REQ,              // 방 생성 요청
    CREATE_STAGE_ACK,              // 방 생성 응답
    UPDATE_STAGE_INFO,             // 방 정보가 갱신됨
    INSERT_STAGE_INFO,             // 방 정보가 추가됨
    DELETE_STAGE_INFO,             // 방 정보가 삭제됨
    ENTRY_STAGE_REQ,               // 방 입장 요청
    ENTRY_STAGE_ACK,               // 방 입장 응답
    EXIT_STAGE_REQ,                // 방 퇴장 요청
    EXIT_STAGE_ACK,                // 방 퇴장 응답

    // Actor
    SPAWN_ACTOR_REQ,               // Actor 스폰 요청
    SPAWN_ACTOR_ACK,               // Actor 스폰 응답
    SPAWN_PLAYER_ACK,              // Player 스폰 응답
    SPAWN_BULLET_REQ,              // Bullet 스폰 요청
    SPAWN_BULLET_ACK,              // Bullet 스폰 응답
    REMOVE_ACTOR_REQ,              // Actor 제거 요청
    REMOVE_ACTOR_ACK,              // Actor 제거 응답

    SYNC_MOVEMENT_REQ,             // Actor 이동 동기화 요청
    SYNC_MOVEMENT_ACK,             // Actor 이동 동기화 응답
    SYNC_RELOAD_REQ,               // 재장전 동기화 요청
    SYNC_RELOAD_ACK,               // 재장전 동기화 응답
    SYNC_LOOK_ANGLE_REQ,           // Player 시선 동기화 요청
    SYNC_LOOK_ANGLE_ACK,           // Player 시선 동기화 응답
    SYNC_DODGE_ACTION_REQ,         // Player 회피 동기화 요청
    SYNC_DODGE_ACTION_ACK,         // Player 회피 동기화 응답
    SYNC_SWAP_WEAPON_REQ,          // Player 무기 교체 동기화 요청
    SYNC_SWAP_WEAPON_ACK,          // Player 무기 교체 동기화 응답
    HIT_ACTOR_REQ,                 // 피격 동기화 요청
    HIT_ACTOR_ACK,                 // 피격 동기화 응답
    INGAME_LOAD_DATA_REQ,          // InGame 입장시 데이터 요청
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
public struct INDEX_INFO : IProtocol { public uint serial; public int index; }

public struct Personnel { public int current, maximum; }
public struct STAGE_INFO : IProtocol
{
    public ushort serial;
    public string title;
    public int targetKill;
    public Personnel personnel;
}

public struct LOGIN_INFO : IProtocol
{
    public string nickname;
    public string email;
    public string password;
}

public struct ACTOR_INFO : IProtocol
{
    public int prefab;
    public bool isLocal;
    public uint serial;
    public VECTOR2 pos;
    public VECTOR2 vel;
    public float hp;
}

public struct PLAYER_INFO : IProtocol
{
    public ACTOR_INFO actorInfo;
    public string nickname;
    public float angle;
    public int weapon;
}

public struct BULLET_INFO : IProtocol
{
    public int prefab;
    public bool isLocal;
    public uint serial;
    public uint owner;
    public VECTOR2 pos;
    public float angle;
    public float look;
    public float damage;
}

public struct HIT_INFO : IProtocol
{
    public bool needRelease;
    public uint bullet;
    public uint attacker;
    public uint defender;
    public float hp;
}

public struct MOVEMENT_INFO : IProtocol
{
    public uint serial;
    public VECTOR2 pos;
    public VECTOR2 vel;
}

public struct LOOK_INFO : IProtocol
{
    public uint serial;
    public float angle;
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
    public string nickname;
    public string message;
}