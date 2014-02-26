using UnityEngine;
using System.Collections;

public class ScoringSystem{

    public int ParScore { get; set; }
    public int ProjectedScore { get; set; }
    public int CurrentScore { get; set; }
    public int FinalScore { get; set; } 
    

	// Use this for initialization
	void Start () {

        ParScore = 0;
        ProjectedScore = 0;
        CurrentScore = 0;
        FinalScore = 0;
       
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
