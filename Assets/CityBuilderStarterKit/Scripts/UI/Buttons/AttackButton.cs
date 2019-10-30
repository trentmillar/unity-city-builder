using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/**
 * A button which performs the attack activity.
 */ 
public class AttackButton : MonoBehaviour {

	public string activity;
	public UISprite icon;
	public UISprite ring;
	public UISprite background;
	public ChooseTargetButton chooseTargetButton;

	void OnEnable() {
		if (OccupantManager.GetInstance().GetAllOccupants().Count < 1) {
			collider.enabled = false;
			icon.color = UIColor.DESATURATE;
			ring.gameObject.SetActive(false);
			background.color = UIColor.DESATURATE;
		} else {
			collider.enabled = true;
			icon.color = Color.white;
			ring.gameObject.SetActive(true);
			background.color = Color.white;
		}
	}

	public void OnClick() {
		ActivityManager.GetInstance().StartActivity(activity, System.DateTime.Now, OccupantManager.GetInstance().GetAllOccupants().Select(o=>o.uid).ToList());
		UIGamePanel.ShowPanel(PanelType.DEFAULT);
	}

}
