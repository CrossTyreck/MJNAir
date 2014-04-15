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
    /// the arrow used for the directive
    /// </summary>
    public GameObject Arrow;
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
	public bool moving;
	/// <summary>
	/// The next point along this copter's path.
	/// </summary>
	Vector3 target;
	int draggedDirective;
	int selDirective;
	int currentDirective;
	int currentPathPosition;
	Vector2 pTouchPosition;
    Vector3 pMouse;

	public static float FlashTimer;
	public static string Message;
	public GUISkin customSkin;

	void Start () 
	{
		directives = new List<Directive>();
		draggedDirective = -1;
		selDirective = -1;
		FlashTimer = 0.0f;
		Message = "";
		currentDirective = 0;
		currentPathPosition = 0;
		pTouchPosition = Vector3.zero;
        pMouse = Vector3.zero;
        directives.Add(new Directive(PlayerCopter.transform.position, Arrow));
	}

	void Update () 
	{
		foreach(Directive d in directives)
			d.Update(LineParticles);
		if (FlashTimer > 0.0f)
			FlashTimer -= Time.deltaTime;
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
	void OnGUI() 
	{
        GUI.skin = customSkin;
		// basic player instructions go here
		if (FlashTimer > 0.0f) // for messaging the player involving action or inaction, set flashtimer > 0 and message to desired message
		{
			GUI.Label (new Rect(Screen.width * 0.1f, Screen.height * 0.4f, Screen.width * 0.8f, Screen.height * 0.2f), Message);
		}
		if(directives[0].Points.Count < 2)
			GUI.Label (new Rect(Screen.width * 0.1f, Screen.height * 0.15f, Screen.width * 0.8f, Screen.height * 0.15f), 
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
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.touchCount > 0)
            {
                if (cam.isOrthoGraphic)
                    TopDownEditMode(cam);
                else
                    SelectDirectiveAndDrag(cam);
                pTouchPosition = Input.GetTouch(0).position;
            }
        }
        else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            if (cam.isOrthoGraphic)
                TopDownMouseEdit(cam);
            else
                SelectDirectiveAndDragWithMouse(cam);
            pMouse = Input.mousePosition;
        }
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
            else if(Input.touchCount == 1)
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
    void TopDownMouseEdit(Camera cam)
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (directives[directives.Count - 1].Position != directives[currentDirective].Points[directives[currentDirective].Points.Count - 1])
                {
                    directives.Add(new Directive(directives[currentDirective].Points[directives[currentDirective].Points.Count - 1], Instantiate(Arrow) as GameObject));
                    currentDirective++;
                }
            }
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
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
    int getIndexOnClick(Camera cam)
    {
        if (directives.Count > 0)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
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
    void SelectDirectiveAndDragWithMouse(Camera cam)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (selDirective == -1)
            {
                int t = getIndexOnClick(cam);

                selDirective = t;
                draggedDirective = t;
                if (t > -1)
                    directives[t].Highlight = true;
            }
        }
        if (draggedDirective > -1)
        {
            Vector3 dif = (Input.mousePosition - pMouse);
            Ray r = cam.ScreenPointToRay(Input.mousePosition);
            float t = (directives[draggedDirective].Position.y - r.origin.y) / r.direction.y;
            int draggedLineIndex = 0;
            for (int i = 0; i < draggedDirective; i++)
                draggedLineIndex += directives[i].Points.Count - 1;

            if (Input.GetKey(KeyCode.LeftShift))
                directives[draggedDirective].Set(r.GetPoint(t), draggedDirective, directives);
            else
                directives[draggedDirective].Set(directives[draggedDirective].Position + new Vector3(0, dif.y * 0.03f, 0), draggedDirective, directives);

            if (Input.GetMouseButtonUp(0))
                draggedDirective = -1;
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
    void EditDirectiveWithMouse(Camera cam)
	{
		Directive d = directives[selDirective];
		Vector3 point = d.Position;
		d.Pyramid.renderer.material.color = Color.white;
		Rect guiRect = new Rect(cam.WorldToScreenPoint(point).x, Screen.height - cam.WorldToScreenPoint(point).y, 320, 220);
		GUI.Window(0, guiRect, DirectiveData2, "Directive Data");
		if (Input.GetMouseButtonDown(0))
			if (!guiRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
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
    void DirectiveData2(int id)
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
			//Vector2 deltaTouchPosition = Input.mousePosition - pMouse;
			//Rect lookRect = new Rect(5, 50, 310, 20);

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
    public static void FlashMessage(string m, float t)
    {
        Message = m;
        FlashTimer = t;
    }
}
