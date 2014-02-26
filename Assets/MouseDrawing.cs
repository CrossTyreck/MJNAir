using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MouseDrawing : MonoBehaviour
{

    #region Variable Definitions
    enum CameraType
    {
        PerspectiveEditing,
        TopDownEditing,
        Copter
    }

    public Camera gameCam;
    public Camera topDownCam;
    public Camera copterCam;
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

    public bool drawingPath = false;
    public GUITexture startButton;

    #endregion

    void Start()
    {
        copter.SetActive(false);
        moving = false;
        drawingPath = true;
        startButton.enabled = false;
        gameCam.enabled = false;
        speed = 0;
    }

    void Update()
    {
        switch (camtype)
        {
            case CameraType.TopDownEditing:
                CameraRayCastingOnClick(topDownCam);
                break;
            case CameraType.PerspectiveEditing:
                SelectTriangleAndDrag();
                break;
            case CameraType.Copter:

                break;
        }

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
        if (Input.GetKeyDown(KeyCode.Space))
        {

            CameraSwitch();
        }
    }

    void OnGUI()
    {

        GUI.Box(new Rect(10, 775, 250, 25), "Copter Position: " + copter.transform.position.ToString());
        GUI.Box(new Rect(10, 800, 250, 25), Vector3.Distance(target, copter.transform.position).ToString());
        GUI.Box(new Rect(10, 825, 250, 25), "Position counter: " + pathPosCount);
        GUI.Box(new Rect(10, 850, 250, 25), "Position counter: " + pathPosCount);

		if (selPoint > -1) {
			if(GUI.Button( new Rect(Camera.main.WorldToScreenPoint(points[mouseDragPoint]).x, Camera.main.WorldToScreenPoint(points[mouseDragPoint]).y, 300, 150), "Location: " + points[mouseDragPoint].ToString()))
			{
				Debug.Log (" I AM SUPER! ");
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

        if (Vector3.Distance(target, copter.transform.position) > ((speed + SpeedSlider.speed) * Time.deltaTime))
        {
            copter.transform.position += direction * (speed + SpeedSlider.speed) * Time.deltaTime;
        }

        else
        {
            if (pathPosCount < points.Count)
            {
                target = points[pathPosCount];
                pathPosCount++;
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
            selPoint = mouseDragPoint = getIndexOnClick(gameCam);
            Debug.Log(mouseDragPoint);
        }
        if (mouseDragPoint > -1)
        {
            Vector3 dif = (Input.mousePosition - pMouse);
            Ray r = gameCam.ScreenPointToRay(Input.mousePosition);
            float t = (points[mouseDragPoint].y - r.origin.y) / r.direction.y;
            if (Input.GetKey(KeyCode.LeftShift))
                points[mouseDragPoint] = r.GetPoint(t);
            else
                points[mouseDragPoint] += new Vector3(0, dif.y, 0);
            Lines.SetPosition(mouseDragPoint, points[mouseDragPoint]);
            PyramidList[mouseDragPoint].transform.position = points[mouseDragPoint];
            PyramidList[mouseDragPoint].transform.LookAt(points[mouseDragPoint + 1]);


            if (Input.GetMouseButtonUp(0))
            {
                PyramidList[mouseDragPoint].renderer.material.color = new Color(0.4f, 1f, 0.4f);
                mouseDragPoint = -1;
            }
        }
        pMouse = Input.mousePosition;
    }

    #endregion
    
    #region User Controls
    void CameraSwitch()
    {
        switch (camtype)
        {
            case CameraType.PerspectiveEditing:
                camtype = CameraType.TopDownEditing;
                topDownCam.enabled = true;
                copterCam.enabled = false;
                gameCam.enabled = false;
                break;
            case CameraType.TopDownEditing:
                camtype = CameraType.PerspectiveEditing;
                topDownCam.enabled = false;
                copterCam.enabled = false;
                gameCam.enabled = true;
                break;
            case CameraType.Copter:
                topDownCam.enabled = false;
                gameCam.enabled = false;
                copterCam.enabled = true;
                break;
        }
    }
    #endregion
}
