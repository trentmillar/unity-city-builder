using UnityEngine;
using System.Collections;
/**
 * Button for collecting stored resources.
 */ 
public class CollectButton : MonoBehaviour {
	
	public UILabel resourceLabel;
	public UISprite icon;
	public UISprite ring;
	
	protected Building myBuilding;
	
	virtual public void Init(Building building) {
		myBuilding = building;
		icon.spriteName = building.Type.generationType.ToString().ToLower() + "_icon";
		ring.color = UIColor.GetColourForRewardType(building.Type.generationType);
		StartCoroutine (DoUpdateResourceLabel());
	}
	
	virtual public void OnClick() {
		myBuilding.Collect();
		resourceLabel.text = "" + myBuilding.StoredResources;
	}
	
	/**
	 * Coroutine to ensure the displayed resource is up to date.
	 */ 
	private IEnumerator DoUpdateResourceLabel() {
		resourceLabel.text = "" + myBuilding.StoredResources;
		while (gameObject.activeInHierarchy || myBuilding == null) {
			resourceLabel.text = "" + myBuilding.StoredResources;
			// Update frequently
			yield return new WaitForSeconds(0.25f);
		}
	}
}
