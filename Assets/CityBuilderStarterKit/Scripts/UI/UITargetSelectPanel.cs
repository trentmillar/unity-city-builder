using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
/**
 * Panel for showing target select
 */ 
public class UITargetSelectPanel : UIGamePanel {

	public GameObject cancelButton;

	override protected void Init() {
	}
	
	void Start() {
		// Force content to normal position then update cancel button position
		content.transform.position = showPosition;	
		showPosition = cancelButton.transform.position;
		hidePosition = cancelButton.transform.position + new Vector3(0, UI_TRAVEL_DIST, 0);
		cancelButton.transform.position = hidePosition;
	}
	
	override public void Show() {
		if (activePanel != null) activePanel.Hide ();
		StartCoroutine(DoShow ());
		activePanel = this;
	}
	
	override public void Hide() {
		StartCoroutine(DoHide ());
	}
	
	new protected IEnumerator DoShow() {
		content.SetActive(true);
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

}