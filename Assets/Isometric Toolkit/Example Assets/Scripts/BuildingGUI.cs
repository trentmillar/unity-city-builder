using UnityEngine;
using System.Collections;

public class BuildingGUI : MonoBehaviour {
	
	private int buildingindex;
	private string buildingname;
		
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(!Globals.IsBuildPlacing) return;
					
		if (Input.GetKeyDown("1"))
		{
			Globals.BuildManager.SelectedBuilding = 0;
		}
		if (Input.GetKeyDown("2"))
		{
			Globals.BuildManager.SelectedBuilding = 1;
		}
		
		buildingindex = Globals.BuildManager.SelectedBuilding;
		buildingname = Globals.BuildManager.Building[buildingindex].name;
	}
	
	void OnGUI()
	{
		
		GUILayout.TextArea("Selected Building Index: " + buildingindex + " Name: " + buildingname);
	}
}
