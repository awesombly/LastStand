using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent( typeof( Image ) )]
[RequireComponent( typeof( Button ) )]
public class ButtonSelector : MonoBehaviour
{
    private Image image;
    private Button button;

    public bool Enabled { set => image.color = value ? Color.white : Color.gray; }

    private void Awake()
    {
        image  = GetComponent<Image>();
        button = GetComponent<Button>();

        button.onClick.AddListener( () =>
        {
            Enabled = true;
            AudioManager.Inst.Play( SFX.MouseClick );
        } );
    }
}