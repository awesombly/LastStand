using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ScreenMode { Exclusive_FullScreen, FullScreen_Window, Windowed, Count, }
public class ScreenModeOption : OptionText
{
    protected override void Initialize()
    {
        int length = ( int )ScreenMode.Count;
        for ( int i = 0; i < length; i++ )
        {
            switch( ( ScreenMode )i )
            {
                case ScreenMode.Exclusive_FullScreen: AddText( $"��üȭ��" ); break;
                case ScreenMode.FullScreen_Window:    AddText( $"�׵θ����� â���" ); break;
                case ScreenMode.Windowed:             AddText( $"â���" ); break;
            }
        }

        if ( !int.TryParse( Config.Inst.Read( ConfigType.ScreenMode ), out int index ) )
             index = ( int )ScreenMode.Windowed;
            
        Current = index;
        Process();
    }

    public override void Process()
    {
        base.Process();
        var type = ( ScreenMode )Current;
        switch ( type )
        {
            case ScreenMode.Exclusive_FullScreen:
            Screen.SetResolution( Screen.width, Screen.height, FullScreenMode.ExclusiveFullScreen );
            break;

            case ScreenMode.FullScreen_Window:
            Screen.SetResolution( Screen.width, Screen.height, FullScreenMode.FullScreenWindow );
            break;

            case ScreenMode.Windowed:
            Screen.SetResolution( Screen.width, Screen.height, FullScreenMode.Windowed );
            break;
        }

        Global.CurrentScreenMode = type;
        Config.Inst.Write( ConfigType.ScreenMode, Current.ToString() );
    }

}
