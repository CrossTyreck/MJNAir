using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// A MonoBehaviour handling the basic player interface with the game, to be used by up to four players.
/// </summary>
public class PlayerController : MonoBehaviour {

	/// <summary>
	/// The game object representing the player. In this case, the Heliquad.
	/// </summary>
    public GameObject PlayerCopter;
	/// <summary>
	/// The game ground level.
	/// </summary>
	public GameObject gameGroundLevel;
	/// <summary>
	/// The particle system designated for use with this code, will draw particles along the directive line.
	/// </summary>
	public ParticleSystem LineParticles;
	/// <summary>
	/// The current set of directives along this player's path.
	/// </summary>
	List<Directive> directives;
	/// <summary>
	/// The current speed of the copter.
	/// </summary>
	float speed;
	/// <summary>
	/// The amount of time until the copter begins moving again, in seconds
	/// </summary>
	float waitTime;
	/// <summary>
	/// Determines if the copter is moving.
	/// </summary>
	bool moving;
	/// <summary>
	/// The next point along this copter's path.
	/// </summary>
	Vector3 target;
	int draggedDirective;
	int selDirective;
	int currentDirective;
	int currentPathPosition;
	Vector2 pTouchPosition;

	float flashtimer;
	string message;
	GUISkin customSkin;

	void Start () 
	{
		directives = new List<Directive>();
		draggedDirective = -1;
		selDirective = -1;
		flashtimer = 0.0f;
		message = "";
		currentDirective = 0;
		currentPathPosition = 0;
		pTouchPosition = Vector3.zero;
		customSkin = Resources.Load<GUISkin> ("CustomGUISkin");
	}

	void Update () 
	{
		foreach(Directive d in directives)
			d.Update(LineParticles);
		if (flashtimer > 0.0f)
			flashtimer -= Time.deltaTime;
		if (waitTime > 0.0f) 
		{
			waitTime -= Time.deltaTime;
			if(waitTime < 0.0f)
			{
				moving = true;
				waitTime = 0.0f;
			}
		}
		if (moving) 
		{
			MovingAlong();
			//goPlanePosition.transform.position = gameGrid.CopterLocation(copter);
		}
	}
	void OnGui() 
	{
		// basic player instructions go here
		if (flashtimer > 0.0f) // for messaging the player involving action or inaction, set flashtimer > 0 and message to desired message
		{
			GUI.Label (new Rect(Screen.width * 0.3f, Screen.height * 0.3f, Screen.width * 0.4f, Screen.height * 0.4f), message);
		}
		if(directives[0].Points.Count < 0.0f)
			GUI.Label (new Rect(Screen.width * 0.3f, Screen.height * 0.15f, Screen.width * 0.4f, Screen.height * 0.15f), 
			           "Draw a path for your copter to follow by moving your finger across the screen");
		else if(directives.Count < 2)
			GUI.Label (new Rect(Screen.width * 0.3f, Screen.height * 0.15f, Screen.width * 0.4f, Screen.height * 0.15f), 
			           "Add additional directives to control your copter by tapping twice");
	}

	public void CopterControl(Camera cam) 
	{

	}
	#region Path Drawing and Editing
	public void LineDrawingControl(Camera cam) 
	{
		if (cam.isOrthoGraphic) 
		{
			TopDownEditMode(cam);
		} 
		else 
		{
			PerspectiveCameraControls(cam);
		}
		pTouchPosition = Input.GetTouch (0).position;
	}
	void TopDownEditMode(Camera cam)
	{

		if (Input.GetTouch(0).tapCount == 2)
		{
			if (directives[directives.Count - 1].Position != directives[currentDirective].Points[directives[currentDirective].Points.Count - 1])
			{
				directives.Add(new Directive(directives[currentDirective].Points[directives[currentDirective].Points.Count - 1], Instantiate(Resources.Load("prefabs/Arrow")) as GameObject)); 
				currentDirective++;
			}
		}
		else
		{
			if (Input.GetTouch(0).phase == TouchPhase.Ended)
			{
				Ray ray = cam.ScreenPointToRay(Input.GetTouch(0).position);
				RaycastHit hit = new RaycastHit();
				if (gameGroundLevel.collider.Raycast(ray, out hit, cam.farClipPlane))
				{
					Vector3 p = new Vector3(hit.point.x, hit.point.y + 6, hit.point.z);
					float d = Vector3.Distance(directives[currentDirective].Points[directives[currentDirective].Points.Count - 1], p);
					if (d > 0.01f)
						directives[currentDirective].AddPoint(p);
				}
			}
		}

	}
	int getIndexOnTouch(Camera cam)
	{
		if (directives.Count > 0)
		{
			Ray ray = cam.ScreenPointToRay(Input.GetTouch(0).position);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, cam.farClipPlane))
			{
				for (int i = 0; i < directives.Count; i++)
					if (hit.collider.gameObject.Equals(directives[i].Pyramid))
						return i;
			}
		}
		return -1;
	}
	float lastPinchDistance = 0.0f;
	void SelectDirectiveAndDrag(Camera cam)
	{
		if (Input.GetTouch(0).phase == TouchPhase.Began)
		{
			if (selDirective == -1)
			{
				int t = getIndexOnTouch(cam);
				
				selDirective = t;
				draggedDirective = t;
				if (t > -1)
					directives[t].Highlight = true;
			}
		}
		if (draggedDirective > -1)
		{
			Vector3 dif = (Input.GetTouch(0).position - pTouchPosition);
			Ray r = cam.ScreenPointToRay(Input.GetTouch(0).position);
			float t = (directives[draggedDirective].Position.y - r.origin.y) / r.direction.y;
			int draggedLineIndex = 0;
			for (int i = 0; i < draggedDirective; i++)
				draggedLineIndex += directives[i].Points.Count - 1;
			
			if (Input.touchCount == 2)
				directives[draggedDirective].Set(r.GetPoint(t), draggedDirective, directives);
			else
				directives[draggedDirective].Set(directives[draggedDirective].Position + new Vector3(0, dif.y * 0.03f, 0), draggedDirective, directives);
			
			if (Input.GetTouch(0).phase == TouchPhase.Ended)
			{
				draggedDirective = -1;
			}
		}
	}
	void PerspectiveCameraControls(Camera cam)
	{
		Vector3 terrainCenter = gameGroundLevel.transform.position + new Vector3(gameGroundLevel.transform.localScale.x, 6, gameGroundLevel.transform.localScale.z) / 2;

		// if the player has two fingers on the screen and they are simply moving around, scroll the camera
		// if the player starts to move their fingers apart or together, zoom the camera
		if (Input.touchCount == 2) {
			float pinchDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
			if(pinchDistance > 100f) { // THIS NUMBER IS UNTESTED
			float deltaPinchDistance = lastPinchDistance - pinchDistance;
			if (Vector3.Distance (cam.transform.position, terrainCenter) > 5)
				cam.transform.position += cam.transform.forward * deltaPinchDistance;
			else if (deltaPinchDistance > 0)
				cam.transform.position += cam.transform.forward * deltaPinchDistance;
			}
			else {
				Vector3 campos = cam.transform.position;
				Vector3 v = campos - terrainCenter;
				float x = v.x;
				float y = v.z;
				float theta = -Input.GetTouch(0).deltaPosition.x * 0.003f;
				float xp = x * Mathf.Cos (theta) - y * Mathf.Sin (theta);
				float yp = x * Mathf.Sin (theta) + y * Mathf.Cos (theta);
				cam.transform.position = new Vector3 (terrainCenter.x + xp, Mathf.Clamp (campos.y + Input.GetTouch(0).deltaPosition.y * 0.01f, terrainCenter.y - 2f, terrainCenter.y + 15f), terrainCenter.z + yp);
				cam.transform.LookAt (terrainCenter);
			}
			lastPinchDistance = pinchDistance;
		}
	}
	void EditDirective(Camera cam)
	{
		Directive d = directives[selDirective];
		Vector3 point = d.Position;
		d.Pyramid.renderer.material.color = Color.white;
		Rect guiRect = new Rect(cam.WorldToScreenPoint(point).x, Screen.height - cam.WorldToScreenPoint(point).y, 320, 220);
		GUI.Window(0, guiRect, DirectiveData, "Directive Data");
		if (Input.GetTouch(0).phase == TouchPhase.Began)
			if (!guiRect.Contains(new Vector2(Input.GetTouch(0).position.x, Screen.height - Input.GetTouch(0).position.y)))
		{

			selDirective = -1;
			d.Highlight = false;
		}
	}
	
	void DirectiveData(int id)
	{
		if (selDirective > -1)
		{
			Directive d = directives[selDirective];
			
			if (GUI.Button(new Rect(5, 25, 310, 20), "Pos X:" + d.Position.x.ToString("0.0") + " Y:" + d.Position.y.ToString("0.0") + " Z:" + d.Position.z.ToString("0.0")))
			{
				draggedDirective = selDirective;
			}
			else if (GUI.Button(new Rect(5, 85, 200, 20), "Arc Type: " + d.Alignment.ToString()))
			{
				d.Alignment = (ArcAlignment)(((int)d.Alignment + 1) % 7);
			}
			else if (GUI.Button(new Rect(225, 75, 90, 20), "CHANGE"))
			{
				d.Alignment = (ArcAlignment)(((int)d.Alignment + 1) % 7);
			}
			else if (GUI.Button(new Rect(225, 95, 90, 20), "ALIGN"))
			{
				AlignAllDirectives();
			}
			Vector2 deltaTouchPosition = Input.GetTouch(0).position - pTouchPosition;
			Rect lookRect = new Rect(5, 50, 310, 20);

			/*
			if (movingLook)
			{
				Vector3 lv = Quaternion.Euler(dMouse.x, dMouse.y, 0) * d.LookVector;
				print(lv);
				d.LookVector = lv;
				if (!Input.GetMouseButton(0))
					movingLook = false;
			}
			if (Input.GetMouseButton(0))
				movingLook = true;
			if (GUI.Button(lookRect, "Look X:" + d.LookVector.x.ToString("0.0") + " Y:" + d.LookVector.y.ToString("0.0") + " Z:" + d.LookVector.z.ToString("0.0")))
			{
				movingLook = true;
			}
			*/
			// THE SPEED SLIDER
			GUI.skin = customSkin;
			GUI.Label(new Rect(30, 118, 250, 30), "Line Length: " + d.Distance.ToString("0.00"));
			d.Speed = GUI.HorizontalSlider(new Rect(10, 145, 220, 30), d.Speed, 0.0f, 5.0f);
			GUI.Label(new Rect(70, 142, 100, 30), "Speed");
			GUI.Label(new Rect(240, 142, 70, 30), d.Speed.ToString("0.00"));
			// THE WAIT TIME SLIDER
			d.Speed = GUI.HorizontalSlider(new Rect(10, 170, 220, 30), d.WaitTime, 0.0f, 20.0f);
			GUI.Label(new Rect(70, 170, 100, 30), "Wait Time");
			GUI.Label(new Rect(240, 170, 70, 30), d.WaitTime.ToString("0.00") + "s");
			GUI.skin.button.fontSize = 18;
			GUI.Label(new Rect(30, 195, 250, 28), "# data points: " + d.Points.Count.ToString());       
		}
	}
	#endregion
	void MovingAlong()
	{
		if (Vector3.Distance(target, PlayerCopter.transform.position) > ((speed + SpeedSlider.speed) * Time.deltaTime) && moving)
		{
			Vector3 direction = target = PlayerCopter.transform.position;
			PlayerCopter.transform.position += direction * (speed + SpeedSlider.speed) * Time.deltaTime;
		}
		else
		{
			currentPathPosition++;
			if (currentPathPosition < directives[currentDirective].Points.Count)
				target = directives[currentDirective].Points[currentPathPosition];
			else
				QueryDirective();
		}
	}
	void AlignAllDirectives()
	{
		for (int i = 0; i < directives.Count; i++)
			directives[i].Align(directives, i);
	}
	void QueryDirective()
	{
		currentDirective++;
		if (currentDirective < directives.Count)
		{
			currentPathPosition = 0;
			speed = directives[currentDirective].Speed;
			PlayerCopter.transform.forward = directives[currentDirective].LookVector;
			target = directives[currentDirective].Points[currentPathPosition];
			waitTime = directives[currentDirective].WaitTime;
			if (waitTime > 0.0f)
				moving = false;
		}
		else
		{
			moving = false;
			currentDirective--;
		}
	}
}
