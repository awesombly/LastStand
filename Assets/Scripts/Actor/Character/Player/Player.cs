using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum PlayerType
{
    None = 0, Pilot, Hunter, Convict,
}

public class Player : Character
{
    #region UI
    private string nickname;
    public string Nickname 
    {
        get => nickname;
        set
        {
            nickname = value;
            nicknameUI?.SetText( nickname );
        }
    }
    public bool IsHost 
    { 
        get 
        {
            if ( GameManager.StageInfo is null )
            {
                return false;
            }
            return Serial == GameManager.StageInfo.Value.hostSerial;
        }
    }
    private int killScore;
    public int KillScore 
    {
        get => killScore;
        set
        {
            killScore = value;
            Board?.UpdateKillCount( killScore );
        }
    }
    private int deathScore;
    public int DeathScore
    {
        get => deathScore;
        set
        {
            deathScore = value;
            Board?.UpdateDeathCount( deathScore );
        }
    }

    [SerializeField]
    private TextMeshProUGUI nicknameUI;
    [SerializeField]
    private UnityEngine.UI.Slider healthBar;
    [SerializeField]
    private UnityEngine.UI.Slider healthLerpBar;
    [SerializeField]
    private PlayerChatMessage chatMessage;
    public PlayerBoard Board { get; set; }
    public float Health     => healthBar.value;
    public float HealthLerp => healthLerpBar.value;

    public event Action OnPlayerRespawn;
    public event Action<Player/* 공격한 플레이어 */> OnPlayerDead;
    public event Action<Player/* 처치 플레이어 */>   OnPlayerKill;
    #endregion

    [HideInInspector]
    public PlayerSO playerData;
    private PlayerType playerType;
    public PlayerType PlayerType
    {
        get => playerType;
        set
        {
            if ( playerType == value )
            {
                return;
            }

            playerType = value;
            playerData = GameManager.Inst.GetPlayerSO( playerType );
            OnChangePlayerType?.Invoke( playerType );
        }
    }
    // 0일때만 조작 가능
    private int unmoveableCount;
    public int UnmoveableCount
    {
        get => unmoveableCount;
        set
        {
            unmoveableCount = value;

            bool isMoveable = unmoveableCount <= 0;
            receiver.enabled = isMoveable;
            movement.IsMoveable = isMoveable;
            movement.moveInfo.moveVector = Vector2.zero;
            SceneBase.EnabledInputSystem( isMoveable, isMoveable );
        }
    }
    public Vector2 Direction { get; set; }
    private List<Weapon> Weapons { get; set; } = null;
    public Weapon PrevWeapon 
    {
        get
        {
            int index = Weapons.IndexOf( EquipWeapon ) - 1;
            if ( index <= 0 )
            {
                index = Weapons.Count - 1;
            }
            return Weapons[index];
        }
    }
    public Weapon NextWeapon
    {
        get
        {
            int index = Weapons.IndexOf( EquipWeapon ) + 1;
            if ( index >= Weapons.Count )
            {
                index = 1;
            }
            return Weapons[index];
        }
    }

    private InteractableActor nearestInteractable = null;
    private float interactableAngle = 0f;

    #region Components
    public PlayerUI PlayerUI { get; private set; }
    public PlayerDodgeAttack DodgeAttack { get; private set; }
    private PlayerInput playerInput;
    private PlayerAnimator playerAnimator;
    private ActionReceiver receiver;
    private PlayerMovement movement;
    #endregion

    public event Action<PlayerType> OnChangePlayerType;
    public Action<float/*angle*/> OnInteractionAction;

    #region Unity Callback
    protected override void Awake()
    {
        base.Awake();
        PlayerUI = GetComponent<PlayerUI>();
        DodgeAttack = GetComponentInChildren<PlayerDodgeAttack>();
        playerInput = GetComponent<PlayerInput>();
        playerAnimator = GetComponent<PlayerAnimator>();
        receiver = GetComponent<ActionReceiver>();
        movement = GetComponent<PlayerMovement>();
        Weapons = new List<Weapon>( GetComponentsInChildren<Weapon>( true ) );

        receiver.OnSwapWeaponEvent += SwapWeapon;
        receiver.OnPrevWeaponEvent += SwapPrevWeapon;
        receiver.OnNextWeaponEvent += SwapNextWeapon;
        receiver.OnInteractionEvent += Interaction;

        healthBar.value = Hp.Current / Hp.Max;
        healthLerpBar.value = healthBar.value;
        Hp.OnChangeCurrent += OnChangeHp;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected void FixedUpdate()
    {
        if ( !IsLocal )
        {
            return;
        }
        UpdateNearestInteractable();
    }
    #endregion

    public void UpdateUI()
    {
        healthLerpBar.value = Mathf.Max( healthLerpBar.value - 0.1f * Time.deltaTime, healthBar.value );
        healthLerpBar.value = Mathf.Lerp( healthLerpBar.value, healthBar.value, 2f * Time.deltaTime );
        Board?.UpdateHealthLerp( healthLerpBar.value );
    }

    public void ReceiveMessage( string _message )
    {
        chatMessage.gameObject.SetActive( true );
        chatMessage.UpdateMessage( _message );
    }

    public void SwapWeapon( int _index )
    {
        if ( _index <= 0 || _index >= Weapons.Count )
        {
            return;
        }

        EquipWeapon = Weapons[_index];

        if ( IsLocal )
        {
            INDEX_INFO protocol;
            protocol.serial = Serial;
            protocol.index = _index;
            Network.Inst.Send( PacketType.SYNC_SWAP_WEAPON_REQ, protocol );
        }
    }

    private void SwapPrevWeapon()
    {
        SwapWeapon( Weapons.IndexOf( PrevWeapon ) );
    }

    private void SwapNextWeapon()
    {
        SwapWeapon( Weapons.IndexOf( NextWeapon ));
    }

    public void ResetExcludeLayers()
    {
        movement.ResetExcludeLayers();
    }

    public void SetInvincibleTime( float _time )
    {
        StartCoroutine( UpdateInvincible( _time ) );
    }

    private IEnumerator UpdateInvincible( float _time )
    {
        int originLayer = gameObject.layer;

        playerAnimator.SetReviveAction( true );
        ++UnactionableCount;
        gameObject.layer = Global.Layer.Invincible;
        while ( _time > 0f )
        {
            _time -= Time.deltaTime;
            float alpha = Mathf.Max( .4f, .4f + ( 2f - _time ) * .2f );
            Spriter.color = new Color( 1f, 1f, 1f, alpha );
            yield return YieldCache.WaitForEndOfFrame;
        }
        Spriter.color = Color.white;
        gameObject.layer = originLayer;
    }

    public override void SetMovement( Vector3 _position, Vector3 _velocity )
    {
        transform.position = _position;
        movement.moveInfo.moveVector = _velocity;
    }

    public void AckDodgeAction( bool _useCollision, Vector2 _direction, float _duration )
    {
        movement.DodgeAction( _useCollision, _direction, _duration );
    }

    private void UpdateNearestInteractable()
    {
        InteractableActor target = null;
        float nearestDistance = float.MaxValue;

        RaycastHit2D[] hits = Physics2D.CircleCastAll( transform.position, 1.5f, Vector2.zero, 0f, ( int )Global.LayerFlag.Misc );
        foreach ( RaycastHit2D hit in hits )
        {
            InteractableActor interactable = hit.transform.GetComponent<InteractableActor>();
            if ( interactable is null || interactable.IsInteracted )
            {
                continue;
            }

            float distance = Vector2.Distance( transform.position, interactable.transform.position );
            if ( nearestDistance < distance )
            {
                continue;
            }

            // LookAngle 사용시 각도에 제한을 둠.(등에 있는 물체를 날리는 등 방지)
            float angle = Global.GetAngle( transform.position, interactable.transform.position );
            if ( interactable.useLookAngle )
            {
                if ( Mathf.Abs( Mathf.DeltaAngle( angle, GameManager.LookAngle ) ) > 90f )
                {
                    continue;
                }
                interactableAngle = GameManager.LookAngle;
            }
            else
            {
                interactableAngle = angle;
            }

            nearestDistance = distance;
            target = interactable;
        }

        if ( !ReferenceEquals( nearestInteractable, target ) )
        {
            if ( nearestInteractable is not null )
            {
                nearestInteractable.IsSelected = false;
            }

            if ( target is not null )
            {
                target.IsSelected = true;
            }
            nearestInteractable = target;
        }
    }

    private void Interaction()
    {
        if ( nearestInteractable is null || UnactionableCount > 0 )
        {
            return;
        }
        nearestInteractable.TryInteraction( this, interactableAngle );
    }

    protected override void OnDead( Actor _attacker, IHitable _hitable )
    {
        if ( IsDead )
        {
            Debug.LogWarning( $"Already dead player. nick:{nickname}, name:{name}, attacker:{_attacker.name}" );
            return;
        }

        IsDead = true;
        ++DeathScore;

        Player attacker = _attacker as Player;
        if ( attacker is not null )
        {
            ++attacker.KillScore;
            if ( attacker == GameManager.LocalPlayer )
                 attacker.OnPlayerKill?.Invoke( this );
        }

        Vector3 direction = _hitable is not null ? _hitable.GetUpDirection() : Vector3.zero;
        playerAnimator.OnDead( direction );
        GameManager.Inst.PlayerDead( this, _attacker, _hitable );

        OnPlayerDead?.Invoke( attacker );
    }

    public void OnRespawn()
    {
        OnPlayerRespawn?.Invoke();
    }

    protected override void OnChangeLocal( bool _isLocal )
    {
        base.OnChangeLocal( _isLocal );
        playerInput.enabled = _isLocal;
        receiver.enabled = _isLocal;
    }

    private void OnChangeHp( float _old, float _new, float _max )
    {
        healthBar.value = _new / _max;
        if ( _old < _new )
        {
            healthLerpBar.value = healthBar.value;
        }

        Board?.UpdateHealth( healthBar.value );
    }

    public override void Release()
    {
        EquipWeapon = null;
        GameManager.Inst.RemovePlayer( this );
        base.Release();
    }
}
