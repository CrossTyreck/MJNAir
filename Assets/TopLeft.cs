﻿using UnityEngine;
using System.Collections;

public class TopLeft : MonoBehaviour {

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
}