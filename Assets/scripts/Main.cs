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
    }

    public PlayerController QuadCopter1;
    ScoringSystem score;
    ScoreBoard gameGrid;
    public GameObject gameGroundLevel;
    //Used to display game Grid for testing
    public GameObject boardSquare;
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
    public float sliderValue = 1.0f;
    public bool endingCondition = false;
    public Transform gameBoard;
    public GameObject goPlanePosition;
    public GUISkin GUISkin;
    int x;
    int z;
    Vector3 offset;
    int count;
    Quaternion rotation;
Vector3 radius;
float currentRotation = 0.0f;
    #endregion

    void Start()
    {
        radius = new Vector3(1, 0, 0);
        currentRotation = 0.0f;
        gameGrid = new ScoreBoard(gameGroundLevel.transform, GameObject.FindGameObjectsWithTag("Obstacle"));
        count = 0;
        x = (int)(gameGroundLevel.transform.localScale.x * 10);
        z = (int)(gameGroundLevel.transform.localScale.z * 10);
        offset = new Vector3(gameGroundLevel.transform.position.x - x * 0.5f, 0, gameGroundLevel.transform.position.z - z * 0.5f);

        startButton.enabled = false;
        startButton.transform.position = new Vector3(0.5f, 0.5f, 1);
        PerspectiveEditingCam.enabled = false;
        score = new ScoringSystem();
        score.InitializeScore();
    }

    void Update()
    {
        foreach (GameObject obstacle in gameGrid.Obstacles)
        {
            if (obstacle.name == "Bee")
            {
                currentRotation += Input.GetAxis("Horizontal") * Time.deltaTime * 100;
                rotation.eulerAngles = new Vector3(0, currentRotation, 0);
                //obstacle.transform.position = rotation * radius;
                obstacle.transform.Rotate(rotation * radius);
             
            }
            
        }


        QuadCopter1.GetComponent<EnergyBar>().barDisplay = QuadCopter1.Energy * 0.01f;
        if (!QuadCopter1.Drawing)
            startButton.enabled = true;
        else
            startButton.enabled = false;

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

        //Setting up and checking copter position on the GameBoard
        foreach (BoardSquare square in gameGrid.GameBoard)
        {
            if ((Vector2.Distance(new Vector2(QuadCopter1.transform.position.x, QuadCopter1.transform.position.z), square.Position) <= 1))
            {
                if (!square.Traversed)
                {
                    square.Traversed = true;
                    score.CurrentScore += square.PointValue;

                    QuadCopter1.Energy += square.EnergyUsed;
                    //Not working
                    //QuadCopter1.SetEnergy(square.EnergyConsumptionMultiplier);
                    
                    Instantiate(boardSquare, new Vector3(square.Position.x, 5, square.Position.y), Quaternion.identity);
                }
            }

        }
    }

    void OnGUI()
    {
        GUI.skin = GUISkin;
        MouseControls();

        if(GUI.Button(new Rect(0.5f, 0.5f, 100, 50), "Exit"))
            Application.Quit();

        GUI.Label(new Rect(Screen.width * 0.85f, Screen.height * 0.05f, 100, 50), score.CurrentScore.ToString());
        switch (camtype)
        {
            case CameraType.Copter:
                sliderValue = GUI.VerticalSlider(new Rect(Screen.width * 0.025f, Screen.height * 0.6f, 75, 250), sliderValue, 10.0f, 0.0f, speedSlider, speedButton);
                QuadCopter1.speed = sliderValue;
                break;
            case CameraType.TopDownEditing:
                sliderValue = GUI.VerticalSlider(new Rect(Screen.width * 0.025f, Screen.height * 0.6f, 75, 250), sliderValue, 10.0f, 0.0f, speedSlider, speedButton);
                if (Input.GetMouseButtonDown(0) && startButton.HitTest(Input.mousePosition))
                {
                    QuadCopter1.Drawing = false;
                    QuadCopter1.Moving = true;
                    startButton.enabled = false;
                    startButton.transform.position = new Vector3(9999, 9999, -100);
                    //QuadCopter1.transform.position = directives[0].Points[0];
                    //curDirective = 0;
                    //pathPosCount = 0;
                    //target = directives[curDirective].Points[pathPosCount];
                    //QuadCopter1.gameObject.SetActive(true);
                }
                break;
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
                {
                    TopDownEditingCam.transform.position -= (new Vector3(dMouse.x, 0, dMouse.y) * TopDownEditingCam.orthographicSize) * 0.003f;
                }

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
                        if (lastPinchDistance == 0f)
                            lastPinchDistance = pinchDistance;
                        float deltaPinchDistance = lastPinchDistance - pinchDistance;
                        if (Vector3.Distance(PerspectiveEditingCam.transform.position, terrainCenter) > 5)
                            PerspectiveEditingCam.transform.position += PerspectiveEditingCam.transform.forward * deltaPinchDistance * 0.1f;
                        else if (deltaPinchDistance > 0)
                            PerspectiveEditingCam.transform.position += PerspectiveEditingCam.transform.forward * deltaPinchDistance * 0.1f;
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
                else
                    lastPinchDistance = 0f;
                break;
            case CameraType.TopDownEditing:
                if (Input.touchCount == 2)
                {
                    float pinchDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                    TopDownEditingCam.transform.position -= (new Vector3(Input.GetTouch(0).deltaPosition.x, 0, Input.GetTouch(0).deltaPosition.y) * TopDownEditingCam.orthographicSize) * 0.003f;
                    if (pinchDistance > 50f)
                    {
                        if (lastPinchDistance == 0f)
                            lastPinchDistance = pinchDistance;
                        float deltaPinchDistance = lastPinchDistance - pinchDistance;
                        if (deltaPinchDistance > 0)
                        {
                            if (TopDownEditingCam.orthographicSize > 5)
                                TopDownEditingCam.orthographicSize -= deltaPinchDistance * 0.1f;
                        }
                        if (deltaPinchDistance < 0)
                            TopDownEditingCam.orthographicSize -= deltaPinchDistance * 0.1f;

                        lastPinchDistance = pinchDistance;
                    }
                }
                else
                    lastPinchDistance = 0f;
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                break;
        }
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
                if (Input.GetMouseButton(0))
                    QuadCopter1.transform.Rotate(Input.mousePosition, (0.5f * Time.deltaTime) * Mathf.Rad2Deg, relativeTo);
                break;
        }
    }

    void CameraCheckingPC()
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

    void CameraCheckingTouch()
    {
        if (Input.touchCount >= 3)
        {
            if (Input.GetTouch(2).phase == TouchPhase.Ended)
            {
                camtype = (CameraType)(((int)camtype + 1) % 3);
                CameraSwitch();
            }
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
        }
    }
    #endregion
}
