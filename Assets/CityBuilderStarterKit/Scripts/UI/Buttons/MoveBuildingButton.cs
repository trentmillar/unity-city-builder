using UnityEngine;
using System.Collections;

public class MoveBuildingButton : MonoBehaviour {
		
	public void OnClick() {
		BuildingManager.ActiveBuilding.StartMoving ();
		UIGamePanel.ShowPanel (PanelType.PLACE_BUILDING);
	}
}
