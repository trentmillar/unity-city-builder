using UnityEngine;
using System.Collections;

public class Globals : MonoBehaviour
{
	
	
	public static RaycastHit? CurrentMouseHitOnTerrain = null;
	public static bool IsBuildPlacing;
	public static BuildManager BuildManager;
	
	#region timers
	private float _nextBuildPlacing;
	#endregion
	
	// Use this for initialization
	void Start ()
	{
		CurrentMouseHitOnTerrain = null;
		IsBuildPlacing = false;
		BuildManager = GameObject.Find ("BuildManager").GetComponent<BuildManager> ();
	}
	
	// Update is called once per frame
	void Update ()
	{		
		#region GetMouse Hit on Terrain
		
		RaycastHit hit;
		Ray mousePositionRay = Camera.main.ScreenPointToRay (Input.mousePosition);
         
		if (Physics.Raycast (mousePositionRay, out hit, Mathf.Infinity, Constants.TerrainLayerMask)) {			
			CurrentMouseHitOnTerrain = hit;
		} else {
			CurrentMouseHitOnTerrain = null;
		}
		
		#endregion
		
		#region IsPlacing a building?
		
		if (_nextBuildPlacing < Time.time) {
			if (IsBuildPlacing && Input.GetMouseButton (1)) {
				IsBuildPlacing = false;
				_nextBuildPlacing = SetNextTime(0.1f);
			} else if (CurrentMouseHitOnTerrain.HasValue && Input.GetMouseButton (1)) {
				IsBuildPlacing = true;
				_nextBuildPlacing = SetNextTime(0.1f);
			}
		}
		
		#endregion
	}
	
	private float SetNextTime(float delta) {
		return Time.time + delta;
	}
}
