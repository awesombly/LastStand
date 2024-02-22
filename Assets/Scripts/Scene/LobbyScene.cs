using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : SceneBase
{
    protected override void Awake()
    {
        base.Awake();
        SceneType = SceneType.Lobby;

        StartCoroutine( WaitForAudioLoad() );
    }

    private IEnumerator WaitForAudioLoad()
    {
        yield return new WaitUntil( () => !AudioManager.Inst.IsLoading );
        AudioManager.Inst.Play( BGMType.Default, BGMSound.Lobby, 0f, 1f, 5f );
    }

    protected override void Start()
    {
        base.Start();
    }
}
