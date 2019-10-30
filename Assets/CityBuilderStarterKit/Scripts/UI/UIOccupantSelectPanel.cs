using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * User interface for an individual occupant panel. Shown when
 * selecting which occupant type to recruit.
 */
public class UIOccupantSelectPanel : MonoBehaviour
{
	public UILabel titleLabel;
	public UILabel descriptionLabel;
	public UILabel allowsLabel;
	public UILabel costLabel;
	public UISprite sprite;
	public UISprite backgroundSprite;
	public ActivityButton recruitButton;

	public string[] spriteNames;

	private OccupantTypeData type;

	/**
	 * Set up the occupant with the given type data.
     */
	public void InitialiseWithOccupantType(OccupantTypeData type) {
		this.type = type;
		titleLabel.text = type.name;
		descriptionLabel.text = type.description;
		costLabel.text = type.cost.ToString();
		sprite.spriteName = type.spriteName;
		UpdateOccupantStatus ();
	}

	/**
	 * Updates the UI (text, buttons, etc), based on if the occupant type
	 * requirements are met or not.
     */
	public void UpdateOccupantStatus() {
		if (OccupantManager.GetInstance().CanRecruitOccupant(type.id) && BuildingManager.ActiveBuilding.CanFitOccupant(type.occupantSize)) {
			allowsLabel.text = "Allows: " + FormatIds(type.allowIds, false);
			backgroundSprite.spriteName = spriteNames[0];
			recruitButton.gameObject.SetActive(true);
			recruitButton.InitWithActivityType(DoActivity, ActivityType.RECRUIT, type.id);
		} else {
			if (OccupantManager.GetInstance().CanRecruitOccupant(type.id)) {
				allowsLabel.text = "[ff0000]Not Enough Room";
				backgroundSprite.spriteName = spriteNames[1];
				recruitButton.gameObject.SetActive(false);
			} else {
				allowsLabel.text = "Requires: " + FormatIds(type.requireIds, true);
				backgroundSprite.spriteName = spriteNames[1];
				recruitButton.gameObject.SetActive(false);
			}
		}
	}

	/**
	 * Formats the allows/required identifiers to be nice strings, coloured correctly.
	 * Returns the identifiers.
     */
	private string FormatIds(List<string> allowIds, bool redIfNotPresent) {
		BuildingManager manager = BuildingManager.GetInstance();
		string result = "";
		foreach (string id in allowIds) {
			if (redIfNotPresent && !manager.PlayerHasBuilding(id)) {
				result += "[ff0000]";
			} else {
				result += "[000000]";
			}
			BuildingTypeData type = manager.GetBuildingTypeData(id);
			if (type != null) {
				result += manager.GetBuildingTypeData(id).name + ", ";
			} else {
				Debug.LogWarning("No building type data found for id:" + id);
				result += id + ", ";
			}
		}
		if (result.Length > 2) {
			result = result.Substring(0, result.Length - 2);
		} else {
			return "Nothing";
		}
		return result;
	}
	
	/**
	 * Start the generic activity function.
	 */ 
	public void DoActivity(string type, string supportingId) {
		if (BuildingManager.ActiveBuilding.StartActivity (type, System.DateTime.Now, supportingId)) {
			UIGamePanel.ShowPanel (PanelType.DEFAULT);
		} else {
			Debug.LogWarning("This is where you pop up your IAP screen");
		}
	}
	
}