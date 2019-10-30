using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIGamePanel : MonoBehaviour {
	
	public const float UI_DELAY = 0.75f;
	public const float UI_TRAVEL_DIST = 0.6f;
	
	public GameObject content;
	public PanelType panelType;
	public PanelType openFromPanelOnly;

	/**
	 * Position of the buttons when visible.
	 */ 
	protected Vector3 showPosition;
	
	/**
	 * Position of the buttons when hidden.
	 */ 
	protected Vector3 hidePosition;
	
	public static Dictionary <PanelType, UIGamePanel> panels;

	void Awake() {
		if (panels == null) panels = new Dictionary <PanelType, UIGamePanel> ();
		panels.Add (panelType, this);
		showPosition = content.transform.position;
		hidePosition = content.transform.position - new Vector3(0, UI_TRAVEL_DIST, 0);
		if (panelType == PanelType.DEFAULT) {
			activePanel = this;
		} else {
			content.transform.position = hidePosition;	
		}
		
		Init ();
	}

	virtual protected void Init() {
	}
	
	virtual public void InitialiseWithBuilding(Building building) {
	}
	
	virtual public void Show() {
		if (activePanel == this) {
			StartCoroutine(DoReShow());
		} else if (activePanel == null || activePanel.panelType == openFromPanelOnly || openFromPanelOnly == PanelType.NONE) {
			if (activePanel != null) activePanel.Hide ();
			StartCoroutine(DoShow());
			activePanel = this;
		}
	}

	virtual public void Hide() {
		StartCoroutine(DoHide());
	}

	public static void ShowPanel(PanelType panelType) {
		if (panelType == PanelType.DEFAULT) BuildingManager.ActiveBuilding = null;
		if (panels.ContainsKey (panelType)) panels [panelType].Show ();
	}

	public static UIGamePanel activePanel;
		
	/**
	 * Reshow the panel (i.e. same panel but for a different object/building).
	 */ 
	virtual protected IEnumerator DoReShow() {
		iTween.MoveTo(content, hidePosition, UI_DELAY);
		yield return new WaitForSeconds(UI_DELAY / 3.0f);
		iTween.MoveTo(content, showPosition, UI_DELAY);
	}
	
	
	/**
	 * Show the panel.
	 */ 
	virtual protected IEnumerator DoShow() {
		yield return new WaitForSeconds(UI_DELAY / 3.0f);
		content.SetActive (true);
		iTween.MoveTo(content, showPosition, UI_DELAY);
	}
	
	/**
	 * Hide the panel. 
	 */
	virtual protected IEnumerator DoHide() {
		iTween.MoveTo(content, hidePosition, UI_DELAY);
		yield return new WaitForSeconds(UI_DELAY / 3.0f);
		content.SetActive (false);
	}
}

public enum PanelType {
	NONE,
	DEFAULT,
	BUY_BUILDING,
	PLACE_BUILDING,
	RESOURCE,
	BUY_RESOURCES,
	BUILDING_INFO,
	SELL_BUILDING,
	OBSTACLE_INFO,
	SPEED_UP,
	RECRUIT_OCCUPANTS,
	VIEW_OCCUPANTS,
	TARGET_SELECT,
	BATTLE_RESULTS
}
