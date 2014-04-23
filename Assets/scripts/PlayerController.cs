using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
/// <summary>
/// A MonoBehaviour handling the basic player interface with the game, to be used by up to four players.
/// </summary>
public class PlayerController : MonoBehaviour
{

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
    public float speed = 1.0f;
    /// <summary>
    /// The amount of time until the copter begins moving again, in seconds
    /// </summary>
    float waitTime;
    /// <summary>
    /// Determines if the copter is moving.
    /// </summary>
    public bool Moving = false;
    /// <summary>
    /// The next point along this copter's path.
    /// </summary>
    Vector3 target;
    int draggedDirective;
    int pathFollowingDirective;
    int selDirective;
    int currentDirective;

    public bool Drawing;
    int currentPathPosition;
    Vector2 pTouchPosition;
    Vector3 pMouse;

    public static float FlashTimer;
    public static string Message;
    public GUISkin customSkin;
    Rect directiveEditingRect;
    public float Energy;

    void Start()
    {
        directives = new List<Directive>();
        draggedDirective = -1;
        selDirective = -1;
        FlashTimer = 0.0f;
        speed = 1.0f;
        Message = "";
        Energy = 100;
        currentPathPosition = 0;
        pTouchPosition = Vector3.zero;
        pMouse = Vector3.zero;
        directives.Add(new Directive(PlayerCopter.transform.position, Instantiate(Arrow) as GameObject));
        currentDirective = 0;
        pathFollowingDirective = 0;
        target = directives[pathFollowingDirective].Points[0];
    }

    void Update()
    {

        foreach (Directive d in directives)
            d.Update(LineParticles);
        if (FlashTimer > 0.0f)
            FlashTimer -= Time.deltaTime;
        if (waitTime > 0.0f)
        {
            waitTime -= Time.deltaTime;
            if (waitTime < 0.0f)
            {
                Moving = true;
                waitTime = 0.0f;
            }
        }

        if (Energy == 0)
        {
            Moving = false;
        }
        if (Moving)
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
            GUI.Label(new Rect(Screen.width * 0.1f, Screen.height * 0.4f, Screen.width * 0.8f, Screen.height * 0.2f), Message);
        }
        //if (directives[0].Points.Count < 2)
        //    GUI.Label(new Rect(Screen.width * 0.1f, Screen.height * 0.15f, Screen.width * 0.8f, Screen.height * 0.15f),
        //               "Draw a path for your copter to follow by moving your finger across the screen");
        //else if (directives.Count < 2)
        //    GUI.Label(new Rect(Screen.width * 0.3f, Screen.height * 0.15f, Screen.width * 0.4f, Screen.height * 0.15f),
        //               "Add additional directives to control your copter by tapping twice");

        if (selDirective > -1)
        {
            GUI.Window(0, directiveEditingRect, DirectiveData, "Directive Data");
            if (Application.platform == RuntimePlatform.Android)
            {
                if (Input.touchCount == 1)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began)
                        if (!directiveEditingRect.Contains(new Vector2(Input.GetTouch(0).position.x, Screen.height - Input.GetTouch(0).position.y)))
                        {
                            directives[selDirective].Highlight = false;
                            selDirective = -1;
                        }
                }
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                if (Input.GetMouseButtonDown(0))
                    if (!directiveEditingRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
                    {
                        directives[selDirective].Highlight = false;
                        selDirective = -1;
                    }
            }
        }
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
        if (selDirective > -1)
        {
            Vector3 point = directives[selDirective].Position;
            directiveEditingRect = new Rect(cam.WorldToScreenPoint(point).x, Screen.height - cam.WorldToScreenPoint(point).y, 320, 220);
        }
    }

    void TopDownEditMode(Camera cam)
    {
        if (Input.GetTouch(0).tapCount == 2)
        {
            if (directives[directives.Count - 1].Position != directives[currentDirective].Points[directives[currentDirective].Points.Count - 1])
            {
                directives.Add(new Directive(directives[currentDirective].Points[directives[currentDirective].Points.Count - 1], Instantiate(Arrow) as GameObject));
                currentDirective++;
            }
        }
        else if (Input.touchCount == 1)
        {
            Drawing = true;
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
        else
        {
            Drawing = false;
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
        if (selDirective > -1)
        {
            if (directives[selDirective].LookEdit)
            {
                if (Input.touchCount == 1)
                {
                    Vector2 dTouch = Input.GetTouch(0).deltaPosition;
                    if (dTouch.x > 1.0f || dTouch.x < -1.0f)
                    {
                        directives[selDirective].Pyramid.transform.Rotate(dTouch.x * 0.01f, 0f, 0f);
                        directives[selDirective].LookVector = directives[selDirective].Pyramid.transform.forward;
                    }
                    if (dTouch.x > 1.0f || dTouch.x < -1.0f)
                    {
                        directives[selDirective].Pyramid.transform.Rotate(0f, dTouch.y * 0.01f, 0f);
                        directives[selDirective].LookVector = directives[selDirective].Pyramid.transform.forward;
                    }
                    if (Input.GetTouch(0).phase == TouchPhase.Ended)
                    {
                        directives[selDirective].LookEdit = false;
                        draggedDirective = selDirective = -1;
                    }
                }
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
        if (selDirective > -1)
        {
            if (directives[selDirective].LookEdit)
            {
                Vector3 dMouse = Input.mousePosition - pMouse;
                if(dMouse.x > 1.0f || dMouse.x < -1.0f)
                {
                    directives[selDirective].Pyramid.transform.Rotate(dMouse.x * 0.5f, 0f, 0f);
                    directives[selDirective].LookVector = directives[selDirective].Pyramid.transform.forward;
                }
                if (dMouse.x > 1.0f || dMouse.x < -1.0f)
                {
                    directives[selDirective].Pyramid.transform.Rotate(0f, dMouse.y * 0.5f, 0f);
                    directives[selDirective].LookVector = directives[selDirective].Pyramid.transform.forward;
                }
                if (Input.GetMouseButtonDown(0))
                {
                    directives[selDirective].LookEdit = false;
                    draggedDirective = selDirective = -1;
                }
            }
        }
    }


    void DirectiveData(int id)
    {
        if (!(selDirective > -1))
            return;
        Directive d = directives[selDirective];
        if (GUI.Button(new Rect(5, 25, 310, 20), "Pos X:" + d.Position.x.ToString("0.0") + " Y:" + d.Position.y.ToString("0.0") + " Z:" + d.Position.z.ToString("0.0")))
            if(draggedDirective != selDirective)
                draggedDirective = selDirective;

        if (GUI.Button(new Rect(5, 50, 310, 20), "Look X:" + d.LookVector.x.ToString("0.0") + " Y:" + d.LookVector.y.ToString("0.0") + " Z:" + d.LookVector.z.ToString("0.0")))
            d.LookEdit = true;
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
        

        // THE SPEED SLIDER
        GUI.skin = customSkin;
        GUI.Label(new Rect(30, 118, 250, 30), "Line Length: " + d.Distance.ToString("0.00"));
        GUI.Label(new Rect(70, 142, 100, 30), "Speed");
        GUI.Label(new Rect(240, 142, 70, 30), d.Speed.ToString("0.00"));
        d.Speed = GUI.HorizontalSlider(new Rect(10, 145, 220, 30), d.Speed, 0.1f, 5.0f);

        // THE WAIT TIME SLIDER
        GUI.Label(new Rect(70, 170, 100, 30), "Wait Time");
        GUI.Label(new Rect(240, 170, 70, 30), d.WaitTime.ToString("0.00") + "s");
        d.WaitTime = GUI.HorizontalSlider(new Rect(10, 170, 220, 30), d.WaitTime, 0.0f, 20.0f);

        GUI.skin.button.fontSize = 18;
        GUI.Label(new Rect(30, 195, 250, 28), "# data points: " + d.Points.Count.ToString());
    }
    #endregion
    void MovingAlong()
    {
        Vector3 direction = target - PlayerCopter.transform.position;
        Vector3 next = direction.normalized * Time.deltaTime * speed * 20f;
        float dmag = direction.magnitude;
        if (next.magnitude > dmag)
        {
            PlayerCopter.transform.position = target;
            if (currentPathPosition < directives[pathFollowingDirective].Points.Count)
                target = directives[pathFollowingDirective].Points[currentPathPosition];
            else
                QueryDirective();
        }
        else
        {
            PlayerCopter.transform.position += next;
            if (dmag < 0.2f)
            {
                currentPathPosition++;
                if (currentPathPosition < directives[pathFollowingDirective].Points.Count)
                    target = directives[pathFollowingDirective].Points[currentPathPosition];
                else
                    QueryDirective();
            }
        }
    }
    void AlignAllDirectives()
    {
        for (int i = 0; i < directives.Count; i++)
            directives[i].Align(directives, i);
    }
    void QueryDirective()
    {
        pathFollowingDirective++;
        if (pathFollowingDirective < directives.Count)
        {
            currentPathPosition = 0;
            speed = directives[pathFollowingDirective].Speed;
            PlayerCopter.transform.forward = directives[pathFollowingDirective].LookVector;
            target = directives[pathFollowingDirective].Points[currentPathPosition];
            waitTime = directives[pathFollowingDirective].WaitTime;
            if (waitTime > 0.0f)
                Moving = false;

        }
        else
        {
            
            Moving = false;
            pathFollowingDirective--;
        }
    }
    public static void FlashMessage(string m, float t)
    {
        Message = m;
        FlashTimer = t;
    }

    /// <summary>
    /// One way of calculating energy usage based on obstacles hit
    /// </summary>
    /// <param name="collider"></param>
    //private void OnCollisionEnter(Collider collider)
    //{
    //    if (collider.gameObject.tag == "Obstacle")
    //    {
    //       Energy -= collider.gameObject.GetComponent<Obstacle>().EnergyConsumptionMultiplier;
    //    }

    //}

    /// <summary>
    /// Sets energy and verifies it does not go below 0 or above 100
    /// Does not work as float is intended. 
    /// </summary>
    /// <param name="EnergyValue"></param>
    /// <returns></returns>
    public float SetEnergy(float EnergyValue)
    {
       
        for (int i = 0; i < Convert.ToUInt32(EnergyValue); i++)
        {
            if (EnergyValue < 0)
            {
                if (Energy > 0)
                {
                    Energy--;
                }
                else
                {
                    Moving = false; //might not want this here
                    return 0;
                }
            }

            if (EnergyValue > 0)
            {
                for (int j = 0; j < EnergyValue; j++)
                {
                    if (Energy < 100)
                    {
                        Energy++;
                    }
                    else
                        return 100;
                }
            }
        }

        return Energy;
    }

    /// <summary>
    /// Trying to increase energy level. 
    /// </summary>
    /// <param name="collider"></param>
    public void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Obstacle")
        {
            if (Energy < 100)
                Energy = 100;
        }
    }
}
