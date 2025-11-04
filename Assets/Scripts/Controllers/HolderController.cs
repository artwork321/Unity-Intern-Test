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
}
