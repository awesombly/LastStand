using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static partial class Global
{
    [Flags]
    public enum LayerValue
    {
        Default         = 1 << 0,
        TransparentFX   = 1 << 1,
        IgnoreRaycast   = 1 << 2,
        Temp            = 1 << 3,
        Water           = 1 << 4,
        UI              = 1 << 5,
        Player          = 1 << 6,
        Enemy           = 1 << 7,
        EnemyArea       = 1 << 8,
        PlayerAttack    = 1 << 9,
        EnemyAttack     = 1 << 10,
        Misc            = 1 << 11,
    }

    public static bool CompareLayer( LayerValue _flagLayer, int _intLayer )
    {
        int result = ( int )_flagLayer & ( 1 << _intLayer );
        return result != 0;
    }

    [Serializable]
    public struct StatusFloat
    {
        [SerializeField]
        private float max;
        public float Max
        {
            get => max;
            set
            {
                if ( max == value )
                {
                    return;
                }

                float old = max;
                max = value;
                current = Math.Min( current, max );

                OnChangeMax?.Invoke( old, max );
            }
        }
        public Action<float/*old*/, float/*new*/> OnChangeMax;

        private float current;
        public float Current
        {
            get => current;
            set
            {
                if ( current == value )
                {
                    return;
                }

                float old = current;
                current = Mathf.Clamp( value, 0f, Max );

                OnChangeCurrent?.Invoke( old, current );
            }
        }
        public Action<float/*old*/, float/*new*/> OnChangeCurrent;

        public bool IsMax { get => Current == Max; }
        public bool IsZero { get => Current == 0f; }
        public void SetMax() => Current = max;
        public void SetZero() => Current = 0f;
    }

    [Serializable]
    public struct StatusInt
    {
        [SerializeField]
        private int max;
        public int Max
        {
            get => max;
            set
            {
                if ( max == value )
                {
                    return;
                }

                int old = max;
                max = value;
                current = Math.Min( current, max );

                OnChangeMax?.Invoke( old, max );
            }
        }
        public Action<int/*old*/, int/*new*/> OnChangeMax;

        private int current;
        public int Current
        {
            get => current;
            set
            {
                if ( current == value )
                {
                    return;
                }

                int old = current;
                current = Mathf.Clamp( value, 0, Max );

                OnChangeCurrent?.Invoke( old, current );
            }
        }
        public Action<int/*old*/, int/*new*/> OnChangeCurrent;

        public bool IsMax { get => Current == Max; }
        public bool IsZero { get => Current == 0; }
        public void SetMax() => Current = max;
        public void SetZero() => Current = 0;
    }
}
