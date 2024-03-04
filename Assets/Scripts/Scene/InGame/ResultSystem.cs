using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using static PacketType;
public class ResultSystem : MonoBehaviour
{
    [Header( "< Result >" )]
    public GameObject canvas;
    public List<ResultBoard> boards;
    public CustomHorizontalLayoutGroup layoutGroup;

    public TextMeshProUGUI level;
    public Slider          expSlider;
    public GameObject      exit;

    private void Awake()
    {
        GameManager.OnGameOver += OnGameOver;

        ProtocolSystem.Inst.Regist( UPDATE_RESULT_INFO_ACK, AckUpdateResultInfo );
    }

    private void OnDestroy()
    {
        GameManager.OnGameOver -= OnGameOver;
    }

    private void AckUpdateResultInfo( Packet _packet )
    {
        var prevInfo = GameManager.UserInfo.Value;
        GameManager.UserInfo = Global.FromJson<USER_INFO>( _packet );
        var curInfo = GameManager.UserInfo.Value;

        StartCoroutine( SmoothLevelUp( prevInfo, curInfo ) );
    }

    private IEnumerator SmoothLevelUp( USER_INFO _prev, USER_INFO _cur )
    {
        yield return YieldCache.WaitForSeconds( 1.25f );

        float time = 0f;
        float prevExp = ( _prev.exp / Global.GetTotalEXP( _prev.level ) );
        float curExp  = ( _cur.exp  / Global.GetTotalEXP( _cur.level ) );
        bool isLevelUp = false;

        while ( true )
        {
            if ( _prev.level < _cur.level )
            {
                expSlider.value = WNS.Math.Lerp( prevExp, 1f, time );
                time += ( 1f + ( _cur.level - _prev.level ) ) * Time.deltaTime;

                if ( time >= 1f )
                {
                    isLevelUp = true;

                    time = 0f;
                    _prev.level += 1;
                    _prev.exp = 0f;
                    prevExp = ( _prev.exp / Global.GetTotalEXP( _prev.level ) );
                    expSlider.value = 0f;
                    level.text = $"{_prev.level}";
                }
            }
            else
            {
                expSlider.value = isLevelUp ? WNS.Math.Lerp( 0f, curExp, time ) :
                                              WNS.Math.Lerp( prevExp, curExp, time );

                time += Time.deltaTime;
                if ( time >= 1f )
                {
                    expSlider.value = curExp;
                    break;
                }
            }

            yield return null;
        }

        exit.SetActive( true );
    }

    private void OnGameOver( Player _winner )
    {
        canvas.SetActive( true );
        //bool isWinner = ReferenceEquals( GameManager.LocalPlayer, _winner );
        //resultText.text = isWinner ? "- �¸� -" : "- �й� -";

        var players = GameManager.Players;
        for ( int i = 0; i < boards.Count; ++i )
        {
            if ( players.Count <= i )
            {
                boards[i].gameObject.SetActive( false );
                continue;
            }

            boards[i].gameObject.SetActive( true );
            boards[i].Initialize( players[i], ReferenceEquals( players[i], _winner ) );
        }
        layoutGroup.SetLayoutHorizontal();

        if ( GameManager.UserInfo != null )
        {
            var userInfo = GameManager.UserInfo.Value;
            level.text = $"{userInfo.level}";
            expSlider.value = userInfo.exp / Global.GetTotalEXP( userInfo.level );
        }

        SceneBase.EnabledInputSystem( false, false );
        SceneBase.IsLock = true;

        if ( GameManager.LoginInfo != null )
        {
            RESULT_INFO protocol;
            protocol.uid   = GameManager.LoginInfo.Value.uid;
            protocol.kill  = GameManager.LocalPlayer.KillScore;
            protocol.death = GameManager.LocalPlayer.DeathScore;
            Network.Inst.Send( new Packet( UPDATE_RESULT_INFO_REQ, protocol ) );
        }
    }
}
