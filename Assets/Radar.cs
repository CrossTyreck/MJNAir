using UnityEngine;
using System.Collections;

public class Radar : MonoBehaviour {
	/*
	 	attach this script to a gui object
	  	position x 0.87 y 0.17 z 0
	  	scale 0.1, 0.1, 0.1
	  	create a plane and attach to gui object
	  	position 23000, 0, 1000
	  	scale 1000, 1, 1000
	  	use the back texture for this plane
	  	create a plane and attach to the plane above
	  	position 0, 1, 0
	  	scale 1, 1, 1
	  	use the pointer texture for this plane
	  	assign the active camera to cam
	  	this should work, drag the mouse around the dial to move it.

	*/
	public GameObject RadarPlane;
    public GameObject RadarPointer;
    public Camera cam;
	float pangle = 0.0f;

    void Start() { }

	void OnMouseOver()  {
		Vector3 mv = cam.ScreenToViewportPoint(Input.mousePosition);
		Vector2 oPos = new Vector2(this.transform.position.x, this.transform.position.y);
		float angle = Mathf.Atan2(mv.y - oPos.y, mv.x - oPos.x) * Mathf.Rad2Deg;
        if (Input.GetMouseButton(0))
        {
			float current_angle = RadarPointer.transform.rotation.eulerAngles.y;
            RadarPointer.transform.rotation = Quaternion.AngleAxis(current_angle + (pangle - angle), Vector3.up);
        }      
		pangle = angle;
	}
}