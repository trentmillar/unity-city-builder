using UnityEngine;
using System.Collections;

public class SellBuildingButton : MonoBehaviour {
	
	public void OnClick() {
		UIGamePanel.ShowPanel (PanelType.SELL_BUILDING);
		((UISellBuildingPanel)UIGamePanel.activePanel).InitialiseWithBuilding(BuildingManager.ActiveBuilding);
	}

}
