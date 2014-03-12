using UnityEngine;
using System.Collections;

public class RadarCube : MonoBehaviour {
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
    public GameObject RadarPointer;
    public Camera cam;
	Vector3 previousPoint;
	float speed = 0;

	void Start() { 
		speed = 90/transform.localScale.x;
	}
	
	void OnMouseEnter() {
		previousPoint = cam.ScreenToWorldPoint(Input.mousePosition);
	}
	void OnMouseDown() {
		previousPoint = cam.ScreenToWorldPoint(Input.mousePosition);
	}

	void OnMouseOver() {
		if (Input.GetMouseButton (0)) {
						Vector3 mv = cam.ScreenToWorldPoint (Input.mousePosition);
						Vector3 dir = previousPoint - transform.position;
						dir.y = 0;
						Vector3 mov = mv - previousPoint;
						RadarPointer.transform.Rotate (Vector3.Cross (dir, mov).normalized, mov.magnitude * speed);
						previousPoint = mv;
				}
	}
}