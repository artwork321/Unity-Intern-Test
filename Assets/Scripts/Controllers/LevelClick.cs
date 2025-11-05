using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelClick : LevelCondition
{

    private HolderController m_holder;

    private BoardController m_board;

    public override void Setup(Text txt, BoardController board, HolderController itemHolder)
    {
        base.Setup(txt, board, itemHolder);

        m_txt.text = "FREE PLAY";

        m_board = board;

        m_holder = itemHolder;

        m_board.OnMoveEvent += OnMove;
    }
    

    private void OnMove()
    {
        UnityEngine.Debug.Log(m_holder.IsFull());

        if (m_conditionCompleted) return;

        if (m_board.IsEmpty() && !m_holder.IsFull())
        {
            OnConditionComplete();
        }
        else if (m_holder.IsFull())
        {
            OnConditionLose();
        }
    }

    protected override void OnDestroy()
    {
        if (m_board != null) m_board.OnMoveEvent -= OnMove;

        base.OnDestroy();
    }
}
