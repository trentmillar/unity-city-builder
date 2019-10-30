using UnityEngine;
using System.Collections;

public class UIResourceView : MonoBehaviour {

	public UILabel resourceLabel;

	public UILabel goldLabel;
	
	private int displayedResources;
	private int displayedGold;

	void Start() {
		displayedResources = ResourceManager.Instance.Resources;
		resourceLabel.text = displayedResources.ToString();
		displayedGold = ResourceManager.Instance.Gold;
		goldLabel.text = displayedGold.ToString();

	}

	public void UpdateResource(bool instant = false) {
		StopCoroutine ("DisplayResource");
		if (instant) {
			resourceLabel.text = ResourceManager.Instance.Resources.ToString();
			displayedResources = ResourceManager.Instance.Resources;
		} else {
			StartCoroutine ("DisplayResource");
		}
	}

	public void UpdateGold(bool instant = false) {
		StopCoroutine ("DisplayGold");
		if (instant) {
			goldLabel.text = ResourceManager.Instance.Gold.ToString();
			displayedGold = ResourceManager.Instance.Gold;
		} else {
			StartCoroutine ("DisplayGold");
		}
	}


	private IEnumerator DisplayResource() {
		while (displayedResources != ResourceManager.Instance.Resources) {
			int difference = displayedResources - ResourceManager.Instance.Resources;
			if (difference > 2000) displayedResources -= 1000;
			else if (difference > 200) displayedResources -= 100;
			else if (difference > 20) displayedResources -= 10;
			else if (difference > 0) displayedResources -= 1;
			else if (difference < -2000) displayedResources += 1000;
			else if (difference < -200) displayedResources += 100;
			else if (difference < -20) displayedResources += 10;
			else if (difference < 0) displayedResources += 1;
			resourceLabel.text = displayedResources.ToString();
			yield return true;
		}
	}

	private IEnumerator DisplayGold() {
		while (displayedGold != ResourceManager.Instance.Gold) {
			int difference = displayedGold - ResourceManager.Instance.Gold;
			if (difference > 2000) displayedGold -= 1000;
			else if (difference > 200) displayedGold -= 100;
			else if (difference > 20) displayedGold -= 10;
			else if (difference > 0) displayedGold -= 1;
			else if (difference < -2000) displayedGold += 1000;
			else if (difference < -200) displayedGold += 100;
			else if (difference < -20) displayedGold += 10;
			else if (difference < 0) displayedGold += 1;
			goldLabel.text = displayedGold.ToString();
			yield return true;
		}
	}
}
