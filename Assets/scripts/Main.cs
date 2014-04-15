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
    public Texture topDownCameraUI;
    public Texture topDownButtonsBG;
    public Texture goButton;
    public Texture stopButton;
    Vector3 pMouse = Vector3.zero;
    CameraType camtype = CameraType.TopDownEditing;
    Vector3 direction;
    Vector3 cross;
    Vector3 target;
    float scale;
    Space relativeTo = Space.World;
    public GUITexture startButton;
    public GUISkin UISkin;
    float lastPinchDistance = 0.0f;
    public GUIStyle speedButton;
    public GUIStyle speedSlider;
    public float sliderValue;
    public bool endingCondition = false;
    public Transform gameBoard;
    public GameObject goPlanePosition;
    public GUISkin GUISkin;
    #endregion

    void Start()
    {
        gameGrid = new ScoreBoard(gameGroundLevel.transform);
        
        startButton.enabled = false;
        startButton.transform.position = new Vector3(0.5f, 0.5f, 1);
        PerspectiveEditingCam.enabled = false;
        score = new ScoringSystem();
        score.InitializeScore();
    }

    void Update()
    {
        if (endingCondition)
            score.FinalScore += gameGrid.GameBoardScore + gameGrid.GetScoreFromTraversed();

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            CameraCheckingPC();
            MouseCameraControls();
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            CameraCheckingTouch();
            TouchCameraControls();
        }
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
    }

    #region User Controls
    void MouseControls()
    {
        //Vector3 dMouse = Input.mousePosition - pMouse;
        switch (camtype)
        {
            case CameraType.TopDownEditing:
                QuadCopter1.LineDrawingControl(TopDownEditingCam);
                break;
            case CameraType.PerspectiveEditing:
                QuadCopter1.LineDrawingControl(PerspectiveEditingCam);
                break;
            case CameraType.Copter:
                if(Input.GetMouseButton(0))
                QuadCopter1.transform.Rotate(Input.mousePosition, (0.5f * Time.deltaTime) * Mathf.Rad2Deg, relativeTo);
                break;
        }
    }
 
    void CameraCheckingPC()
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

    void CameraCheckingTouch()
    {
        //Swipe left
        if (Input.touchCount == 4)
        {
            camtype = (CameraType)(((int)camtype + 1) % 4);
            CameraSwitch();
        }
        //Swipe right
        if (Input.touchCount == 4)
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
                CopterCam.enabled = true;
                break;
        }
    }
    #endregion
}
