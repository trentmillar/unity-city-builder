using UnityEngine;
using System.Collections;

public class DoSellBuildingButton : MonoBehaviour {

	public bool isGold = false;

	public void OnClick() {
		if (isGold) {
			BuildingManager.GetInstance ().SellBuildingForGold (BuildingManager.ActiveBuilding);
		} else {
			BuildingManager.GetInstance ().SellBuilding (BuildingManager.ActiveBuilding, false);
		}
		UIGamePanel.ShowPanel (PanelType.DEFAULT);
		BuildingManager.ActiveBuilding = null;
	}
}
