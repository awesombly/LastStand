using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static partial class Global
{
    public static readonly int RoundDigit = 3;

    public struct Layer
    {
        public const int Default       = 0;
        public const int TransparentFX = 1;
        public const int IgnoreRaycast = 2;
        public const int Temp          = 3;
        public const int Water         = 4;
        public const int UI            = 5;
        public const int Player        = 6;
        public const int Enemy         = 7;
        public const int Wall          = 8;
        public const int PlayerAttack  = 9;
        public const int EnemyAttack   = 10;
        public const int Misc          = 11;
    }

    [Flags]
    public enum LayerFlag
    {
        Default         = 1 << Layer.Default,
        TransparentFX   = 1 << Layer.TransparentFX,
        IgnoreRaycast   = 1 << Layer.IgnoreRaycast,
        Temp            = 1 << Layer.Temp,
        Water           = 1 << Layer.Water,
        UI              = 1 << Layer.UI,
        Player          = 1 << Layer.Player,
        Enemy           = 1 << Layer.Enemy,
        Wall            = 1 << Layer.Wall,
        PlayerAttack    = 1 << Layer.PlayerAttack,
        EnemyAttack     = 1 << Layer.EnemyAttack,
        Misc            = 1 << Layer.Misc,
    }

    public static bool CompareLayer( LayerFlag _flagLayer, int _intLayer )
    {
        int result = ( int )_flagLayer & ( 1 << _intLayer );
        return result != 0;
    }

    [Serializable]
    public struct StatusFloat
    {
        [SerializeField, Min( 0f )]
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
                value = Mathf.Clamp( value, 0f, Max );
                if ( current == value )
                {
                    return;
                }

                float old = current;
                current = value;
                OnChangeCurrent?.Invoke( old, current, max );
            }
        }
        public Action<float/*old*/, float/*new*/, float/*max*/> OnChangeCurrent;

        public bool IsMax { get => Current == Max; }
        public bool IsZero { get => Current == 0f; }
        public void SetMax() => Current = max;
        public void SetZero() => Current = 0f;
    }

    [Serializable]
    public struct StatusInt
    {
        [SerializeField, Min( 0 )]
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
                value = Mathf.Clamp( value, 0, Max );
                if ( current == value )
                {
                    return;
                }

                int old = current;
                current = value;
                OnChangeCurrent?.Invoke( old, current, max );
            }
        }
        public Action<int/*old*/, int/*new*/, int/*max*/> OnChangeCurrent;

        public bool IsMax { get => Current == Max; }
        public bool IsZero { get => Current == 0; }
        public void SetMax() => Current = max;
        public void SetZero() => Current = 0;
    }
}
