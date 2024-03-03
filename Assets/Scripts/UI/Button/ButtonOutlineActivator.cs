using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent( typeof( Button ) )]
[RequireComponent( typeof( Outline ) )]
public class ButtonOutlineActivator : MonoBehaviour
{
    private Outline outline;
    private Button button;

    private void Awake()
    {
        outline = GetComponent<Outline>();
        button = GetComponent<Button>();

        button.onClick.AddListener( () => outline.enabled = true );
    }
}
