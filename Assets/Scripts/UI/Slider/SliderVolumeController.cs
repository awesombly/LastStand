using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderVolumeController : MonoBehaviour, IPointerUpHandler
{
    public MixerType type;
    private Slider slider;

    private void Awake()
    {
        if ( TryGetComponent( out slider ) )
             slider.onValueChanged.AddListener( OnMixerVolumeChanged );

        if ( float.TryParse( Config.Inst.Read( type ), out float volume ) ) slider.value = volume;
        else                                                                Config.Inst.Write( type, slider.value.ToString() );
    }

    private void OnMixerVolumeChanged( float _volume )
    {
        AudioManager.Inst.MixerDecibelControl( type, _volume );
    }

    public void OnPointerUp( PointerEventData eventData )
    {
        Config.Inst.Write( type, slider.value.ToString() );
    }
}