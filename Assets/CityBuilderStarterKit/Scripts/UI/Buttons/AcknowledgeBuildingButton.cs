using UnityEngine;
using System.Collections;

/**
 * Acknowledge building turning it from ready to built.
 */ 
public class AcknowledgeBuildingButton : MonoBehaviour {

	public Building building;

	public void OnClick() {
		if (building.State == BuildingState.READY) {
			BuildingManager.GetInstance ().AcknowledgeBuilding (building);
		} else if (building.State == BuildingState.IN_PROGRESS || building.CurrentActivity != null) {
			UIGamePanel.ShowPanel(PanelType.SPEED_UP);
			if (UIGamePanel.activePanel is UISpeedUpPanel) ((UISpeedUpPanel)UIGamePanel.activePanel).InitialiseWithBuilding(building);
		} else {
			building.AcknowledgeActivity();
		}
	}
}