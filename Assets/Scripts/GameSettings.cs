using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : ScriptableObject
{
    public int BoardSizeX = 6;

    public int BoardSizeY = 4;

    public int HolderSize = 5;

    public int MatchesMin = 3;

    public int LevelMoves = 16;

    public float LevelTime = 60f;

    public string LevelClick = "Free Play";

    public float TimeForHint = 5f;
}
