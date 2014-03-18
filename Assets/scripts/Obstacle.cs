using UnityEngine;
using System.Collections;

public class Obstacle {

    //How much energy consumption this obstacle uses, from 2- 4 times
    public int EnergyConsumptionMultiplier {get; set;}

    public Obstacle(int multiplier){

        EnergyConsumptionMultiplier = multiplier;
    }
}
