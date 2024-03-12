using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 단순 숫자 카운팅 클래스 입니다.
public class ScrollBase : MonoBehaviour
{
    public bool IsDuplicate  { get; private set; }
    public bool IsLoop       { get; set; } = false;
    public int Length        { get; set; }

    private int current;
    public int Current
    {
        get => current;
        set
        {
            current = value;
            Previous = current - 1 < 0 ? Length - 1 : current - 1;
        }
    }

    public int Previous { get; private set; }

    public virtual void PrevMove()
    {
        if ( current == 0 )
        {
            if ( IsLoop )
            {
                Previous = current;
                current  = Length - 1;
                return;
            }

            IsDuplicate = true;
            return;
        }

        Previous = current--;
        IsDuplicate = false;
    }

    public virtual void NextMove()
    {
        if ( current == Length - 1 )
        {
            if ( IsLoop )
            {
                Previous = current;
                current = 0;
                return;
            }

            IsDuplicate = true;
            return;
        }

        Previous    = current++;
        IsDuplicate = false;
    }
}
