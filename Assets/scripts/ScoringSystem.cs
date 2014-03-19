using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScoringSystem 
{

    public int ParScore { get; set; }
    public int ProjectedScore { get; set; }
    public int CurrentScore { get; set; }
    public int FinalScore { get; set; }
    public List<Obstacle> Obstacles { get; set; }

    // Use this for initialization
    void Start()
    {
       
        ParScore = 0;
        ProjectedScore = 0;
        CurrentScore = 0;
        FinalScore = 0;

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetScoreAtDirective(Directive directive)
    {
        //CurrentScore += directive.ScoreValue;
    }

    public void SetFinalScore()
    {
        FinalScore += CurrentScore;
    }

    public void InitializeScore()
    {
        CurrentScore = 0;
    }

    public override string ToString()
    {
        return CurrentScore.ToString();
    }

    void OnGUI()
    {

        GUI.Box(new Rect(10, 10, 250, 25), "Score: " + CurrentScore.ToString());

    }
}