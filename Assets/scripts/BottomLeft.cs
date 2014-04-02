using UnityEngine;
using System.Collections;

public class BottomLeft : MonoBehaviour {
	
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
        //Hard coded based on the levels I dropped in the build settings
		Application.LoadLevel(1);
	}
}
