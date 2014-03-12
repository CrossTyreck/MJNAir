using UnityEngine;
using System.Collections;

public class RadarPlane : MonoBehaviour {

	Vector3 viewportCoordinates;
	public Camera topDownCamera;
	
	void Start() {
		viewportCoordinates = topDownCamera.WorldToViewportPoint (transform.position);
	}
	void Update () {
		Vector3 worldPosition = topDownCamera.ViewportToWorldPoint (viewportCoordinates);
		transform.position = new Vector3 (worldPosition.x, transform.position.y, worldPosition.z);

	}
}
