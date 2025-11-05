using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public event Action OnMoveEvent = delegate { };

    public bool IsBusy { get; private set; }

    private Board m_board;

    private GameManager m_gameManager;

    private HolderController m_holderController;

    private bool m_isDragging;

    private Camera m_cam;

    private Collider2D m_hitCollider;

    private GameSettings m_gameSettings;

    private List<Cell> m_potentialMatch;

    private float m_timeAfterFill;

    private bool m_hintIsShown;

    private bool m_gameOver;

    public void StartGame(GameManager gameManager, HolderController holderController, GameSettings gameSettings)
    {
        m_gameManager = gameManager;

        m_holderController = holderController;

        m_gameSettings = gameSettings;

        m_gameManager.StateChangedAction += OnGameStateChange;

        m_cam = Camera.main;

        m_board = new Board(this.transform, gameSettings);

        Fill();
    }

    private void Fill()
    {
        // m_board.Fill();
        m_board.FillDivisibleBy3();
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                IsBusy = false;
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                break;
            case GameManager.eStateGame.GAME_OVER:
                m_gameOver = true;
                // StopHints();
                break;
        }
    }


    public void Update()
    {
        if (m_gameOver) return;
        if (IsBusy) return;

        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {

                Cell clickedCell = hit.collider.GetComponent<Cell>();

                if (clickedCell != null && clickedCell.IsClickable && !clickedCell.IsEmpty)
                {
                    // Move cell from Board to Holder
                    if (clickedCell.InBoard)
                    {
                        AddItemFromBoardToHolder(clickedCell);
                        IsBusy = true;
                        FindMatchesAndCollapseHolder();
                    }
                    // Move cell from Holder to Board
                    else
                    {
                        AddItemFromHolderToBoard(clickedCell);
                    }
                }
            }
        }
    }

    public bool IsEmpty()
    {
        return m_board.IsEmpty();
    }

    private void FindMatchesAndCollapseHolder()
    {
        OnMoveEvent();
        m_holderController.CollapseMatchesAndShift();
        IsBusy = false;
    }

    public IEnumerator AutoWin()
    {
        IsBusy = true;

        // Count number of each item type currently in the holder
        var holderCells = m_holderController.GetAllItemCells();
        var types = new Dictionary<NormalItem.eNormalType, int>();

        foreach (var cell in holderCells)
        {
            var normalItem = cell.Item as NormalItem;
            if (normalItem == null) continue;

            if (!types.ContainsKey(normalItem.ItemType))
                types[normalItem.ItemType] = 0;

            types[normalItem.ItemType]++;
        }

        // Fill holder to make sets of 3 for existing types
        foreach (var kvp in types.ToList())
        {
            var type = kvp.Key;
            int needed = m_board.MatchMin - kvp.Value;
            if (needed <= 0) continue;

            var matchedCells = m_board.PickMatchedCells(type, needed);
            foreach (var cell in matchedCells)
            {
                yield return new WaitForSeconds(0.5f);
                UnityEngine.Debug.Log($"AutoWin: Adding item of type {(cell.Item as NormalItem).ItemType}");
                AddItemFromBoardToHolder(cell);
                FindMatchesAndCollapseHolder();
                IsBusy = true;
            }
        }

        // Fill remaining holder slots with any available matches from the board
        var remainingCellsByType = m_board.DictOfCellsByType();

        foreach (var kvp in remainingCellsByType.ToList())
        {
            var matchedCells = kvp.Value;
            foreach (var cell in matchedCells)
            {
                yield return new WaitForSeconds(0.5f);
                UnityEngine.Debug.Log($"AutoWin: Adding item of type {(cell.Item as NormalItem).ItemType}");
                AddItemFromBoardToHolder(cell);
                FindMatchesAndCollapseHolder();
                IsBusy = true;
            }
        }

        IsBusy = false;
        OnMoveEvent();
    }


    public IEnumerator AutoLose()
    {
        IsBusy = true;

        // Count number of each item type currently in holder
        var holderCells = m_holderController.GetAllItemCells();
        var unmatchedTypes = new Dictionary<NormalItem.eNormalType, int>();

        foreach (var cell in holderCells)
        {
            var normalItem = cell.Item as NormalItem;
            if (normalItem == null) continue;

            if (!unmatchedTypes.ContainsKey(normalItem.ItemType))
                unmatchedTypes[normalItem.ItemType] = 0;

            unmatchedTypes[normalItem.ItemType]++;
        }

        // Fill remaining slots with items that do NOT form matches
        int remainingSlots = m_gameSettings.HolderSize - holderCells.Count;
        var selectedCells = m_board.PickUnmatchedCells(unmatchedTypes, remainingSlots);

        foreach (var cell in selectedCells)
        {
            yield return new WaitForSeconds(0.5f);
            UnityEngine.Debug.Log($"AutoLose: Adding item of type {(cell.Item as NormalItem)?.ItemType}");
            AddItemFromBoardToHolder(cell);
        }

        IsBusy = false;
        OnMoveEvent();
    }


    public void AddItemFromBoardToHolder(Cell clickedCell)
    {
        Item item = clickedCell.Item;
        int i_cell = m_holderController.GetIndexCellWithSameType(clickedCell);
        Cell emptyCell = m_holderController.GetEmptyCell();

        // Add next to the same type in holder
        if (i_cell != -1)
        {
            Cell sameTypeCell = m_holderController.GetCellAtIndex(i_cell);
            m_holderController.ShiftItemsRightFromIndex(i_cell);
            emptyCell = sameTypeCell;
        }

        if (emptyCell != null && emptyCell.IsEmpty)
        {
            emptyCell.Assign(item);
            emptyCell.ApplyItemMoveToPosition();
            clickedCell.Free();
        }

        if (m_gameManager.Mode == GameManager.eLevelMode.CLICK)
        {
            emptyCell.IsClickable = false;
        }
    }

    public void AddItemFromHolderToBoard(Cell clickedCell)
    {
        Item item = clickedCell.Item;
        Cell emptyCell = item.InitialCell;

        emptyCell.Assign(item);
        emptyCell.ApplyItemMoveToPosition();
        clickedCell.Free();
    }

    // private IEnumerator ShiftDownItemsCoroutine()
    // {
    //     m_board.ShiftDownItems();

    //     yield return new WaitForSeconds(0.2f);

    //     // m_board.FillGapsWithNewItems();

    //     yield return new WaitForSeconds(0.2f);

    //     // FindMatchesAndCollapse();
    // }

    // private IEnumerator RefillBoardCoroutine()
    // {
    //     m_board.ExplodeAllItems();

    //     yield return new WaitForSeconds(0.2f);

    //     m_board.Fill();

    //     yield return new WaitForSeconds(0.2f);

    //     // FindMatchesAndCollapse();
    // }

    // private IEnumerator ShuffleBoardCoroutine()
    // {
    //     m_board.Shuffle();

    //     yield return new WaitForSeconds(0.3f);

    //     // FindMatchesAndCollapse();
    // }


    // private void SetSortingLayer(Cell cell1, Cell cell2)
    // {
    //     if (cell1.Item != null) cell1.Item.SetSortingLayerHigher();
    //     if (cell2.Item != null) cell2.Item.SetSortingLayerLower();
    // }

    // private bool AreItemsNeighbor(Cell cell1, Cell cell2)
    // {
    //     return cell1.IsNeighbour(cell2);
    // }

    internal void Clear()
    {
        m_board.Clear();
    }

    // private void ShowHint()
    // {
    //     m_hintIsShown = true;
    //     foreach (var cell in m_potentialMatch)
    //     {
    //         cell.AnimateItemForHint();
    //     }
    // }

    // private void StopHints()
    // {
    //     m_hintIsShown = false;
    //     foreach (var cell in m_potentialMatch)
    //     {
    //         cell.StopHintAnimation();
    //     }

    //     m_potentialMatch.Clear();
    // }
}
