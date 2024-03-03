using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent( typeof( Image ) )]
[RequireComponent( typeof( Button ) )]
public class ButtonActivator : MonoBehaviour
{
    private Image image;
    private Button button;
    public Color enableColor  = Color.white;
    public Color disableColor = Color.gray;

    public bool Enabled { set => image.color = value ? enableColor : disableColor; }

    private void Awake()
    {
        image  = GetComponent<Image>();
        button = GetComponent<Button>();

        button.onClick.AddListener( () => Enabled = true );
    }
}