using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UIBuildingSelect : UIGamePanel {

	public GameObject buildingScrollPanel;
	public GameObject buildingPanelPrefab;
	public GameObject cancelButton;
	
	private bool initialised = false;
	private List<UIBuildingSelectPanel> buildingSelectPanels;

	override protected void Init() {
	}
	
	void Start() {
		if (!initialised) {
			List <BuildingTypeData> types = BuildingManager.GetInstance().GetAllBuildingTypes().Where(b=>!b.isObstacle).ToList();
			buildingSelectPanels = new List<UIBuildingSelectPanel>();
			foreach(BuildingTypeData type in types) {
				AddBuildingPanel(type);
			}
			buildingScrollPanel.GetComponent<UIGrid>().Reposition();
			initialised = true;
		}
		// Force content to normal position then update cancel button position
		content.transform.position = showPosition;	
		showPosition = cancelButton.transform.position;
		hidePosition = cancelButton.transform.position + new Vector3(0, UI_TRAVEL_DIST, 0);
		cancelButton.transform.position = hidePosition;
	}

	override public void Show() {
		if (activePanel != null) activePanel.Hide ();
		foreach(UIBuildingSelectPanel p in buildingSelectPanels) {
			p.UpdateBuildingStatus();
		}
		StartCoroutine(DoShow ());
		activePanel = this;
	}
	
	override public void Hide() {
		StartCoroutine(DoHide ());
	}
	
	new protected IEnumerator DoShow() {
		content.SetActive(true);
		yield return true;
		buildingScrollPanel.GetComponent<UIDraggablePanel>().ResetPosition();
		yield return new WaitForSeconds(UI_DELAY / 3.0f);
		cancelButton.SetActive (true);
		iTween.MoveTo(cancelButton, showPosition, UI_DELAY);
	}
	
	new protected IEnumerator DoHide() {
		content.SetActive(false);
		iTween.MoveTo(cancelButton, hidePosition, UI_DELAY);
		yield return new WaitForSeconds(UI_DELAY / 3.0f);
		cancelButton.SetActive (false);
	}
	
	private void AddBuildingPanel(BuildingTypeData type) {
		GameObject panelGo = (GameObject) NGUITools.AddChild(buildingScrollPanel, buildingPanelPrefab);
		UIBuildingSelectPanel panel = panelGo.GetComponent<UIBuildingSelectPanel>();
		panel.InitialiseWithBuildingType(type);
		buildingSelectPanels.Add (panel);
	}
}
