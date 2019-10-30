using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/**
 * Panel for selecting the occupant to recruit.
 */ 
public class UIOccupantSelect : UIGamePanel {

	public GameObject occupantScrollPanel;
	public GameObject occupantPanelPrefab;
	public GameObject cancelButton;
	
	private bool initialised = false;
	private List<UIOccupantSelectPanel> occupantSelectPanels;
	
	override protected void Init() {
		// Force content to normal position then update cancel button position
		content.transform.position = showPosition;	
		showPosition = cancelButton.transform.position;
		hidePosition = cancelButton.transform.position + new Vector3(0, UI_TRAVEL_DIST, 0);
		cancelButton.transform.position = hidePosition;	
	}
	
	override public void InitialiseWithBuilding(Building building) {
		if (!initialised) {
			List <OccupantTypeData> types = OccupantManager.GetInstance().GetAllOccupantTypes().Where(o=>o.recruitFromIds.Contains(building.Type.id)).ToList();
			occupantSelectPanels = new List<UIOccupantSelectPanel>();
			foreach(OccupantTypeData type in types) {
				AddOccupantPanel(type);
			}
			initialised = true;
		}
	}

	override public void Show() {
		if (activePanel != null) activePanel.Hide ();
		StartCoroutine(DoShow ());
		activePanel = this;
	}
	
	override public void Hide() {
		StartCoroutine(DoHide ());
	}
	
	override protected IEnumerator DoShow() {
		if (occupantSelectPanels != null) {
			foreach(UIOccupantSelectPanel p in occupantSelectPanels) {
				Destroy(p.gameObject);
			}
		}
		initialised = false;
		InitialiseWithBuilding(BuildingManager.ActiveBuilding);
		yield return true;
		content.SetActive(true);
		foreach(UIOccupantSelectPanel p in occupantSelectPanels) {
			p.UpdateOccupantStatus();
		}
		yield return true;
		occupantScrollPanel.GetComponent<UIDraggablePanel>().ResetPosition();
		occupantScrollPanel.GetComponent<UIGrid>().Reposition();
		yield return new WaitForSeconds(UI_DELAY / 3.0f);
		cancelButton.SetActive (true);
		iTween.MoveTo(cancelButton, showPosition, UI_DELAY);
	}
	
	override protected IEnumerator DoHide() {
		content.SetActive(false);
		iTween.MoveTo(cancelButton, hidePosition, UI_DELAY);
		yield return new WaitForSeconds(UI_DELAY / 3.0f);
		cancelButton.SetActive (false);
	}
	
	private void AddOccupantPanel(OccupantTypeData type) {
		GameObject panelGo = (GameObject) NGUITools.AddChild(occupantScrollPanel, occupantPanelPrefab);
		UIOccupantSelectPanel panel = panelGo.GetComponent<UIOccupantSelectPanel>();
		panel.InitialiseWithOccupantType(type);
		occupantSelectPanels.Add (panel);
	}
}
