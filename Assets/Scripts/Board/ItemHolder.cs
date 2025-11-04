using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHolder
{
    private int holderSize;

    private Cell[] m_cells;

    private Transform m_root;

    private int m_matchMin;

    public ItemHolder(Transform transform, GameSettings gameSettings)
    {
        m_root = transform;

        m_matchMin = gameSettings.MatchesMin;

        this.holderSize = gameSettings.HolderSize;

        m_cells = new Cell[holderSize];

        CreateHolder(gameSettings);
    }

    private void CreateHolder(GameSettings gameSettings)
    {
        // Calculate Board Size
        float positionY = -gameSettings.BoardSizeY * 0.5f - 1f;


        Vector3 origin = new Vector3(-gameSettings.BoardSizeX * 0.5f, positionY, 0f);
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);

        for (int x = 0; x < holderSize; x++)
        {
            GameObject go = GameObject.Instantiate(prefabBG);
            go.transform.position = origin + new Vector3(x, 0, 0f);
            go.transform.SetParent(m_root);

            Cell cell = go.GetComponent<Cell>();
            cell.Setup(x, 0);

            m_cells[x] = cell;
        }
    }

    public Cell GetEmptyCell()
    {
        for (int i = 0; i < m_cells.Length; i++)
        {
            if (m_cells[i].IsEmpty)
            {
                return m_cells[i];
            }
        }

        return null;
    }

    public List<Cell> GetMatches(Cell cell)
    {
        List<Cell> list = new List<Cell>();
        list.Add(cell);

        foreach (Cell o_cell in m_cells)
        {
            if (o_cell.IsSameType(cell) && o_cell != cell)
            {
                list.Add(o_cell);
            }
        }

        return list;
    }


    internal List<Cell> FindMatch()
    {
        List<Cell> list = new List<Cell>();

        for (int x = 0; x < holderSize; x++)
        {
            Cell cell = m_cells[x];

            var listhor = GetMatches(cell);
            if (listhor.Count >= m_matchMin)
            {
                list = listhor;
                break;
            }
        }

        return list;
    }

}
