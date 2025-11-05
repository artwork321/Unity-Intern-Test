using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
            cell.Setup(x, 0, false);

            m_cells[x] = cell;
        }
    }

    public bool IsFull()
    {
        for (int i = 0; i < m_cells.Length; i++)
        {
            if (m_cells[i].IsEmpty)
            {
                return false;
            }
        }

        return true;
    }

    public bool IsEmpty()
    {
        for (int i = 0; i < m_cells.Length; i++)
        {
            if (!m_cells[i].IsEmpty)
            {
                return false;
            }
        }

        return true;
    }

    public Cell[] GetAllCells()
    {
        return m_cells;
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

    public Cell GetCellWithSameType(Cell cell)
    {
        for (int i = 0; i < m_cells.Length; i++)
        {
            if (!m_cells[i].IsEmpty && m_cells[i].IsSameType(cell))
            {
                return m_cells[i];
            }
        }

        return null;
    }

    public int GetIndexCellWithSameType(Cell cell)
    {
        for (int i = 0; i < m_cells.Length; i++)
        {
            if (!m_cells[i].IsEmpty && m_cells[i].IsSameType(cell))
            {
                return i;
            }
        }

        return -1;
    }

    public Cell GetCellAtIndex(int i)
    {
        if (i >= 0 && i < holderSize)
        {
            return m_cells[i];
        }

        return null;
    }

    public void ShiftItemsRightFromIndex(int i)
    {
        for (int x = holderSize - 2; x >= i; x--)
        {
            Cell fromCell = m_cells[x];
            Cell toCell = m_cells[x + 1];

            if (!fromCell.IsEmpty && toCell.IsEmpty)
            {
                Item item = fromCell.Item;
                toCell.Assign(item);
                toCell.ApplyItemMoveToPosition();
                fromCell.Free();
            }
        }
    }

    public void ShiftItemsToFillGap()
    {
        for (int x = 1; x < holderSize; x++)
        {
            Cell fromCell = m_cells[x];
            Cell toCell = m_cells[x - 1];

            while (toCell.BoardX - 1 >= 0 && m_cells[toCell.BoardX - 1].IsEmpty)
            {
                toCell = m_cells[toCell.BoardX - 1];
            }
            if (!fromCell.IsEmpty && toCell.IsEmpty)
            {
                Item item = fromCell.Item;
                toCell.Assign(item);
                toCell.ApplyItemMoveToPosition();
                fromCell.Free();
            }
        }
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
