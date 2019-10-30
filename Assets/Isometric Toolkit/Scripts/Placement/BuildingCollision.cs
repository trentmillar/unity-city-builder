using UnityEngine;
using System.Collections;

/// <summary>
/// Building collision.
/// 
/// This script is attached to the buildings.
/// </summary>

public class BuildingCollision : MonoBehaviour {
	
	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnCollisionEnter(Collision collision)
	{
		
		GameObject go = GameObject.Find("BuildManager");
		BuildManager bm = go.GetComponent<BuildManager>();
		
		
		if (collision.collider.gameObject.tag != "Floor")
		{
			bm.collided.Add(go);
		}

	}

	void OnCollisionExit()
	{
		GameObject go = GameObject.Find("BuildManager");
		BuildManager bm = go.GetComponent<BuildManager>();
		
		bm.collided.Remove(go);
	}
	

}
