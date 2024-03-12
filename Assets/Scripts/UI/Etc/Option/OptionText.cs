using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class OptionText : ScrollBase
{
    [SerializeField]
    protected List<string> texts = new List<string>();
    [SerializeField]
    private TextMeshProUGUI curText;
    [SerializeField]
    private Button undo;
    private int undoIndex;
    private bool isFirstEnterd = true;

    protected virtual void Awake()
    {
        if ( undo is not null )
             undo.onClick.AddListener( Undo );

        Initialize();
        IsLoop = true;
        Length = texts.Count;
        curText.text = texts[Current];
    }

    private void Undo()
    {
        curText.text = texts[undoIndex];
        Current = undoIndex;

        if ( undo is not null )
             undo.image.enabled = undoIndex != Current;

        AudioManager.Inst.Play( SFX.MouseClick );
    }

    public override void NextMove()
    {
        base.NextMove();
        curText.text = texts[Current];
        AudioManager.Inst.Play( SFX.MouseClick );

        if ( undo is not null )
             undo.image.enabled = undoIndex != Current;
    }

    public override void PrevMove()
    {
        base.PrevMove();
        curText.text = texts[Current];
        AudioManager.Inst.Play( SFX.MouseClick );

        if ( undo is not null )
             undo.image.enabled = undoIndex != Current;
    }

    protected void AddText( string _text ) => texts.Add( _text );


    public virtual void Process()
    {
        undoIndex = Current;
        if ( undo is not null )
             undo.image.enabled = undoIndex != Current;

        if ( isFirstEnterd )
        {
            isFirstEnterd = false;
            return;
        }
        AudioManager.Inst.Play( SFX.MenuEntry );
    }

    protected abstract void Initialize();

}