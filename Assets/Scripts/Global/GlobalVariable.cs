using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static partial class Global
{
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
                current = Mathf.Clamp( value, 0.0f, Max );

                OnChangeCurrent?.Invoke( old, current );
            }
        }
        public Action<float/*old*/, float/*new*/> OnChangeCurrent;

        public void SetMax() => Current = max;

        public void SetZero() => Current = 0.0f;
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
        public Action<float/*old*/, float/*new*/> OnChangeMax;

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
        public Action<float/*old*/, float/*new*/> OnChangeCurrent;

        public void SetMax() => Current = max;

        public void SetZero() => Current = 0;
    }
}
