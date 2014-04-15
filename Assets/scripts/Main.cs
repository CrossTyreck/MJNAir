using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Main : MonoBehaviour
{
    #region Variable Definitions
    enum CameraType
    {
        PerspectiveEditing = 0,
        TopDownEditing = 1,
        Copter = 2,
        DualCam = 3
    }

    public PlayerController QuadCopter1;
    ScoringSystem score;
    ScoreBoard gameGrid;
    public GameObject gameGroundLevel;
    public Camera PerspectiveEditingCam;
    public Camera TopDownEditingCam;
    public Camera CopterCam;
    public GameObject Arrow;
	public ParticleSystem arrowPS;
    public Texture topDownCameraUI;
    public Texture topDownButtonsBG;
    public Texture goButton;
    public Texture stopButton;
    int mouseDragDirective = -1;
    int selDirective = -1;
    int curDirective = 0;
    Vector3 pMouse = Vector3.zero;
    List<Directive> directives = new List<Directive>();
    float cameraX = 0;
    float cameraY = 0;
    CameraType camtype = CameraType.TopDownEditing;
    float speed;
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

    //Vertical Slider objects
    public GUIStyle speedButton;
    public GUIStyle speedSlider;
    public float sliderValue;

    public bool endingCondition = false;
    public Transform gameBoard;
    public GameObject goPlanePosition;
    public GUISkin GUISkin;
    #endregion


    //
    void Start()
    {
        gameGrid = new ScoreBoard(gameGroundLevel.transform);
        moving = false;
        drawingPath = true;
        startButton.enabled = false;
        startButton.transform.position = new Vector3(0.5f, 0.5f, 1);
        PerspectiveEditingCam.enabled = false;
        speed = 0;
        score = new ScoringSystem();
        score.InitializeScore();
        customGUIStyle = new GUIStyle();
        customGUIStyle.fontSize = 14;
        //QuadCopter1.gameObject.SetActive(false);
        directives.Add(new Directive(QuadCopter1.transform.position, Instantiate(Arrow) as GameObject));
        arrowPS.Play();
    }

    void Update()
    {
        foreach(Directive d in directives)
            d.Update(arrowPS);

        if (Input.GetMouseButton(0) && !moving)
        {
            startButton.enabled = false;

        }
        if (Input.GetMouseButtonUp(0) && !moving)
        {
            startButton.enabled = true;
        }


        //if (moving)
        //{
        //    SetCopterDirection();
        //    MovingAlong();
        //    goPlanePosition.transform.position = gameGrid.CopterLocation(QuadCopter1.gameObject);

        //}
        //else
        //{
        //    // if the copter has been given a directive to wait, it waits until the timer is up, 
        //    // then continues with its activities
        //    if (waitTime > 0.0f)
        //    {
        //        waitTime -= Time.deltaTime;
        //        if (waitTime <= 0.0f)
        //        {
        //            moving = true;
        //            waitTime = 0.0f;
        //        }
        //    }
        //}

        if (endingCondition)
        {
            score.FinalScore += gameGrid.GameBoardScore + gameGrid.GetScoreFromTraversed();
        }

        CameraChecking();
        if(Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) 
            MouseCameraControls();
        else if (Application.platform == RuntimePlatform.Android)
            TouchCameraControls();
        
    }
    void MouseCameraControls()
    {
        Vector3 dMouse = Input.mousePosition - pMouse;
        switch (camtype)
        {
            case CameraType.PerspectiveEditing:
                Vector3 terrainCenter = gameGroundLevel.transform.position + new Vector3(gameGroundLevel.transform.localScale.x, 6, gameGroundLevel.transform.localScale.z) / 2;
                if (Vector3.Distance(PerspectiveEditingCam.transform.position, terrainCenter) > 5)
                    PerspectiveEditingCam.transform.position += PerspectiveEditingCam.transform.forward * Input.GetAxis("Mouse ScrollWheel");
                else if (Input.GetAxis("Mouse ScrollWheel") < 0)
                    PerspectiveEditingCam.transform.position += PerspectiveEditingCam.transform.forward * Input.GetAxis("Mouse ScrollWheel");
                if (Input.GetMouseButton(2))
                {
                    Vector3 campos = PerspectiveEditingCam.transform.position;
                    Vector3 v = campos - terrainCenter;
                    float x = v.x;
                    float y = v.z;
                    float theta = -(Input.mousePosition.x - pMouse.x) * 0.003f;
                    float xp = x * Mathf.Cos(theta) - y * Mathf.Sin(theta);
                    float yp = x * Mathf.Sin(theta) + y * Mathf.Cos(theta);
                    PerspectiveEditingCam.transform.position = new Vector3(terrainCenter.x + xp, Mathf.Clamp(campos.y + (Input.mousePosition.y - pMouse.y) * 0.01f, terrainCenter.y - 2f, terrainCenter.y + 15f), terrainCenter.z + yp);
                    PerspectiveEditingCam.transform.LookAt(terrainCenter);
                }
                break;
            case CameraType.TopDownEditing:
                if (Input.GetMouseButton(2))
                    TopDownEditingCam.transform.position -= (new Vector3(dMouse.x, 0, dMouse.y) * TopDownEditingCam.orthographicSize) * 0.003f;
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll > 0)
                {
                    if (TopDownEditingCam.orthographicSize > 5)
                        TopDownEditingCam.orthographicSize -= scroll;
                }
                if (scroll < 0)
                   TopDownEditingCam.orthographicSize -= scroll * 3.0f;
                break;
        }
        pMouse = Input.mousePosition;
    }
    float lastPinchDistance = 0.0f;
    void TouchCameraControls()
    {
        switch (camtype)
        {
            case CameraType.PerspectiveEditing:
                Vector3 terrainCenter = gameGroundLevel.transform.position + new Vector3(gameGroundLevel.transform.localScale.x, 6, gameGroundLevel.transform.localScale.z) / 2;
                // if the player has two fingers on the screen and they are simply moving around, scroll the camera
                // if the player starts to move their fingers apart or together, zoom the camera
                if (Input.touchCount == 2)
                {
                    float pinchDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                    if (pinchDistance > 50f)
                    { // THIS NUMBER IS UNTESTED
                        float deltaPinchDistance = lastPinchDistance - pinchDistance;
                        if (Vector3.Distance(PerspectiveEditingCam.transform.position, terrainCenter) > 5)
                            PerspectiveEditingCam.transform.position += PerspectiveEditingCam.transform.forward * deltaPinchDistance;
                        else if (deltaPinchDistance > 0)
                            PerspectiveEditingCam.transform.position += PerspectiveEditingCam.transform.forward * deltaPinchDistance;
                    }
                    else
                    {
                        Vector3 campos = PerspectiveEditingCam.transform.position;
                        Vector3 v = campos - terrainCenter;
                        float x = v.x;
                        float y = v.z;
                        float theta = -Input.GetTouch(0).deltaPosition.x * 0.003f;
                        float xp = x * Mathf.Cos(theta) - y * Mathf.Sin(theta);
                        float yp = x * Mathf.Sin(theta) + y * Mathf.Cos(theta);
                        PerspectiveEditingCam.transform.position = new Vector3(terrainCenter.x + xp, Mathf.Clamp(campos.y + Input.GetTouch(0).deltaPosition.y * 0.01f, terrainCenter.y - 2f, terrainCenter.y + 15f), terrainCenter.z + yp);
                        PerspectiveEditingCam.transform.LookAt(terrainCenter);
                    }
                    lastPinchDistance = pinchDistance;
                }
                break;
            case CameraType.TopDownEditing:
                if (Input.touchCount == 2)
                {
                    float pinchDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                    TopDownEditingCam.transform.position -= (new Vector3(Input.GetTouch(0).deltaPosition.x, 0, Input.GetTouch(0).deltaPosition.y) * TopDownEditingCam.orthographicSize) * 0.003f;
                    float deltaPinchDistance = lastPinchDistance - pinchDistance;
                    if (deltaPinchDistance > 0)
                    {
                        if (TopDownEditingCam.orthographicSize > 5)
                            TopDownEditingCam.orthographicSize -= deltaPinchDistance;
                    }
                    if (deltaPinchDistance < 0)
                        TopDownEditingCam.orthographicSize -= deltaPinchDistance * 3.0f;

                    lastPinchDistance = pinchDistance;
                }
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                break;
        }
        
    }
    void OnGUI()
    {
        MouseControls();
        KeyboardControls();

        //Rect btnUpRect = new Rect(Screen.width * 0.9f, 65, 75, 50);
        //GUI.skin = UISkin;

        //if (TopDownEditingCam.enabled && !CopterCam.enabled)
        //{
        //    GUI.BeginGroup(new Rect(Screen.width * 0.025f, Screen.height * 0.042f, Screen.width * 0.2f, Screen.height * 0.9f));

        //    GUI.Box(new Rect(0, 0, 150, 740), topDownButtonsBG, GUI.skin.GetStyle("Label"));
        //    for (int i = 0; i < gameGrid.GameBoard.Length - 1; i++)
        //    {
        //        for (int j = 0; j < gameGrid.GameBoard.Length - 1; j++) { }
        //        // GUI.Label(new Rect(Screen.width * 0.01f, Screen.height * 0.1f + (j * 60), 250, 200), "Squares: " + gameGrid.GameBoard[i, j].Position);
        //    }

        //    sliderValue = GUI.VerticalSlider(new Rect(Screen.width * 0.025f, Screen.height * 0.6f, 75, 250), sliderValue, 10.0f, 0.0f, speedSlider, speedButton);

        //    if (moving)
        //    {
        //        if (GUI.Button(new Rect(-(Screen.width * 0.078f), 0, 350, 100), stopButton, GUI.skin.GetStyle("Label")))
        //        {
        //            moving = false;
        //        }
        //    }
        //    else
        //    {
        //        if (GUI.Button(new Rect(-(Screen.width * 0.078f), 0, 350, 100), goButton, GUI.skin.GetStyle("Label")))
        //        {
        //            QuadCopter1.moving = true;
        //        }
        //    }
        //    GUI.EndGroup();

        //    if (GUI.Button(new Rect(Screen.width * 0.9f, Screen.height * 0.05f, 75, 50), "EXIT"))
        //    {
        //        Application.Quit();
        //    }
        //}
        //if (Input.GetMouseButtonDown(0) && startButton.HitTest(Input.mousePosition))
        //{
        //    drawingPath = false;
        //    moving = true;
        //    startButton.enabled = false;
        //    startButton.transform.position = new Vector3(9999, 9999, -100);
        //    QuadCopter1.transform.position = directives[0].Points[0];
        //    curDirective = 0;
        //    pathPosCount = 0;
        //    target = directives[curDirective].Points[pathPosCount];
        //    QuadCopter1.gameObject.SetActive(true);
        //}

    }

    #region CopterMovement
    void SetCopterDirection()
    {

        direction = Vector3.Normalize(target - QuadCopter1.transform.position);
        scale = Vector3.Dot(QuadCopter1.transform.forward, direction);
        direction = Vector3.Normalize(direction);
        cross = Vector3.Cross(QuadCopter1.transform.forward, direction);

        if (scale > .999f)
        {
            QuadCopter1.transform.forward = direction;
        }
        else
        {
            QuadCopter1.transform.Rotate(cross, (0.5f * Time.deltaTime) * Mathf.Rad2Deg, relativeTo);
        }
    }
    float waitTime = 0.0f;
    void MovingAlong()
    {

        if (Vector3.Distance(target, QuadCopter1.transform.position) > ((speed + sliderValue) * Time.deltaTime) && moving)
        {
            QuadCopter1.transform.position += direction * (speed + sliderValue) * Time.deltaTime;
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
    void QueryDirective()
    {
        curDirective++;
        if (curDirective < directives.Count)
        {
            pathPosCount = 0;
            speed = directives[curDirective].Speed;
            target = directives[curDirective].Points[pathPosCount];
            waitTime = directives[curDirective].WaitTime;
            if (waitTime > 0.0f)
                moving = false;
        }
        else
        {
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
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (directives[directives.Count - 1].Position != directives[curDirective].Points[directives[curDirective].Points.Count - 1])
                {
                    directives.Add(new Directive(directives[curDirective].Points[directives[curDirective].Points.Count - 1], Instantiate(Arrow) as GameObject));
                    curDirective++;
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
					float d = Vector3.Distance(directives[curDirective].Points[directives[curDirective].Points.Count - 1], p);
					if (d > 0.01f)
						directives[curDirective].AddPoint(p);
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
            if (Physics.Raycast(ray, out hit, cam.farClipPlane))
            {
                for (int i = 0; i < directives.Count; i++)
                    if (hit.collider.gameObject.Equals(directives[i].Pyramid))
                        return i;
            }
        }
        return -1;
    }
    void SelectDirectiveAndDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (selDirective == -1)
            {
                int t = getIndexOnClick(PerspectiveEditingCam);

                selDirective = t;
                mouseDragDirective = t;
                if (t > -1)
                {
                    arrowPS.enableEmission = true;
                    directives[t].Highlight = true;
				}
            }
        }
        if (mouseDragDirective > -1)
        {
            Vector3 dif = (Input.mousePosition - pMouse);
            Ray r = PerspectiveEditingCam.ScreenPointToRay(Input.mousePosition);
            float t = (directives[mouseDragDirective].Position.y - r.origin.y) / r.direction.y;
            int draggedLineIndex = 0;
            for (int i = 0; i < mouseDragDirective; i++)
                draggedLineIndex += directives[i].Points.Count - 1;

            if (Input.GetKey(KeyCode.LeftShift))
                directives[mouseDragDirective].Set(r.GetPoint(t), mouseDragDirective, directives);
            else
				directives[mouseDragDirective].Set(directives[mouseDragDirective].Position + new Vector3(0, dif.y * 0.03f, 0), mouseDragDirective, directives);
			
			if (Input.GetMouseButtonUp(0))
            {
                directives[mouseDragDirective].Pyramid.renderer.material.color = new Color(0.3f, 1.0f, 0.3f);
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
        //Vector3 dMouse = Input.mousePosition - pMouse;
        switch (camtype)
        {
            case CameraType.TopDownEditing:
                QuadCopter1.LineDrawingControl(TopDownEditingCam);
                //TopDownEditMode(TopDownEditingCam);
                
                break;
            case CameraType.PerspectiveEditing:
                //if (selDirective > -1)
                //    EditDirective(dMouse);
                //SelectDirectiveAndDrag();
                QuadCopter1.LineDrawingControl(PerspectiveEditingCam);
                //PerspectiveCameraControls(dMouse);
                break;
            case CameraType.Copter:
                //CopterCam.rect = new Rect(cameraX, cameraY, Screen.width * 0.5f, Screen.height);
                if(Input.GetMouseButton(0))
                QuadCopter1.transform.Rotate(Input.mousePosition, (0.5f * Time.deltaTime) * Mathf.Rad2Deg, relativeTo);
                break;
        }
        //pMouse = Input.mousePosition;
    }
    /*
    void PerspectiveCameraControls(Vector3 dMouse)
    {
        Vector3 terrainCenter = gameGroundLevel.transform.position + new Vector3(gameGroundLevel.transform.localScale.x, 6, gameGroundLevel.transform.localScale.z) / 2;
        if (Vector3.Distance(PerspectiveEditingCam.transform.position, terrainCenter) > 5)
            PerspectiveEditingCam.transform.position += PerspectiveEditingCam.transform.forward * Input.GetAxis("Mouse ScrollWheel");
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            PerspectiveEditingCam.transform.position += PerspectiveEditingCam.transform.forward * Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetMouseButton(2))
        {
            Vector3 campos = PerspectiveEditingCam.transform.position;
            Vector3 v = campos - terrainCenter;
            float x = v.x;
            float y = v.z;
            float theta = -dMouse.x * 0.003f;
            float xp = x * Mathf.Cos(theta) - y * Mathf.Sin(theta);
            float yp = x * Mathf.Sin(theta) + y * Mathf.Cos(theta);
            PerspectiveEditingCam.transform.position = new Vector3(terrainCenter.x + xp, Mathf.Clamp(campos.y + dMouse.y * 0.01f, terrainCenter.y - 2f, terrainCenter.y + 15f), terrainCenter.z + yp);
            PerspectiveEditingCam.transform.LookAt(terrainCenter);
        }
    }
    
    void EditDirective(Vector3 dMouse)
    {
        Directive d = directives[selDirective];
        Vector3 point = d.Position;
        d.Pyramid.renderer.material.color = Color.white;
        guiRect = new Rect(PerspectiveEditingCam.WorldToScreenPoint(point).x, Screen.height - PerspectiveEditingCam.WorldToScreenPoint(point).y, 320, 220);
        GUI.Window(0, guiRect, DirectiveData, "Directive Data");
        if (Input.GetMouseButtonDown(0))
            if (!guiRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
            {
                d.Pyramid.renderer.material.color = new Color(0.3f, 1.0f, 0.3f);
                selDirective = -1;
				arrowPS.enableEmission = false;
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
                mouseDragDirective = selDirective;
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
            Vector3 dMouse = Input.mousePosition - pMouse;
            Rect lookRect = new Rect(5, 50, 310, 20);


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
            
            // THE SPEED SLIDER
            GUI.skin = GUISkin;
            GUI.Label(new Rect(30, 118, 250, 30), "Line Length: " + d.Distance.ToString("0.00"));
            d.Speed = GUI.HorizontalSlider(new Rect(10, 145, 220, 30), d.Speed, 0.0f, 5.0f);
            GUI.Label(new Rect(70, 142, 100, 30), "Speed");
            GUI.Label(new Rect(240, 142, 70, 30), d.Speed.ToString("0.00"));
            GUI.skin.button.fontSize = 18;
            if (GUI.Button(new Rect(30, 170, 250, 28), "Wait for: " + d.WaitTime.ToString("0.0") + "s"))
            {
                if (Input.GetMouseButtonUp(0))
                    d.WaitTime += 0.1f;
                else if (Input.GetMouseButtonUp(1))
                    d.WaitTime = Mathf.Max(0.0f, d.WaitTime - 0.1f);
            }
            GUI.Label(new Rect(30, 195, 250, 28), "# data points: " + d.Points.Count.ToString());       
        }
    }
     * */
    bool movingLook = false;
    void CameraChecking()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            camtype = (CameraType)(((int)camtype + 1) % 4);
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
                TopDownEditingCam.pixelRect = new Rect(0, 0, Screen.width, Screen.height);
                TopDownEditingCam.enabled = true;
                CopterCam.enabled = false;
                PerspectiveEditingCam.enabled = false;
                break;
            case CameraType.Copter:
                CopterCam.pixelRect = new Rect(0, 0, Screen.width, Screen.height);
                TopDownEditingCam.enabled = false;
                PerspectiveEditingCam.enabled = false;
                CopterCam.enabled = true;
                break;
            case CameraType.DualCam:
                TopDownEditingCam.pixelRect = new Rect(0, 0, Screen.width * 0.5f, Screen.height);
                TopDownEditingCam.enabled = true;
                PerspectiveEditingCam.enabled = false;
                //CopterCam.rect = new Rect(float.Parse(cameraX), 0, Screen.width * 0.5f, Screen.height);
                CopterCam.enabled = true;
                break;
        }
    }
    #endregion

    #region Tools

    #endregion
}
