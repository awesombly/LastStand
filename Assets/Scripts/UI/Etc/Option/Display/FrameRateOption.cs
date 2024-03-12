using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FrameRate { vSync, No_Limit, _60, _144, _240, _960, Count, }
public class FrameRateOption : OptionText
{
    protected override void Initialize()
    {
        for ( int i = 0; i < ( int )FrameRate.Count; i++ )
        {
            switch ( ( FrameRate )i )
            {
                case FrameRate.No_Limit: texts.Add( $"제한없음" );    break;
                case FrameRate.vSync:    texts.Add( $"수직 동기화" ); break;
                case FrameRate._60:      texts.Add( $"60 FPS" );     break;
                case FrameRate._144:     texts.Add( $"144 FPS" );    break;
                case FrameRate._240:     texts.Add( $"240 FPS" );    break;
                case FrameRate._960:     texts.Add( $"960 FPS" );    break;
            }
        }

        if ( int.TryParse( Config.Inst.Read( ConfigType.FrameRate ), out int index ) )
        {
            Current = index;
            Process();
        }
    }
    public override void Process()
    {
        var type = ( FrameRate )Current;
        switch ( type )
        {
            case FrameRate.vSync:
                 QualitySettings.vSyncCount  = 1;
                 Application.targetFrameRate = 0;
                 break;

            case FrameRate.No_Limit:
                 QualitySettings.vSyncCount  = 0;
                 Application.targetFrameRate = 0; 
                 break;

            case FrameRate._60:
            case FrameRate._144:
            case FrameRate._240:
            case FrameRate._960:
            {
                QualitySettings.vSyncCount = 0;
                var frame = ( type ).ToString().Replace( "_", " " );
                Application.targetFrameRate = int.Parse( frame );
            } break;
        }

        Global.CurrentFrameRate = type;
        Config.Inst.Write( ConfigType.FrameRate, Current.ToString() );
        AudioManager.Inst.Play( SFX.MenuEntry );
    }
}
