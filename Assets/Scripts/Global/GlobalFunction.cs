using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using UnityEngine;

public static partial class Global
{
    public const int HeaderSize  = 4;
    public const int MaxDataSize = 2048;
    public const int PacketSize  = HeaderSize + MaxDataSize;

    public static byte[] Serialize( in Packet _obj )
    {
        int copySize = _obj.size;
        int bufSize = Marshal.SizeOf( _obj );

        System.IntPtr buffer = Marshal.AllocHGlobal( bufSize + 1 );
        if ( buffer == System.IntPtr.Zero )
             return null;

        Marshal.StructureToPtr( _obj, buffer, false );
        
        byte[] data = new byte[copySize];
        Marshal.Copy( buffer, data, 0, copySize );

        Marshal.DestroyStructure<Packet>( buffer );
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
        return JsonConvert.DeserializeObject<Type>( System.Text.Encoding.UTF8.GetString( _packet.data ) );
    }

    public static float GetAngle( Vector3 _from, Vector3 _to )
    {
        Vector3 dir = ( _to - _from ).normalized;
        return Mathf.Atan2( dir.y, dir.x ) * Mathf.Rad2Deg;
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

public static class Debug
{
    [Conditional( "UNITY_EDITOR" )] public static void Log( object _message )        => UnityEngine.Debug.Log( _message );
    [Conditional( "UNITY_EDITOR" )] public static void LogWarning( object _message ) => UnityEngine.Debug.LogWarning( _message );
    [Conditional( "UNITY_EDITOR" )] public static void LogError( object _message )   => UnityEngine.Debug.LogError( _message );

    [Conditional( "UNITY_EDITOR" )] public static void Log( object _message, UnityEngine.Object _context )        => UnityEngine.Debug.Log( _message, _context );
    [Conditional( "UNITY_EDITOR" )] public static void LogWarning( object _message, UnityEngine.Object _context ) => UnityEngine.Debug.LogWarning( _message, _context );
    [Conditional( "UNITY_EDITOR" )] public static void LogError( object _message, UnityEngine.Object _context )   => UnityEngine.Debug.LogError( _message, _context );
}

namespace WNS
{
    public static class Math
    {
        public static float  Lerp( float  _start, float  _end, float  _t ) => _start + ( _end - _start ) * _t;
        public static double Lerp( double _start, double _end, double _t ) => _start + ( _end - _start ) * _t;
        public static double Abs( double _value ) => _value >= 0d ? _value : -_value;
        public static float  Abs( float  _value ) => _value >= 0f ? _value : -_value;
        public static int    Abs( int    _value ) => _value >= 0  ? _value : -_value;
        public static double Round( double _value ) => _value - ( int )_value >= .5d ? ( int )_value + 1d : ( int )_value;
        public static float  Round( float  _value ) => _value - ( int )_value >= .5f ? ( int )_value + 1f : ( int )_value;
        public static int    Clamp( int    _value, int    _min, int    _max )
        {
            return _value < _min ? _min :
                   _value > _max ? _max :
                   _value;
        }
        public static float  Clamp( float  _value, float  _min, float  _max )
        {
            return _value < _min ? _min :
                   _value > _max ? _max :
                   _value;
        }
        public static double Clamp( double _value, double _min, double _max )
        {
            return _value < _min ? _min :
                   _value > _max ? _max :
                   _value;
        }
        public static int    Log10( double _value )
        {
            return ( _value >= 10000000u ) ? 7 : ( _value >= 1000000u ) ? 6 :
                   ( _value >= 100000u ) ? 5 : ( _value >= 10000u ) ? 4 :
                   ( _value >= 1000u ) ? 3 : ( _value >= 100u ) ? 2 :
                   ( _value >= 10u ) ? 1 : 0;
        }
    }
    public interface IObjectPool<T> where T : MonoBehaviour
    {
        public ObjectPool<T> pool { get; set; }
    }

    public class ObjectPool<T> where T : MonoBehaviour
    {
        private T prefab;
        private Transform parent;
        private Stack<T>  objects = new Stack<T>();
        private LinkedList<T> activeObjects = new LinkedList<T>();
        private int allocate = 1;

        public ObjectPool( T _prefab, Transform _parent )
        {
            if ( ReferenceEquals( _prefab, null ) )
            {
                Debug.LogError( "Prefab is null" );
                return;
            }
            prefab = _prefab;
            parent = _parent;
            Allocate( 5 );
        }

        public ObjectPool( T _prefab, int _initialize = 5, int _allocate = 1 )
        {
            allocate = _allocate;

            if ( ReferenceEquals( _prefab, null ) )
            {
                Debug.LogError( "Prefab is null" );
                return;
            }

            prefab = _prefab;
            
            GameObject canvas = GameObject.Find( "Pools" );
            if ( ReferenceEquals( canvas, null ) )
            {
                canvas = new GameObject();
                canvas.transform.localPosition = Vector3.zero;
                canvas.transform.localRotation = Quaternion.identity;
                canvas.transform.localScale    = Vector3.one;
                canvas.name = $"Pools";
            }

            GameObject parentObj = new GameObject();
            parentObj.transform.parent        = canvas.transform;
            parentObj.transform.localPosition = Vector3.zero;
            parentObj.transform.localRotation = Quaternion.identity;
            parentObj.transform.localScale    = Vector3.one;
            parentObj.name = $"{typeof( T ).Name} Pool";

            parent = parentObj.transform;
            Allocate( _initialize );
        }

        private void Allocate( int _allocate )
        {
            if ( _allocate < 0 )
                 return;

            for ( int i = 0; i < _allocate; i++ )
            {
                T obj = UnityEngine.GameObject.Instantiate( prefab, parent );
                if ( obj.TryGetComponent( out IObjectPool<T> _base ) )
                     _base.pool = this;

                obj.gameObject.SetActive( false );
                objects.Push( obj );
            }
        }

        public T Spawn()
        {
            if ( objects.Count == 0 )
                 Allocate( allocate );

            T obj = objects.Pop();
            obj.gameObject.SetActive( true );
            activeObjects.AddLast( obj );

            return obj;
        }

        public void Despawn( T _obj )
        {
            activeObjects.Remove( _obj );
            _obj.gameObject.SetActive( false );
            objects.Push( _obj );
        }

        public void AllDespawn()
        {
            foreach ( var obj in activeObjects )
            {
                obj.gameObject.SetActive( false );
                objects.Push( obj );
            }

            activeObjects.Clear();
        }
    }
}