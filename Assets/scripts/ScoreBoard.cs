using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// An attempt at making a score on the board that can help to calculate the users 
/// overall points at the end of the level. 
/// </summary>
public class ScoreBoard
{

    public int GameBoardScore { get; set; }
    public List<BoardSquare> GameBoard { get; set; }

    // Setup Game Board
    public ScoreBoard(TerrainData tSize)
    {
        GameBoard = new List<BoardSquare>();
        //for (int i = 0; i <= tSize.size.x; i++)
        //{

        //    for (int j = 0; j <= tSize.size.z; i++)
        //    {

        //        GameBoard.Add(new BoardSquare(new Vector2(i, j), 1));
        //        Debug.Log(GameBoard.Count.ToString());
        //    }
        //}

        GameBoardScore = GameBoard.Count;

    }

    /// <summary>
    /// Sets traversed variable if copter traverses a particular square
    /// </summary>
    /// <param name="copterPos"></param>
    public void CheckSquareTraversal(Vector3 copterPos)
    {
        foreach (BoardSquare square in GameBoard)
        {
            if (square.Position.x == copterPos.x && square.Position.y == copterPos.z)
            {
                square.SetTraversed();
            }
        }

    }

    /// <summary>
    /// Returns the number of points collected from traversed squares
    /// </summary>
    /// <returns></returns>
    public int GetScoreFromTraversed()
    {
        int points = 0;
        foreach (BoardSquare square in GameBoard)
        {
            if (square.Traversed)
            {
                points = square.Point;
            }
        }
        return points;
    }
}
