﻿using UnityEngine;
using System.Collections;

public class TopRight : MonoBehaviour {
	
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
        //Credits
        Application.LoadLevel(7);
    }
}
