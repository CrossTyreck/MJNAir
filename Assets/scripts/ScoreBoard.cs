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
    public BoardSquare[,] GameBoard { get; set; }

    // Setup Game Board
    public ScoreBoard(Transform transform)
    {
        int x = (int)(transform.localScale.x * 10);
        int z = (int)(transform.localScale.z * 10);
        Vector2 offset = new Vector2(transform.position.x - x * 0.5f, transform.position.z - z * 0.5f);
        GameBoard = new BoardSquare[x, z];

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < z; j++)
            {
                GameBoard[i, j] = new BoardSquare(offset + new Vector2(i, j), 1);
            }
        }

        GameBoardScore = GameBoard.Length;
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

    /// <summary>
    /// Check which square the quad copter is closest to. 
    /// </summary>
    /// <param name="copter"></param>
    /// <returns></returns>
    public Vector3 CopterLocation(GameObject copter)
    {
        float smallest = int.MaxValue;
        BoardSquare bsReturn = new BoardSquare();

        foreach (BoardSquare square in this.GameBoard)
        {
            float value = Vector2.Distance((Vector2)copter.transform.position, square.Position);
            bsReturn = smallest < value ? square : bsReturn;
        }
        
        return (Vector3)bsReturn.Position;
    }

    /// <summary>
    /// Output all board square in the Gameboard
    /// </summary>
    /// <returns></returns>
    public string ToString()
    {
        string squaresPos = "";

        if (GameBoard.Length > 0)
        {
            foreach (BoardSquare square in GameBoard)
            {
                squaresPos += square.ToString() + "\n";
            }
            return squaresPos.ToString();
        }

        return GameBoard.Length.ToString();
    }
}
