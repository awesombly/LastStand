using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameUIScene : SceneBase
{
    public Camera uiCamera;

    public TextMeshProUGUI magazineText;
    public TextMeshProUGUI ammoText;

    protected override void Awake()
    {
        base.Awake();
        SceneType = SceneType.InGame_UI;
        uiCamera.clearFlags = CameraClearFlags.Depth;
    }
    protected override void Start()
    {
        base.Start();
    }
}
