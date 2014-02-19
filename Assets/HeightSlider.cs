using UnityEngine;
using System.Collections;

public class HeightSlider : MonoBehaviour
{
    public GUITexture slider;

    public static float height;

    void Start()
    {
        height = 0;

    }
    public void OnMouseDrag()
    {
        transform.position = Camera.main.ScreenToViewportPoint(new Vector3((slider.transform.position.x * Screen.width), Mathf.Clamp(Input.mousePosition.y, Screen.height * slider.transform.position.y, (Screen.height * slider.transform.position.y) + (slider.pixelInset.height * 0.65f)), 1));
    }

    void Update()
    {

        height = Mathf.Clamp((Screen.height * transform.position.y), -20, 20);

    }


    //public float GetValue()
    //{

    //    return speed = Screen.height * transform.position.y;

    //}
}