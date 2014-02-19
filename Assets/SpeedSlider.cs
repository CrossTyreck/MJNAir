using UnityEngine;
using System.Collections;

public class SpeedSlider : MonoBehaviour
{
    public GUITexture slider;

    public static float speed;

    void Start()
    {
        speed = 0;

    }
    public void OnMouseDrag()
    {
        transform.position = Camera.main.ScreenToViewportPoint(new Vector3((slider.transform.position.x * Screen.width), Mathf.Clamp(Input.mousePosition.y, Screen.height* slider.transform.position.y, (Screen.height * slider.transform.position.y) + (slider.pixelInset.height * 0.65f)), 1));
    }

    void Update()
    {

        speed = Screen.height * transform.position.y;

    }

   
    //public float GetValue()
    //{

    //    return speed = Screen.height * transform.position.y;

    //}
}