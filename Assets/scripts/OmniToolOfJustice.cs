﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OmniToolOfJustice : MonoBehaviour
{
	#region Variable Definitions
	enum CameraType
	{
		PerspectiveEditing = 0,
		TopDownEditing = 1,
		Copter = 2
	}
	ScoringSystem score; 
	public Camera PerspectiveEditingCam;
	public Camera TopDownEditingCam;
	public Camera CopterCam;
	public GameObject Arrow;
	public LineRenderer Lines;
	int mouseDragDirective = -1;
	int selDirective = -1;
	int curDirective = 0;
	int vertexCount = 0;
	Vector3 pMouse = Vector3.zero;
	List<Directive> directives = new List<Directive>();

	CameraType camtype = CameraType.TopDownEditing;
	float speed;
	int lineres = 10;
	public Terrain t;
	public GameObject copter;
	Vector3 direction;
	Vector3 cross;
	Vector3 target;
	public bool moving;
	float scale;
	Space relativeTo = Space.World;
	int pathPosCount = 0;
	Rect guiRect = new Rect();
	public bool drawingPath = false;
	public GUITexture startButton;
	public GUISkin UISkin;
	public GUIStyle customGUIStyle;
	
	#endregion
	
	
	//
	void Start()
	{
		moving = false;
		drawingPath = true;
		startButton.enabled = false;
		PerspectiveEditingCam.enabled = false;
		speed = 0;
		score = new ScoringSystem();
		score.InitializeScore(); 

		customGUIStyle = new GUIStyle ();
		customGUIStyle.fontSize = 14;
		copter.SetActive(false);
		copter.transform.position = new Vector3 (500, 100, 500);
		directives.Add (new Directive (copter.transform.position, Instantiate (Arrow) as GameObject));
		vertexCount++;
		Lines.SetVertexCount (vertexCount);
		Lines.SetPosition (vertexCount - 1, copter.transform.position);
	}
	
	void Update()
	{
		
		
		if (Input.GetMouseButton(0) && !moving)
		{
			startButton.enabled = false;
			
		}
		if (Input.GetMouseButtonUp(0) && !moving)
		{
			startButton.enabled = true;
		}


		if (moving)
		{
			SetCopterDirection();
			MovingAlong();
		}
		else
		{
			// if the copter has been given a directive to wait, it waits until the timer is up, 
			// then continues with its activities
			if (waitTime > 0.0f) { 
				waitTime -= Time.deltaTime;
				if (waitTime <= 0.0f)
				{
					moving = true;
					waitTime = 0.0f;
				}
			}
		}

		
		CameraChecking();
		MouseControls();
		KeyboardControls();
	}
	
	void OnGUI()
	{
		GUI.skin = UISkin;
		//GUI.Label(new Rect(Screen.width * 0.5f, 20, 250, 50), "Score: " + score.CurrentScore.ToString());
		GUI.Box(new Rect(10, 775, 250, 25), "Copter Position: " + copter.transform.position.ToString());
		GUI.Box(new Rect(10, 800, 250, 25), Vector3.Distance(target, copter.transform.position).ToString());
		GUI.Box(new Rect(10, 825, 250, 25), "Position counter: " + pathPosCount);
		GUI.Box(new Rect(10, 850, 250, 25), "Position counter: " + pathPosCount);
		
		if (GUI.Button(new Rect(Screen.width * 0.9f, 775, 150, 50), "Move Control"))
		{
			moving = !moving;
		}
		
		if (selDirective > 0)
		{
			Directive d = directives[selDirective];
			Vector3 point = d.Position;

			guiRect = new Rect(PerspectiveEditingCam.WorldToScreenPoint(point).x, Screen.height - PerspectiveEditingCam.WorldToScreenPoint(point).y, 200, 100);
			float x = point.x;
			float y = point.y;
			float z = point.z;
			if (GUI.Button(guiRect, 
			               "Location\nX: " + x.ToString() + 
			               "\nY:" + y.ToString() + 
			               "\nZ: " + z.ToString () + 
			               "\nLook Vector:" + d.LookVector.ToString () +
			               "\nArc Type: " + d.Alignment.ToString(), customGUIStyle))
			{
				
			}                
		}
		
		
		
		if (Input.GetMouseButtonDown(0) && startButton.HitTest(Input.mousePosition))
		{
			drawingPath = false;
			moving = true;
			startButton.enabled = false;
			startButton.transform.position = new Vector3(9999, 9999, -100);
			copter.transform.position = directives[0].Points[0];
			curDirective = 0;
			pathPosCount = 0;
			target = directives[curDirective].Points[pathPosCount];

			copter.SetActive(true);
		}
	}
	
	#region CopterMovement
	void SetCopterDirection()
	{
		
		direction = Vector3.Normalize(target - copter.transform.position);
		scale = Vector3.Dot(copter.transform.forward, direction);
		direction = Vector3.Normalize(direction);
		cross = Vector3.Cross(copter.transform.forward, direction);
		
		if (scale > .99f)
		{
			copter.transform.forward = direction;
		}
		else
		{
			copter.transform.Rotate(cross, (0.5f * Time.deltaTime) * Mathf.Rad2Deg, relativeTo);
		}
	}
	float waitTime = 0.0f;
	void MovingAlong()
	{

		if (Vector3.Distance(target, copter.transform.position) > ((speed + SpeedSlider.speed) * Time.deltaTime) && moving)
		{
			copter.transform.position += direction * (speed + SpeedSlider.speed) * Time.deltaTime;
		}
		else
		{
			pathPosCount++;
			if (pathPosCount < directives[curDirective].Points.Count)
			{
				target = directives[curDirective].Points[pathPosCount];
				score.CurrentScore += 10; 
			}
			else 
				QueryDirective();
		}
	}
	// look into the directive that was just reached and act accordingly
	void QueryDirective() {
		curDirective++;
		if (curDirective < directives.Count) {
			pathPosCount = 0;
			speed = directives [curDirective].Speed;
			target = directives[curDirective].Points[pathPosCount];
			waitTime = directives[curDirective].WaitTime;
			if(waitTime > 0.0f)
				moving = false;
		} 
		else {
			moving = false;
			curDirective--;
		}
	}
	#endregion
	
	#region Path drawing
	void TopDownEditMode(Camera cam)
	{
		if (!drawingPath)
			return;
		if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) {
			if (Input.GetMouseButtonDown (0)) {
				if(directives[directives.Count - 1].Position != directives[curDirective].Points[directives[curDirective].Points.Count - 1])
				{
					directives.Add ( new Directive (directives[curDirective].Points[directives[curDirective].Points.Count - 1], Instantiate (Arrow) as GameObject));
					curDirective++;
				}
			}
		}
		else {
			if (Input.GetMouseButton (0)) {
				Ray ray = cam.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit = new RaycastHit ();
				int x1 = (int)(ray.origin.x / lineres);
				int y1 = (int)(ray.origin.z / lineres);
				int x2 = (int)(directives [curDirective].Points [directives [curDirective].Points.Count - 1].x / lineres);
				int y2 = (int)(directives [curDirective].Points [directives [curDirective].Points.Count - 1].z / lineres);
				if (x1 != x2 || y1 != y2) {
					if (t.collider.Raycast (ray, out hit, cam.farClipPlane)) {
						Vector3 p = new Vector3 (hit.point.x, hit.point.y + 100, hit.point.z);
						directives [curDirective].Points.Add (p);
						vertexCount++;
						Lines.SetVertexCount (vertexCount);
						Lines.SetPosition (vertexCount - 1, p);
					}
				}
			}
		}
	}
	
	
	int getIndexOnClick(Camera cam)
	{
		if (directives.Count > 0)
		{
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, cam.farClipPlane, 1 << 9))
			{
				hit.collider.renderer.material.color = Color.black;
				for (int i = 0; i < directives.Count; i++)
					if(hit.collider.gameObject.Equals(directives[i].Pyramid))
						return i;
			}
		}
		return -1;
	}
	
	void SelectDirectiveAndDrag()
	{
		if (Input.GetMouseButtonDown(0)) {
			if (selDirective == -1) {
				int t = getIndexOnClick(PerspectiveEditingCam);
				selDirective = t;
				mouseDragDirective = t;
			}
			else
				if (!guiRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
					selDirective = -1;
		}
		if (mouseDragDirective > -1) {
			Vector3 dif = (Input.mousePosition - pMouse);
			Ray r = PerspectiveEditingCam.ScreenPointToRay(Input.mousePosition);            
			float t = (directives[mouseDragDirective].Position.y - r.origin.y) / r.direction.y;
			int draggedLineIndex = 0;
			for(int i = 0; i < mouseDragDirective; i++)
				draggedLineIndex += directives[i].Points.Count - 1;

			if (Input.GetKey(KeyCode.LeftShift))
				directives[mouseDragDirective].Set(r.GetPoint(t), Lines, directives, mouseDragDirective);
			else
				directives[mouseDragDirective].Set(directives[mouseDragDirective].Position + new Vector3(0, dif.y, 0), Lines, directives, mouseDragDirective);
			//Lines.SetPosition(draggedLineIndex, directives[mouseDragDirective].Position);
			
			if (Input.GetMouseButtonUp(0)) {
				directives[mouseDragDirective].Pyramid.renderer.material.color = new Color(0.4f, 1f, 0.4f);
				mouseDragDirective = -1;
			}
		}
	}
	
	#endregion
	
	#region User Controls
	void KeyboardControls() 
	{
		
	}
	void OnMouseDown()
	{
		
	}
	void OnMouseUp()
	{
		
	}
	void MouseControls()
	{
		Vector3 dMouse = Input.mousePosition - pMouse;
		switch (camtype)
		{
		case CameraType.TopDownEditing:
			TopDownEditMode(TopDownEditingCam);
			if (Input.GetMouseButton(2))
				TopDownEditingCam.transform.position -= (new Vector3(dMouse.x, 0, dMouse.y) * TopDownEditingCam.orthographicSize) * 0.003f;
			TopDownEditingCam.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * 200;
			float scroll = Input.GetAxis("Mouse ScrollWheel");
			if (scroll > 0)
			{
				if (TopDownEditingCam.orthographicSize > 200)
					TopDownEditingCam.orthographicSize -= scroll * 200;
			}
			if (scroll < 0)
				TopDownEditingCam.orthographicSize -= scroll * 200;
			
			break;
		case CameraType.PerspectiveEditing:
			SelectDirectiveAndDrag();
			Vector3 terrainCenter = t.transform.position + new Vector3(t.terrainData.size.x, 0, t.terrainData.size.z) / 2;
			if (Vector3.Distance(PerspectiveEditingCam.transform.position, terrainCenter) > 300)
				PerspectiveEditingCam.transform.position += PerspectiveEditingCam.transform.forward * Input.GetAxis("Mouse ScrollWheel") * 200;
			else if (Input.GetAxis("Mouse ScrollWheel") < 0)
				PerspectiveEditingCam.transform.position += PerspectiveEditingCam.transform.forward * Input.GetAxis("Mouse ScrollWheel") * 200;
			if (Input.GetMouseButton(2))
			{
				Vector3 campos = PerspectiveEditingCam.transform.position;
				Vector3 v = campos - terrainCenter;
				float x = v.x;
				float y = v.z;
				float theta = -dMouse.x * 0.01f;
				float xp = x * Mathf.Cos(theta) - y * Mathf.Sin(theta);
				float yp = x * Mathf.Sin(theta) + y * Mathf.Cos(theta);
				PerspectiveEditingCam.transform.position = new Vector3(terrainCenter.x + xp, Mathf.Clamp(campos.y + dMouse.y, terrainCenter.y + 50f, terrainCenter.y + 500f), terrainCenter.z + yp);
				PerspectiveEditingCam.transform.LookAt(terrainCenter);
			}
			break;
		case CameraType.Copter:
			
			break;
		}
		pMouse = Input.mousePosition;
	}
	void CameraChecking()
	{
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			camtype = (CameraType)(((int)camtype + 1) % 3);
			CameraSwitch();
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			camtype = ((camtype - 1) < 0 ? camtype = CameraType.Copter : camtype--);
			CameraSwitch();
		}
		
	}
	void CameraSwitch()
	{
		switch (camtype)
		{
		case CameraType.PerspectiveEditing:
			TopDownEditingCam.enabled = false;
			CopterCam.enabled = false;
			PerspectiveEditingCam.enabled = true;
			break;
		case CameraType.TopDownEditing:
			TopDownEditingCam.enabled = true;
			CopterCam.enabled = false;
			PerspectiveEditingCam.enabled = false;
			break;
		case CameraType.Copter:
			TopDownEditingCam.enabled = false;
			PerspectiveEditingCam.enabled = false;
			CopterCam.enabled = true;
			break;
		}
	}
	#endregion
}