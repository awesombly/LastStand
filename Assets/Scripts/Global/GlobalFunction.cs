using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Pool;

public static partial class Global
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
        return JsonConvert.DeserializeObject<Type>( System.Text.Encoding.UTF8.GetString( _packet.data ) );
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

namespace WNS
{
    public interface IObjectPool<T> where T : MonoBehaviour
    {
        public ObjectPool<T> pool { get; set; }
    }

    public class ObjectPool<T> where T : MonoBehaviour
    {
        private T prefab;
        private Transform parent;
        private Stack<T>  objects  = new Stack<T>();
        private int allocateCount;

        public ObjectPool( T _prefab, int _initializeCount, int _allocateCount = 1 )
        {
            allocateCount = _allocateCount;

            if ( ReferenceEquals( _prefab, null ) )
                 Debug.LogError( "objectpool Constructor failed" );

            prefab = _prefab;
            
            GameObject canvas = GameObject.Find( "Pools" );
            // GameObject.FindGameObjectWithTag( "Pools" );
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
            Allocate( _initializeCount );
        }

        public ObjectPool( T _prefab, Transform _parent, int _initializeCount, int _allocateCount = 1 )
        {
            allocateCount = _allocateCount;

            if ( ReferenceEquals( _prefab, null ) )
                 Debug.LogError( "objectpool Constructor failed" );

            prefab = _prefab;
            parent = _parent;
            Allocate( _initializeCount );
        }

        private void Allocate( int _allocateCount )
        {
            if ( _allocateCount < 0 )
                 return;

            T[] newObjects = new T[_allocateCount];
            for ( int i = 0; i < _allocateCount; i++ )
            {
                T obj = UnityEngine.GameObject.Instantiate( prefab, parent );
                if ( obj.TryGetComponent( out IObjectPool<T> _base ) )
                     _base.pool = this;

                obj.gameObject.SetActive( false );
                newObjects[i] = obj;
                objects.Push( obj );
            }
        }

        public T Spawn( Transform _parent = null )
        {
            if ( objects.Count == 0 )
                 Allocate( allocateCount );


            T obj = objects.Pop();
            obj.gameObject.SetActive( true );

            if ( _parent != null )
                 obj.transform.SetParent( _parent );

            return obj;
        }

        public void Despawn( T _obj )
        {
            _obj.transform.SetParent( parent );
            _obj.gameObject.SetActive( false );
            objects.Push( _obj );
        }
    }
}