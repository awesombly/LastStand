using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public enum ConfigLogin  { ID, PW, isRemember, };

public class Config : Singleton<Config>
{
    private static readonly string ConfigPath = System.IO.Path.Combine( System.IO.Directory.GetCurrentDirectory(), "config.ini" );
    private StringBuilder text = new StringBuilder( 255 );
    
    public enum SectionType { Login, Volume, }
    [DllImport( "kernel32.dll" )] private static extern uint GetPrivateProfileString( string _section, string _key, string _default, StringBuilder _result, uint _size, string _path );
    [DllImport( "kernel32.dll" )] private static extern bool WritePrivateProfileString( string _section, string _key, string _value, string _path );

    private string GetData( SectionType _type, string _key )
    {
        text.Clear();
        return GetPrivateProfileString( _type.ToString(), _key, string.Empty, text, 255, ConfigPath ) > 0 ? text.ToString() : string.Empty;
    }

    public string Read( ConfigLogin _type ) => GetData( SectionType.Login, _type.ToString() );

    public string Read( MixerType _type )   => GetData( SectionType.Volume, _type.ToString() );

    public void Write( ConfigLogin _type, string _value ) => WritePrivateProfileString( SectionType.Login.ToString(),  _type.ToString(), _value, ConfigPath );

    public void Write( MixerType _type, string _value )   => WritePrivateProfileString( SectionType.Volume.ToString(), _type.ToString(), _value, ConfigPath );
}
