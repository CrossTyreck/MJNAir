using UnityEngine;
using System.Collections;

public class StartGameDirectionsMenu : MonoBehaviour
{
    string directions = "Next";
    string startLevel = "START";
  
    void OnGUI()
    {
        if (Application.loadedLevel == 1)
            if (GUI.Button(new Rect(Screen.width * 0.885f, Screen.height * 0.26f, 125, 75), directions))
                Application.LoadLevel(Application.loadedLevel + 1);
        if (GUI.Button(new Rect(Screen.width * 0.885f, Screen.height * 0.84f, 125, 75), Application.loadedLevel < 4 && Application.loadedLevel > 1 ? directions : startLevel))
            if (Application.loadedLevel >= 4 || Application.loadedLevel == 1)
            {
                Application.LoadLevel("Kitchen Level 1");
            }
            else
            {
                Application.LoadLevel(Application.loadedLevel + 1);
            }
        if (GUI.Button(new Rect(Screen.width * 0.03f, Screen.height * 0.86f, 125, 75), "Main Menu"))
            Application.LoadLevel(0);
    }
}
