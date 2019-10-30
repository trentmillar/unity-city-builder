using UnityEngine;
using System.Collections;

/**
 * Panel for selling a building.
 */ 
public class UISellBuildingPanel : UIGamePanel {

	public UILabel goldLabel;
	public UILabel resourceLabel;
	public UISprite buildingSprite;
	public UILabel messageLabel;
	public GameObject sellForGoldButton;
	
	override protected void Init() {
		// Force content to normal position then update cancel button position
		content.transform.position = showPosition;	
	}
	
	/**
	 * Set up the building with the given building.
     */
	override public void InitialiseWithBuilding(Building building) {
		resourceLabel.text = ((int)building.Type.cost * BuildingManager.RECLAIM_PERCENTAGE).ToString();
		goldLabel.text =  ((int)Mathf.Max(1.0f, (int)(building.Type.cost * BuildingManager.GOLD_SELL_PERCENTAGE))).ToString ();
		buildingSprite.spriteName = building.Type.spriteName;
		messageLabel.text = string.Format ("         Are you sure you want to sell your {0} for {1} resources?", building.Type.name, (BuildingManager.GOLD_SELL_PERCENTAGE <= 0 ? "": "gold or "));
		if (BuildingManager.GOLD_SELL_PERCENTAGE <= 0) sellForGoldButton.SetActive(false);
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
	
	new protected IEnumerator DoShow() {
		yield return new WaitForSeconds(UI_DELAY / 3.0f);
		content.SetActive(true);
	}
	
	new protected IEnumerator DoHide() {
		content.SetActive(false);
		yield return true;
	}
	
}
