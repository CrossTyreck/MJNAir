using UnityEngine;
using System.Collections;

public class Directives {

    /// <summary>
    /// The direction this directive is pointing
    /// </summary>
    public Vector3 LookVector { get; set; }

    /// <summary>
    /// The speed of the copter at this directive. 
    /// </summary>
    public float Speed { get; set; }

    /// <summary>
    /// The altitude of the copter at this directive. 
    /// </summary>
    public float Altitude { get; set; }

    /// <summary>
    /// Distance from the last point to this point
    /// </summary>
    public float Distance { get; set; }

    /// <summary>
    /// Keep the amount of score value to add to the current score.
    /// The Distance may be used for now unless we start manipulating
    /// other things in the environment, on the path, to add or 
    /// subtract from the score.
    /// </summary>
    public int ScoreValue { get; set; }

    /// <summary>
    /// Visual representation of the directive.
    /// </summary>
    public GameObject Pyramid { get; set; }

	// Use this for initialization
	void Start () {

        InitVariables();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {


    }

    void InitVariables()
    {
        LookVector = new Vector3(0, 0, 0);
        Speed = 0;
        Altitude = 0;
        Distance = 0;
        ScoreValue = 0;
    }


}
