using System.Collections;
using UnityEngine;


/// <summary>
/// An object used to track the score for the player based on copter travel. 
/// </summary>
public class BoardSquare
{

    /// <summary>
    /// The Position of this square based on the terrain size
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// How many points this square is worth.
    /// </summary>
    public int Point { get; set; }

    /// <summary>
    /// Did the copter traverse this square, used in scoring
    /// </summary>
    public bool Traversed { get; set; }

    /// <summary>
    /// How much energy this square consumes for the copter
    /// </summary>
    public int EnergyUsed
    {
		get{ return EnergyUsed; }
        set
        {
            if (!containsObstacle)
            {
                EnergyUsed = 1;
            }
            else
            {
                EnergyUsed = 1 * Obstacle.EnergyConsumptionMultiplier;
            }
        }
    }

    /// <summary>
    /// The Obstacle attached to this Square
    /// </summary>
    public Obstacle Obstacle { get; set; }

    public bool containsObstacle = true;

    public BoardSquare(Vector2 pos, int point, Obstacle Obstacle = null)
    {
        if (Obstacle == null)
            containsObstacle = false;
        Traversed = false;
        Position = pos;
        Point = point;

    }

    //Ridiculous but I am rushing this code crunch session 
    public void SetTraversed()
    {
        Traversed = true;
    }

}
