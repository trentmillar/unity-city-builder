using UnityEngine;
using System.Collections;

/**
 * Panel for speeding up an activity by paying gold.
 */ 
public class UISpeedUpPanel : UIGamePanel {

	public UILabel goldLabel;
	public UISprite buildingSprite;
	public UILabel timeLabel;
	public UISprite headerSprite;
	public UISprite headerRing;
	
	//private Building building;
	
	override protected void Init() {
		// Force content to normal position then update cancel button position
		content.transform.position = showPosition;	
	}
	
	/**
	 * Set up the building with the given building.
     */
	override public void InitialiseWithBuilding(Building building) {
	//	this.building = building;
		if (building.CurrentActivity != null) {
			BuildingManager.ActiveBuilding = building;
			timeLabel.text = string.Format("Time Remaining: {0} minutes {1} second{2}", (int)building.CurrentActivity.RemainingTime.TotalMinutes, building.CurrentActivity.RemainingTime.Seconds, (building.CurrentActivity.RemainingTime.Seconds != 1 ? "s" : ""));
			goldLabel.text =  ((int)Mathf.Max (1, (float) (building.CurrentActivity.RemainingTime.TotalSeconds + 1 ) / (float)BuildingManager.GOLD_TO_SECONDS_RATIO)).ToString ();
			buildingSprite.spriteName = building.Type.spriteName;
			headerSprite.spriteName = building.CurrentActivity.Type.ToString().ToLower() + "_icon";
			headerRing.color = UIColor.GetColourForActivityType(building.CurrentActivity.Type);
			StartCoroutine(UpdateLabels());
		} else {
			Debug.LogError ("Can't speed up a building with no activity");	
		}
	}
	
	override public void Show() {
		if (activePanel == null || activePanel.panelType == openFromPanelOnly || openFromPanelOnly == PanelType.NONE) {
			if (activePanel != null) activePanel.Hide ();
			StartCoroutine(DoShow());
			activePanel = this;
		}
	}

	override public void Hide() {
		StartCoroutine(DoHide());
	}
	
	/**
	 * Update the labels as time passes.
	 */ 
	protected IEnumerator UpdateLabels() {
		while (BuildingManager.ActiveBuilding != null && BuildingManager.ActiveBuilding.CurrentActivity != null && BuildingManager.ActiveBuilding.CurrentActivity.RemainingTime.TotalSeconds > 0) {
			timeLabel.text = string.Format("Time Remaining: {0} minutes {1} second{2}", (int)BuildingManager.ActiveBuilding.CurrentActivity.RemainingTime.TotalMinutes, BuildingManager.ActiveBuilding .CurrentActivity.RemainingTime.Seconds, (BuildingManager.ActiveBuilding.CurrentActivity.RemainingTime.Seconds != 1 ? "s" : ""));
			goldLabel.text = ((int)Mathf.Max (1, (float) (BuildingManager.ActiveBuilding .CurrentActivity.RemainingTime.TotalSeconds + 1 ) / (float)BuildingManager.GOLD_TO_SECONDS_RATIO)).ToString ();
			yield return true;
		}
		// If we get to here we finished... hide this panel.
		if (BuildingManager.ActiveBuilding != null && BuildingManager.ActiveBuilding.CurrentActivity != null) {
			UIGamePanel.ShowPanel (PanelType.DEFAULT);
		}
	}
	
	new protected IEnumerator DoShow() {
		yield return new WaitForSeconds(UI_DELAY / 3.0f);
		content.SetActive(true);
	}
	
	new protected IEnumerator DoHide() {
		content.SetActive(false);
		yield return true;
	}
	
}
