using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolderController : MonoBehaviour
{
    private ItemHolder m_holder;

    private GameManager m_gameManager;

    private GameSettings m_gameSettings;

    private bool m_gameOver;

    public void StartGame(GameManager gameManager, GameSettings gameSettings)
    {
        m_gameManager = gameManager;

        m_gameSettings = gameSettings;

        m_holder = new ItemHolder(this.transform, gameSettings);
    }

    public Cell GetEmptyCell()
    {
        return m_holder.GetEmptyCell();
    }

    public Cell GetCellWithSameType(Cell cell)
    {
        return m_holder.GetCellWithSameType(cell);
    }

    public int GetIndexCellWithSameType(Cell cell)
    {
        return m_holder.GetIndexCellWithSameType(cell);
    }

    public Cell GetCellAtIndex(int i)
    {
        return m_holder.GetCellAtIndex(i);
    }

    public void ShiftItemsRightFromIndex(int i)
    {
        m_holder.ShiftItemsRightFromIndex(i);
    }

    public void FindMatchesAndCollapse()
    {
        List<Cell> matches = m_holder.FindMatch();
        if (matches.Count > 0)
        {
            CollapseMatches(matches);
            ShiftItemsToFillGap();
        }
    }

    public void ShiftItemsToFillGap()
    {
        m_holder.ShiftItemsToFillGap();
    }

    private void CollapseMatches(List<Cell> matches)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            matches[i].ExplodeItem();
        }
    }
}
