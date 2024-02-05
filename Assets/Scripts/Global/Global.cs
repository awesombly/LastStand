using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class Global
{
    public const int HeaderSize  = 4;
    public const int MaxDataSize = 2048;

    public static byte[] Serialize( object _obj )
    {
        int copySize = ( ( Packet )_obj ).size;
        int bufSize = Marshal.SizeOf( _obj );

        System.IntPtr buffer = Marshal.AllocHGlobal( bufSize + 1 );
        if ( buffer == System.IntPtr.Zero )
             return null;

        Marshal.StructureToPtr( _obj, buffer, false );

        byte[] data = new byte[copySize];
        Marshal.Copy( buffer, data, 0, copySize );
        Marshal.FreeHGlobal( buffer );

        return data;
    }

    public static Type Deserialize<Type>( byte[] _value, int _start )
    {
        int size = Marshal.SizeOf( typeof( Type ) );
        System.IntPtr buffer = Marshal.AllocHGlobal( size );
        Marshal.Copy( _value, _start, buffer, size );
        Type obj = ( Type )Marshal.PtrToStructure( buffer, typeof( Type ) );
        Marshal.FreeHGlobal( buffer );

        return obj;
    }

    public static Type FromJson<Type>( in Packet _packet )
    {
        string json = System.Text.Encoding.UTF8.GetString( _packet.data, 0, _packet.size - HeaderSize );
        return JsonUtility.FromJson<Type>( json );
    }
}

public class YieldCache
{
    class FloatComparer : IEqualityComparer<float>
    {
        bool IEqualityComparer<float>.Equals( float x, float y ) => x == y;

        int IEqualityComparer<float>.GetHashCode( float obj ) => obj.GetHashCode();
    }

    private static readonly Dictionary<float/*time*/, WaitForSeconds> times = new Dictionary<float, WaitForSeconds>( new FloatComparer() );
    public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
    public static WaitForSeconds WaitForSeconds( float _time )
    {
        WaitForSeconds wfs;
        if ( !times.TryGetValue( _time, out wfs ) )
             times.Add( _time, wfs = new WaitForSeconds( _time ) );

        return wfs;
    }
}