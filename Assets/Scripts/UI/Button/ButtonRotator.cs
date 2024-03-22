using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonRotator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool  isLoop = false;
    public float power = 25f;

    private Transform tf;
    private Vector3 rot;
    private bool isPlay;

    private void Awake()
    {
        tf  = transform;
        rot = Vector3.one;

        isPlay = isLoop;
    }

    private void Update()
    {
        if ( isPlay )
        {
            rot.z -= power * Time.deltaTime;
            tf.rotation = Quaternion.Euler( rot );
        }
    }

    public void OnPointerEnter( PointerEventData eventData )
    {
        isPlay = true;
    }

    public void OnPointerExit( PointerEventData eventData )
    {
        isPlay = isLoop;
        rot = Vector3.one;
        tf.rotation = Quaternion.Euler( rot );
    }
}
