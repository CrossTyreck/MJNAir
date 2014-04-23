using UnityEngine;
using System.Collections;

public class BottomRight : MonoBehaviour {
	
	public GUITexture parent;
	public Texture2D change;
	public Texture2D main;
	
	void OnMouseEnter()
	{
		parent.texture = change;
	}
	
	void OnMouseExit()
	{
		parent.texture = main;
	}

    void OnMouseDown()
    {
        //Controls
        Application.LoadLevel(6);
    }
}
