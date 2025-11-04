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
                StopHints();
                break;
        }
    }


    public void Update()
    {
        if (m_gameOver) return;
        if (IsBusy) return;

        // if (!m_hintIsShown)
        // {
        //     m_timeAfterFill += Time.deltaTime;
        //     if (m_timeAfterFill > m_gameSettings.TimeForHint)
        //     {
        //         m_timeAfterFill = 0f;
        //         ShowHint();
        //     }
        // }

        if (Input.GetMouseButtonDown(0))
        {

            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                Cell clickedCell = hit.collider.GetComponent<Cell>();

                if (clickedCell != null && clickedCell.IsClickable && !clickedCell.IsEmpty)
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

                    if (emptyCell != null)
                    {
                        emptyCell.Assign(item);
                        emptyCell.ApplyItemMoveToPosition();
                        clickedCell.Free();
                    }
                    clickedCell.Free();
                    IsBusy = true;
                    FindMatchesAndCollapse();
                }
            }
        }
    }

    private void FindMatchesAndCollapse()
    {
        m_holderController.CollapseMatchesAndShift();
        IsBusy = false;
    }

    private IEnumerator ShiftDownItemsCoroutine()
    {
        m_board.ShiftDownItems();

        yield return new WaitForSeconds(0.2f);

        // m_board.FillGapsWithNewItems();

        yield return new WaitForSeconds(0.2f);

        // FindMatchesAndCollapse();
    }

    private IEnumerator RefillBoardCoroutine()
    {
        m_board.ExplodeAllItems();

        yield return new WaitForSeconds(0.2f);

        m_board.Fill();

        yield return new WaitForSeconds(0.2f);

        // FindMatchesAndCollapse();
    }

    private IEnumerator ShuffleBoardCoroutine()
    {
        m_board.Shuffle();

        yield return new WaitForSeconds(0.3f);

        // FindMatchesAndCollapse();
    }


    private void SetSortingLayer(Cell cell1, Cell cell2)
    {
        if (cell1.Item != null) cell1.Item.SetSortingLayerHigher();
        if (cell2.Item != null) cell2.Item.SetSortingLayerLower();
    }

    private bool AreItemsNeighbor(Cell cell1, Cell cell2)
    {
        return cell1.IsNeighbour(cell2);
    }

    internal void Clear()
    {
        m_board.Clear();
    }

    private void ShowHint()
    {
        m_hintIsShown = true;
        foreach (var cell in m_potentialMatch)
        {
            cell.AnimateItemForHint();
        }
    }

    private void StopHints()
    {
        m_hintIsShown = false;
        foreach (var cell in m_potentialMatch)
        {
            cell.StopHintAnimation();
        }

        m_potentialMatch.Clear();
    }
}
