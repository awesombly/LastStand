using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class OptionText : ScrollBase
{
    [SerializeField]
    protected List<string> texts = new List<string>();

    [SerializeField]
    private TextMeshProUGUI curText;

    protected virtual void Awake()
    {
        Initialize();
        IsLoop = true;
        Length = texts.Count;
        curText.text = texts[Current];
    }

    protected void AddText( string _text ) => texts.Add( _text );

    public override void NextMove()
    {
        base.NextMove();
        curText.text = texts[Current];
        AudioManager.Inst.Play( SFX.MouseClick );
    }

    public override void PrevMove()
    {
        base.PrevMove();
        curText.text = texts[Current];
        AudioManager.Inst.Play( SFX.MouseClick );
    }

    protected abstract void Initialize();

    public abstract void Process();
}