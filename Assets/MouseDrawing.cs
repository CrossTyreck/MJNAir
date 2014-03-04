using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MouseDrawing : MonoBehaviour
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
    List<GameObject> PyramidList = new List<GameObject>();
    int mouseDragPoint = -1;
    int selPoint = -1;
    Vector3 pMouse = Vector3.zero;
    List<Vector3> points = new List<Vector3>();
    CameraType camtype = CameraType.TopDownEditing;
    int speed;
    int lineres = 50;
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

    #endregion


    //
    void Start()
    {
        copter.SetActive(false);
        moving = false;
        drawingPath = true;
        startButton.enabled = false;
        PerspectiveEditingCam.enabled = false;
        speed = 0;
        score = new ScoringSystem();
        score.InitializeScore(); 
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

        CameraChecking();
        MouseControls();
        KeyboardControls();
    }
    
    void OnGUI()
    {
        GUI.skin = UISkin;
        GUI.Label(new Rect(Screen.width * 0.5f, 20, 250, 50), "Score: " + score.CurrentScore.ToString());
        GUI.Box(new Rect(10, 775, 250, 25), "Copter Position: " + copter.transform.position.ToString());
        GUI.Box(new Rect(10, 800, 250, 25), Vector3.Distance(target, copter.transform.position).ToString());
        GUI.Box(new Rect(10, 825, 250, 25), "Position counter: " + pathPosCount);
        GUI.Box(new Rect(10, 850, 250, 25), "Position counter: " + pathPosCount);

        if (GUI.Button(new Rect(Screen.width * 0.9f, 775, 150, 50), "Move Control"))
        {
            moving = !moving;
        }

        if (selPoint > -1)
        {
            guiRect = new Rect(PerspectiveEditingCam.WorldToScreenPoint(points[selPoint]).x, Screen.height - PerspectiveEditingCam.WorldToScreenPoint(points[selPoint]).y, 200, 100);
            if (GUI.Button(guiRect, "Location: " + points[selPoint].ToString()))
            {
               
            }                
        }
        
        
        
        if (Input.GetMouseButtonDown(0) && startButton.HitTest(Input.mousePosition))
        {
            drawingPath = false;
            moving = true;
            startButton.enabled = false;
            startButton.transform.position = new Vector3(9999, 9999, -100);

            if (points.Count > 0)
            {
                copter.transform.position = points[pathPosCount];
                target = points[1];
            }
            pathPosCount++;
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

    void MovingAlong()
    {

        if (Vector3.Distance(target, copter.transform.position) > ((speed + SpeedSlider.speed) * Time.deltaTime) && moving)
        {
            copter.transform.position += direction * (speed + SpeedSlider.speed) * Time.deltaTime;
        }

        else
        {
            if (pathPosCount < points.Count)
            {
                target = points[pathPosCount];
                pathPosCount++;
                score.CurrentScore += 10; 
            }
        }
    }

    #endregion

    #region Path drawing
    void CameraRayCastingOnClick(Camera cam)
    {
        if (drawingPath)
        {

            if (Input.GetMouseButton(0))
            {
                if (points.Count > 0)
                {
                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit = new RaycastHit();
                    int x1 = (int)(ray.origin.x / lineres);
                    int y1 = (int)(ray.origin.z / lineres);
                    int x2 = (int)(points[points.Count - 1].x / lineres);
                    int y2 = (int)(points[points.Count - 1].z / lineres);
                    if (x1 != x2 || y1 != y2)
                        if (t.collider.Raycast(ray, out hit, cam.farClipPlane))
                        {
                            points.Add(new Vector3(hit.point.x, hit.point.y + 100, hit.point.z));
                            Lines.SetVertexCount(points.Count);
                            Lines.SetPosition(points.Count - 1, points[points.Count - 1]);

                            PyramidList.Add(
                                        Instantiate(Arrow, points[points.Count - 2], Quaternion.LookRotation(points[points.Count - 1] - points[points.Count - 2]))
                                        as GameObject);
                        }
                }
                else
                {
                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit = new RaycastHit();
                    if (t.collider.Raycast(ray, out hit, cam.farClipPlane))
                    {
                        points.Add(new Vector3(hit.point.x, hit.point.y + 100, hit.point.z));
                        Lines.SetVertexCount(points.Count);
                        Lines.SetPosition(points.Count - 1, points[points.Count - 1]);
                    }
                }
            }
        }
    }


    int getIndexOnClick(Camera cam)
    {
        if (points.Count > 0)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, cam.farClipPlane, 1 << 9))
            {
                hit.collider.renderer.material.color = Color.black;
                return PyramidList.IndexOf(hit.collider.gameObject);
            }
        }
        return -1;
    }

    void SelectTriangleAndDrag()
    {

        if (Input.GetMouseButtonDown(0))
        {
            if (selPoint == -1)
            {
                int t = getIndexOnClick(PerspectiveEditingCam);
                selPoint = t;
                mouseDragPoint = t;
            }
            else
            {
                if (!guiRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
                {
                    Debug.Log(Input.mousePosition.ToString());
                    Debug.Log(guiRect.x.ToString() + " " + guiRect.y.ToString());
                    selPoint = -1;
                }
            }
            
        }
        if (mouseDragPoint > -1)
        {
            Vector3 dif = (Input.mousePosition - pMouse);
            Ray r = PerspectiveEditingCam.ScreenPointToRay(Input.mousePosition);
            float t = (points[mouseDragPoint].y - r.origin.y) / r.direction.y;
            if (Input.GetKey(KeyCode.LeftShift))
                points[mouseDragPoint] = r.GetPoint(t);
            else
                points[mouseDragPoint] += new Vector3(0, dif.y, 0);
            Lines.SetPosition(mouseDragPoint, points[mouseDragPoint]);
            PyramidList[mouseDragPoint].transform.position = points[mouseDragPoint];
            PyramidList[mouseDragPoint].transform.LookAt(points[mouseDragPoint + 1]);
            if(mouseDragPoint > 0)
                PyramidList[mouseDragPoint - 1].transform.LookAt(points[mouseDragPoint]);

            if (Input.GetMouseButtonUp(0))
            {
                PyramidList[mouseDragPoint].renderer.material.color = new Color(0.4f, 1f, 0.4f);
                mouseDragPoint = -1;
                Debug.Log(mouseDragPoint);
                Debug.Log(selPoint);
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
                CameraRayCastingOnClick(TopDownEditingCam);
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
                SelectTriangleAndDrag();
                Vector3 terrainCenter = t.transform.position + new Vector3(t.terrainData.size.x, 0, t.terrainData.size.z) / 2;
                Vector3 campos = PerspectiveEditingCam.transform.position;
                Vector3 cp = -Vector3.Cross(Vector3.up, PerspectiveEditingCam.transform.forward);
                PerspectiveEditingCam.transform.LookAt(terrainCenter);
                if (Vector3.Distance(PerspectiveEditingCam.transform.position, terrainCenter) > 300)
                    PerspectiveEditingCam.transform.position += PerspectiveEditingCam.transform.forward * Input.GetAxis("Mouse ScrollWheel") * 200;
                else if (Input.GetAxis("Mouse ScrollWheel") < 0)
                    PerspectiveEditingCam.transform.position += PerspectiveEditingCam.transform.forward * Input.GetAxis("Mouse ScrollWheel") * 200;
                if (Input.GetMouseButton(2))
                {
                    float distance = (PerspectiveEditingCam.transform.position - terrainCenter).magnitude;
                    PerspectiveEditingCam.transform.position = new Vector3(campos.x, Mathf.Clamp(campos.y + dMouse.y, terrainCenter.y + 50f, terrainCenter.y + 500f), campos.z) + cp * dMouse.x * 2;
                    PerspectiveEditingCam.transform.position = terrainCenter+(PerspectiveEditingCam.transform.position - terrainCenter).normalized * distance;
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
