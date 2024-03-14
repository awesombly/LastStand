using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public enum ConfigType : ushort
{
    // Login
    ID, PW, isRemember,

    ScreenMode, Resolution, FrameRate, AntiAliasing,
};

public class Config : Singleton<Config>
{
    private static readonly string ConfigPath  = System.IO.Path.Combine( Global.DefaultDirectory, "config.ini" );
    private static readonly string SectionName = "Config";
    

    private StringBuilder text = new StringBuilder( 255 );
    [DllImport( "kernel32.dll" )] private static extern uint GetPrivateProfileString( string _section, string _key, string _default, StringBuilder _result, uint _size, string _path );
    [DllImport( "kernel32.dll" )] private static extern bool WritePrivateProfileString( string _section, string _key, string _value, string _path );

    public string Read<T>( T _key ) where T : System.Enum
    {
#if !UNITY_ANDROID && !UNITY_IOS
        text.Clear();
        return GetPrivateProfileString( SectionName, _key.ToString(), string.Empty, text, 255, ConfigPath ) > 0 ? text.ToString() : string.Empty;
#else
        return string.Empty;
#endif
    }

    public void Write<T>( T _key, string _value ) where T : System.Enum
    {
#if !UNITY_ANDROID && !UNITY_IOS
        if ( !System.IO.Directory.Exists( Global.DefaultDirectory )  )
             System.IO.Directory.CreateDirectory( Global.DefaultDirectory );

        WritePrivateProfileString( SectionName, _key.ToString(), _value, ConfigPath );
#endif
    }
}
