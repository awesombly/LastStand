using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class OptionSystem : MonoBehaviour
{
    [Header( "< Option >" )]
    public GameObject optionCanvas;
    public RectTransform optionMovement;
    private bool isOptionMovePlaying;

    [Header( "< UserInfo >" )]
    public GameObject userInfoCanvas;
    public TextMeshProUGUI userInfoNickname;
    public TextMeshProUGUI level;
    public Slider exp;

    public TextMeshProUGUI playCount;
    public TextMeshProUGUI kill, death;
    public TextMeshProUGUI killDeathAverage;
    public TextMeshProUGUI bestKill, bestDeath;

    private void Awake()
    {
        if ( TryGetComponent( out LoginSystem login ) )
             login.OnLoginCompleted += LoginCompleted;

        if ( GameManager.LoginInfo is null ) 
            userInfoCanvas.SetActive( false );
        else
        {
            userInfoCanvas.SetActive( true );
            UpdateUserInfo();
        }
    }

    private void LoginCompleted()
    {
        userInfoCanvas.SetActive( true );
        UpdateUserInfo();
    }

    private void UpdateUserInfo()
    {
        if ( GameManager.LoginInfo is null || GameManager.UserInfo is null )
             return;

        LOGIN_INFO loginData = GameManager.LoginInfo.Value;
        userInfoNickname.text = $"- {loginData.nickname} -";

        USER_INFO userData = GameManager.UserInfo.Value;
        level.text = $"{userData.level}";
        exp.value  = ( float )( userData.exp / Global.GetTotalEXP( userData.level ) );
        playCount.text = $"{userData.playCount}";
        kill.text  = $"{userData.kill}";
        death.text = $"{userData.death}";

        float kd = ( ( userData.kill * 100f ) / ( userData.kill + userData.death ) );
        killDeathAverage.text = $"{( float.IsNaN( kd ) ? 0 : kd.ToString( "F1" ) )}";

        bestKill.text  = $"{userData.bestKill}";
        bestDeath.text = $"{userData.bestDeath}";
    }

    public void ActiveOptionPanel()
    {
        if ( isOptionMovePlaying )
             return;

        if ( optionCanvas.activeInHierarchy )
        {
            AudioManager.Inst.Play( SFX.MenuExit );
            isOptionMovePlaying = true;
            optionMovement.DOAnchorPosX( -325f, .5f ).OnComplete( () =>
            {
                optionCanvas.SetActive( false );
                isOptionMovePlaying = false;
            } );
        }
        else
        {
            optionCanvas.SetActive( true );

            // email?.ActivateInputField();
            // if ( password != null )
            //      password.contentType = TMP_InputField.ContentType.Password;

            isOptionMovePlaying = true;
            optionMovement.DOAnchorPosX( 325f, .5f ).OnComplete( () => isOptionMovePlaying = false );
            AudioManager.Inst.Play( SFX.MenuEntry );
        }
    }
}
