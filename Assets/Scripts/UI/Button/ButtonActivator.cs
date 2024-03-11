using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonActivator : MonoBehaviour, IPointerClickHandler
{
    public GameObject panel;
    public bool isActive;
    public UnityEvent OnClick;

    public void OnPointerClick( PointerEventData eventData )
    {
        panel.SetActive( isActive );
        OnClick?.Invoke();

        if ( isActive ) AudioManager.Inst.Play( SFX.MenuEntry );
        else            AudioManager.Inst.Play( SFX.MenuExit );
    }
}
