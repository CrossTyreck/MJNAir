using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WelcomeScreeenPosition : MonoBehaviour {

	public GUITexture front = new GUITexture();
	public GUITexture tr = new GUITexture();
	public GUITexture tl = new GUITexture();
	public GUITexture br = new GUITexture();
	public GUITexture bl = new GUITexture();

	public Texture2D welImage;
	const int val = 600;
	// Use this for initialization
	void Start () {

		front.texture = welImage;
	
		front.pixelInset = new Rect (Screen.width * 0.5f - val/2, Screen.height * 0.5f - val/2, val, val);

		tr.pixelInset = new Rect (Screen.width * 0.5f, Screen.height * 0.5f - val/2, val/2, val/2);
		tl.pixelInset = new Rect (Screen.width * 0.5f - val/2, Screen.height * 0.5f - val/2, val/2, val/2);
		br.pixelInset = new Rect (Screen.width * 0.5f, Screen.height * 0.5f, val/2, val/2);
		bl.pixelInset = new Rect (Screen.width * 0.5f - val/2, Screen.height * 0.5f, val/2, val/2);

	}

	void OnMouseExit()
	{
		front.texture = welImage;
	}
}
