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

    public void CollapseMatchesAndShift()
    {
        List<Cell> matches = m_holder.FindMatch();
        if (matches.Count > 0)
        {
            StartCoroutine(MatchAndShift(matches));
        }
    }

    public IEnumerator MatchAndShift(List<Cell> matches)
    {
        CollapseMatches(matches);
        yield return new WaitForSeconds(0.2f); // Wait for explode animation

        m_holder.ShiftItemsToFillGap();
        yield return new WaitForSeconds(0.2f); // Wait for shift animation
    }

    private void CollapseMatches(List<Cell> matches)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            matches[i].ExplodeItem();
        }
    }
}
