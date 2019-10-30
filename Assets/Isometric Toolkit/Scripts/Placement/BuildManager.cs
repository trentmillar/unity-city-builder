using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Build manager.
/// 
/// This Script is attached to a empty GameObject
/// </summary>

public class BuildManager: MonoBehaviour 
{
	public int SelectedBuilding;
	private int LastSelectedBuilding;
	public GameObject[] Building;
	
	public List<GameObject> collided = new List<GameObject>();
	public List<BuildingList> buildings = new List<BuildingList>();
	
	public string TerrainCollisionTag;
	
	private GameObject ghost;

	private bool ghostOn = false;

	private bool isFlat;
	public float maxSlopeHigh = 5f;
	
	void Start()
	{
		LastSelectedBuilding = SelectedBuilding;
	}
	
	void Update()
	{
		if(!Globals.IsBuildPlacing){
			if(ghostOn){
				Destroy(ghost);
				ghostOn = false;
			}
			return;
		}
			
		Ray ray;
		RaycastHit[] hit;
		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		hit = Physics.RaycastAll(ray, Mathf.Infinity);
		
		
		for (int i = 0; i < hit.Length; i++)
		{
			if (hit[i].collider.tag == TerrainCollisionTag)
			{
				if (SelectedBuilding != LastSelectedBuilding && ghost != null)
				{
					Destroy(ghost);
					ghostOn = false;
					LastSelectedBuilding = SelectedBuilding;
					
					collided.Clear();
					break;
				}
				
				if (!ghostOn)
				{
					ghost = (GameObject)Instantiate(Building[SelectedBuilding], 
					new Vector3(hit[i].point.x,
						hit[i].point.y + (Building[SelectedBuilding].transform.localScale.y / 2), 
						hit[i].point.z), 
						Quaternion.identity);
						
					ghostOn = true;	
				}
			
		
				// adding building
				if (Input.GetMouseButtonDown(0) && collided.Count == 0 && isFlat )
				{
					BuildingList bl = new BuildingList();
					
					DestroyImmediate(ghost);
					
					bl.buildingGameObject = (GameObject)Instantiate(Building[SelectedBuilding], 
					new Vector3(hit[i].point.x, 
						hit[i].point.y + (Building[SelectedBuilding].transform.localScale.y / 2), 
						hit[i].point.z), 
					Quaternion.identity);
					
					string s = bl.buildingGameObject.name.Replace("(Clone)", "");
					bl.buildingGameObject.name = s;
					bl.buildingName = s;
					
					buildings.Add(bl);
				
					ghostOn = false;
					Globals.IsBuildPlacing = false;
				
					collided.Clear();
					break;
				}
				
				if (ghost != null)
				{
					ghost.transform.position = new Vector3(
						hit[i].point.x, 
						hit[i].point.y + Building[SelectedBuilding].collider.transform.localScale.y / 2, 
						hit[i].point.z);
							
					isFlat = GroundFlat(ghost);
						
					if (collided.Count != 0 || !isFlat)
					{
						ghost.renderer.material.CopyPropertiesFromMaterial(Building[SelectedBuilding].renderer.sharedMaterial);
						ghost.renderer.material.color = new Color(
							1f,
							0f, 
							0f, 
							0.6f);
					}			
					else if (collided.Count == 0 && isFlat)
					{
						ghost.renderer.material.CopyPropertiesFromMaterial(Building[SelectedBuilding].renderer.sharedMaterial);
						ghost.renderer.material.color = new Color(
							0f,
							1f, 
							0f, 
							0.6f);
					}
			
				}
			}	
		}
	}

	bool GroundFlat(GameObject Ghost)
	{
		RaycastHit[] buildingSlopeHitUL;
		RaycastHit[] buildingSlopeHitUR;
		RaycastHit[] buildingSlopeHitDL;
		RaycastHit[] buildingSlopeHitDR;
		RaycastHit[] buildingSlopeHitM;
		
		buildingSlopeHitUL = Physics.RaycastAll(new Vector3(
			ghost.collider.transform.position.x - ghost.transform.localScale.x / 2,
			ghost.collider.transform.position.y + (ghost.transform.localScale.y / 2),
			ghost.collider.transform.position.z - ghost.transform.localScale.z / 2),
			Vector3.down, Mathf.Infinity);
		
		buildingSlopeHitUR = Physics.RaycastAll(new Vector3(
			ghost.collider.transform.position.x + ghost.transform.localScale.x / 2,
			ghost.collider.transform.position.y + (ghost.transform.localScale.y / 2),
			ghost.collider.transform.position.z - ghost.transform.localScale.z / 2),
			Vector3.down, Mathf.Infinity);
		
		buildingSlopeHitDL = Physics.RaycastAll(new Vector3(
			ghost.collider.transform.position.x - ghost.transform.localScale.x / 2,
			ghost.collider.transform.position.y + (ghost.transform.localScale.y / 2),
			ghost.collider.transform.position.z + ghost.transform.localScale.z / 2),
			Vector3.down, Mathf.Infinity);
		
		buildingSlopeHitDR = Physics.RaycastAll(new Vector3(
			ghost.collider.transform.position.x + ghost.transform.localScale.x / 2,
			ghost.collider.transform.position.y + (ghost.transform.localScale.y / 2),
			ghost.collider.transform.position.z + ghost.transform.localScale.z / 2),
			Vector3.down, Mathf.Infinity);
		
		buildingSlopeHitM = Physics.RaycastAll(new Vector3(
			ghost.collider.transform.position.x,
			ghost.collider.transform.position.y + (ghost.transform.localScale.y / 2),
			ghost.collider.transform.position.z),
			Vector3.down, Mathf.Infinity);
		
		for (int i = 0; i < buildingSlopeHitM.Length; i++)
		{
			if(buildingSlopeHitUL[i].collider == null) return false;
			
			if ((buildingSlopeHitUL[i].collider.tag == TerrainCollisionTag) &&
				(buildingSlopeHitUR[i].collider.tag == TerrainCollisionTag) &&
				(buildingSlopeHitDL[i].collider.tag == TerrainCollisionTag) &&
				(buildingSlopeHitDR[i].collider.tag == TerrainCollisionTag) &&
				(buildingSlopeHitM[i].collider.tag == TerrainCollisionTag))
			{
				if ((buildingSlopeHitUL[i].distance <= maxSlopeHigh) &&
					(buildingSlopeHitUR[i].distance <= maxSlopeHigh) &&
					(buildingSlopeHitDL[i].distance <= maxSlopeHigh) &&
					(buildingSlopeHitDR[i].distance <= maxSlopeHigh) &&
					(buildingSlopeHitM[i].distance <= maxSlopeHigh))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
			

		}
		return false;
		
	}
	
}
	