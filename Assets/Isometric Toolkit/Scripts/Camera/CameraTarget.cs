using UnityEngine;
using System.Collections;

public class CameraTarget : MonoBehaviour {
	
	public Transform ParentCamera;
	
	private int targetLayerMask = Constants.TerrainLayerMask;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		RaycastHit hit;
		var fwd = ParentCamera.TransformDirection(Vector3.forward);
		if (Physics.Raycast(ParentCamera.position, fwd, out hit, Mathf.Infinity, targetLayerMask)) {
			//print ("There is something in front of the object!");
			//Debug.DrawRay(transform.position, ParentCamera.TransformDirection (Vector3.back) * hit.distance, Color.yellow);
			transform.position = hit.point;
		}
		else {
			//print ("nothing in front of the object!");
		}
	}
}
