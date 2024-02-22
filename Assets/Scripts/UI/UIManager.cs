using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Sample이랑 합쳐도 될듯
public class UIManager : Singleton<UIManager>
{
    public Camera uiCamera;

    public TextMeshProUGUI magazineText;
    public TextMeshProUGUI ammoText;

    protected override void Awake()
    {
        base.Awake();
        uiCamera.clearFlags = CameraClearFlags.Depth;
    }
}
