using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum Resolution { _1920_1080, _1600_900, _1280_720, _960_540, Count, }
public class ResolutionOption : OptionText
{
    protected override void Initialize()
    {
        StringBuilder builder = new StringBuilder();
        for ( int i = 0; i < ( int )Resolution.Count; i++ )
        {
            var replace = ( ( Resolution )i ).ToString().Replace( "_", " " );
            var split   = replace.Trim().Split( ' ' );

            builder.Clear();
            builder.Append( $"{int.Parse( split[0] )}" )  // width
                   .Append( $" x " )
                   .Append( $"{int.Parse( split[1] )}" ); // height

            AddText( builder.ToString() );
        }

        if ( !int.TryParse( Config.Inst.Read( ConfigType.Resolution ), out int index ) )
             index = ( int )Resolution._1920_1080; 

        Current = index;
        Process();
    }

    public override void Process()
    {
        base.Process();

        var replace = ( ( Resolution )Current ).ToString().Replace( "_", " " );
        var split = replace.Trim().Split( ' ' );

        var width  = int.Parse( split[0] );
        var height = int.Parse( split[1] );

        switch ( Global.CurrentScreenMode )
        {
            case ScreenMode.Exclusive_FullScreen:
            Screen.SetResolution( width, height, FullScreenMode.ExclusiveFullScreen );
            break;

            case ScreenMode.FullScreen_Window:
            Screen.SetResolution( width, height, FullScreenMode.FullScreenWindow );
            break;

            case ScreenMode.Windowed:
            Screen.SetResolution( width, height, FullScreenMode.Windowed );
            break;
        }

        Global.CurrentResolution = ( Resolution )Current;
        Config.Inst.Write( ConfigType.Resolution, Current.ToString() );
    }
}
