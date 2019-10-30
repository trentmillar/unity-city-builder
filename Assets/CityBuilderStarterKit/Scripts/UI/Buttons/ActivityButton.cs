using UnityEngine;
using System.Collections;

/**
 * A button which can perform any activity type.
 */ 
public class ActivityButton : MonoBehaviour {

	public UISprite icon;
	public UISprite ring;
	public UILabel label;

	private ActivityDelegate activity;
	private string activityType;
	private string supportingId;
	
	/**
	 * Set up the delegate and visuals.
	 */
	public void InitWithActivityType (ActivityDelegate activity, string activityType, string supportingId) {
		this.activity = activity;
		this.activityType = activityType;
		this.supportingId = supportingId;
		if (activityType != ActivityType.RECRUIT) {
			icon.spriteName = activityType.ToString().ToLower() + "_icon";
			ring.color = UIColor.GetColourForActivityType (activityType);
		}
		string labelString = activityType.ToString ();
		labelString.Replace ("_", " ");
		labelString = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(labelString);
		label.text = labelString;
	}
	
	public void OnClick() {
		activity (activityType, supportingId);
	}
}
