using UnityEngine;
using System.Collections;

/**
 * Handles resources, gold, etc.
 */ 
public class ResourceManager : MonoBehaviour {
	
	/**
	 * View of resources.
	 */
	public UIResourceView view;
	
	/**
	 * Default for resources when new game is started.
	 */ 
	public int defaultResources = 400;
	
	/**
	 * Default for gold when new game is started.
	 */ 
	public int defaultGold = 20;
	
	/**
	 * The resource used for actually building buildings. In some games this is called coins or gp.
	 */ 
	virtual public int Resources {
		get; private set;
	}
	
	/**
	 * The resource used to speed things up. Some games might call this gems or cash.
	 */ 
	virtual public int Gold {
		get; private set;
	}

	void Awake() {
		Instance = this;
		Resources = defaultResources;
		Gold = defaultGold;
	}
	
	/**
	 * Load resources from save game data.
	 */ 
	virtual public void Load(SaveGameData data) {
		Resources = data.resources;
		Gold = data.gold;
		view.UpdateResource(true);
		view.UpdateGold(true);
	}
	
	/**
	 * Return true if there are enough resources to build the given building.
	 */ 
	public bool CanBuild(BuildingTypeData building) {
		if (Resources >= building.cost) return true;
		return false;
	}

	/**
	 * Subtract the given number of resources.
	 */ 
	public void RemoveResources(int resourceCost) {
		if (Resources >= resourceCost) {
			Resources -= resourceCost;
			view.UpdateResource();
		} else {
			Debug.LogWarning("Tried to buy something without enough resource");
		}
	}

	/**
	 * Adds the given number of resources.
	 */ 
	public void AddResources(int resources) {
		Resources += resources;
		view.UpdateResource();
	}

	/**
	 * Subtract the given number of gold.
	 */ 
	public void RemoveGold(int goldCost) {
		if (Gold >= goldCost) {
			Gold -= goldCost;
			view.UpdateGold();
		} else {
			Debug.LogWarning("Tried to buy something without enough gold");
		}
	}
	
	/**
	 * Adds the given number of gold.
	 */ 
	public void AddGold(int gold) {
		Gold += gold;
		view.UpdateGold();
	}

	public static ResourceManager Instance {
		get; private set;
	}
}
