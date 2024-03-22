using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class InitScene : SceneBase
{
    public CanvasGroup     loadPanel;
    public TextMeshProUGUI loadText;
    private string[] loadTexts = { "���� ���� ��.", "���� ���� ��..", "���� ���� ��..." };

    public Transform ipSelectPanel;
    public TextMeshProUGUI ipSelectTitle;

    protected override void Awake()
    {
        base.Awake();
        SceneType = SceneType.Init;

        ipSelectPanel.localScale = new Vector3( 0f, 1f, 1f );
        Network.Inst.OnDisconnected += OnDisconnect;
    }

    protected override void Start()
    {
        base.Start();

        StartCoroutine( WaitForLoad() );
        StartCoroutine( UpdateLoadText() );
        StartCoroutine( WaitForConnected() );
    }

    private void OnDestroy()
    {
        Network.Inst.OnDisconnected -= OnDisconnect;
    }

    private IEnumerator UpdateLoadText()
    {
        int count = 0;
        while ( true )
        {
            yield return YieldCache.WaitForSeconds( .35f );

            loadText.text = loadTexts[count++];
            if ( count >= loadTexts.Length )
                 count = 0;
        }
    }

    public void SelectIp()
    {
        AudioManager.Inst.Play( SFX.MenuEntry );
        ipSelectPanel.DOScaleX( 0f, .35f )
                     .OnComplete( () => loadPanel.DOFade( 1f, .25f )
                     .OnComplete( () => ipSelectPanel.gameObject.SetActive( false ) ) );
    }

    private IEnumerator WaitForLoad()
    {
        yield return new WaitUntil( () => !AudioManager.Inst.IsLoading );
        yield return YieldCache.WaitForSeconds( .5f );

        ipSelectPanel.gameObject.SetActive( true );
        ipSelectPanel.DOScaleX( 1f, .35f );
    }

    private IEnumerator WaitForConnected()
    {
        yield return new WaitUntil( () => Network.Inst.IsConnected );
        yield return YieldCache.WaitForSeconds( .5f );

        // LoadScene�� �񵿱� �۾� �߿� �߻��� �̺�Ʈ������ ������ �ȵǴ� ��
        LoadScene( SceneType.Lobby );
    }

    private void OnDisconnect()
    {
        loadPanel.DOFade( 0f, .25f )
                 .OnComplete( () =>
        {
            ipSelectPanel.gameObject.SetActive( true );
            ipSelectTitle.text = "������ ������ �� �����ϴ�.";
            ipSelectPanel.DOScaleX( 1f, .35f );
        } );
    }
}