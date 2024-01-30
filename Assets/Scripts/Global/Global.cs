using System.Runtime.InteropServices;
using UnityEngine;

public class Global
{
    public static byte[] Serialize( object _obj )
    {
        int copySize = ( ( Packet )_obj ).size;
        int bufSize = Marshal.SizeOf( _obj );

        System.IntPtr buffer = Marshal.AllocHGlobal( bufSize + 1 );
        if ( buffer == System.IntPtr.Zero )
        {
            Debug.LogError( "failed Marshal.AllocHGlobal()." );
            return null;
        }

        Marshal.StructureToPtr( _obj, buffer, false );

        byte[] data = new byte[ copySize ];
        Marshal.Copy( buffer, data, 0, copySize );
        Marshal.FreeHGlobal( buffer );

        return data;
    }

    public static Type Deserialize<Type>( byte[] _value, int startIndex )
    {
        int size = Marshal.SizeOf( typeof( Type ) );
        System.IntPtr buffer = Marshal.AllocHGlobal( size );
        Marshal.Copy( _value, startIndex, buffer, size );
        Type obj = ( Type )Marshal.PtrToStructure( buffer, typeof( Type ) );
        Marshal.FreeHGlobal( buffer );

        return obj;
    }
}