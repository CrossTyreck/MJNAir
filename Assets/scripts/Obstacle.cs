using UnityEngine;
using System.Collections;

public class Obstacle : MonoBehaviour
{
    /// <summary>
    /// This value should be hardcoded on the obstacles themselves. 
    /// </summary>
    public float EnergyConsumptionMultiplier;
    float currentRotation;
    Vector3 rotateAroundPoint;
    void Start()
    {
        currentRotation = 0f;
        Vector2 v =  Random.insideUnitCircle * 10f;
        rotateAroundPoint = transform.position + new Vector3(v.x, 0f, v.y);
    }
    void Update()
    {
        if (name == "Bee")
        {
            transform.RotateAround(rotateAroundPoint, Vector3.up, Time.deltaTime * 100f);
            transform.forward = Vector3.Cross(rotateAroundPoint - transform.position, transform.up);
        }
    }
}
