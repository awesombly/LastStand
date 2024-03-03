using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderVolumeController : MonoBehaviour
{
    public MixerType type;
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.value = AudioManager.Inst.GetMixerDecibel( type );
    }

    public void OnMixerVolumeChanged( float _volume )
    {
        AudioManager.Inst.MixerDecibelControl( type, _volume );
    }
}
