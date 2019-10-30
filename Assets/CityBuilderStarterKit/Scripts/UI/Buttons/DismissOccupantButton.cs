using UnityEngine;
using System.Collections;

/**
 * Button for getting rid of an occupant from a building.
 */ 
public class DismissOccupantButton : MonoBehaviour {
	
	public OccupantData occupant;
	public UISprite icon;
	public UIFilledSprite ring;
	public UIFilledSprite background;
	public UILabel label;

	protected bool canDismiss;

	public void Init(OccupantData occupant) {
		this.occupant = occupant;
		canDismiss = true;
		icon.spriteName = "cancel_icon";
		ring.color = new Color(1,0,0);
		ring.fillAmount = 1.0f;
		background.fillAmount = 1.0f;
		label.text="DISMISS";
	}

	public void InitAsAttack(float percentageComplete) {
		canDismiss = false;
		icon.spriteName = "army_icon";
		ring.color = new Color(0.5f,0,1);
		ring.fillAmount = percentageComplete;
		background.fillAmount = percentageComplete;
		label.text="BATTLE";
	}

	public void OnClick() {
		if (BuildingManager.ActiveBuilding != null && canDismiss) {
			BuildingManager.ActiveBuilding.DismissOccupant(occupant);
			UIGamePanel.ShowPanel(PanelType.VIEW_OCCUPANTS);
		}
	}
}
