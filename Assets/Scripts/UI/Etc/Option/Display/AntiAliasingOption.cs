using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AntiAliasing { None, _2xMultiSampling, _4xMultiSampling, _8xMultiSampling, Count, }
public class AntiAliasingOption : OptionText
{
    protected override void Initialize()
    {
        for ( int i = 0; i < ( int )AntiAliasing.Count; i++ )
        {
            switch ( ( AntiAliasing )i )
            {
                case AntiAliasing.None:             texts.Add( $"Off" );     break;
                case AntiAliasing._2xMultiSampling: texts.Add( $"2x MSAA" ); break;
                case AntiAliasing._4xMultiSampling: texts.Add( $"4x MSAA" ); break;
                case AntiAliasing._8xMultiSampling: texts.Add( $"8x MSAA" ); break;
            }
        }

        if ( !int.TryParse( Config.Inst.Read( ConfigType.AntiAliasing ), out int index ) )
             index = ( int )AntiAliasing.None;

        Current = index;
        Process();
    }

    public override void Process()
    {
        base.Process();
        QualitySettings.antiAliasing = Current == 1 ? 2  :
                                       Current == 2 ? 4  :
                                       Current == 3 ? 8  :
                                       Current == 4 ? 16 : 0;

        Global.CurrentAntiAliasing = ( AntiAliasing )Current;
        Config.Inst.Write( ConfigType.AntiAliasing, Current.ToString() );
        AudioManager.Inst.Play( SFX.MenuEntry );
    }
}
