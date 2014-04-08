using UnityEngine;
using System.Collections;


/// <summary>
/// Attach to the slider button, not the slider~!
/// </summary>
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
       // transform.position = Camera.main.ScreenToViewportPoint(new Vector3((slider.transform.position.x * Screen.width), Mathf.Clamp(Input.mousePosition.y, Screen.height* slider.transform.position.y, (Screen.height * slider.transform.position.y) + (slider.pixelInset.height * 0.65f)), 1));
        transform.position = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>().ScreenToViewportPoint(new Vector3((slider.transform.position.x * Screen.width), Mathf.Clamp(Input.mousePosition.y, Screen.height * slider.transform.position.y, (Screen.height * slider.transform.position.y) + (slider.pixelInset.height * 0.65f)), 1));
    }

    void Update()
    {

        speed = (Screen.height * transform.position.y) * 0.5f;
        if (GameObject.FindGameObjectWithTag("PathEditingCamera").GetComponent<Camera>().enabled)
        {
           GameObject.Find("SpeedSlider").SetActive(false);
           GameObject.Find("SpeedButton").SetActive(false);
        }
        else
        {
            GameObject.Find("SpeedSlider").SetActive(true);
            GameObject.Find("SpeedButton").SetActive(true);
        }
    }

   
    //public float GetValue()
    //{

    //    return speed = Screen.height * transform.position.y;

    //}
}