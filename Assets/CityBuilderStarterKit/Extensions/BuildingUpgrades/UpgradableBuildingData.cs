/**
 * Extends building data by including a building level which can be upgraded.
 */ 
[System.Serializable]
public class UpgradableBuildingData : BuildingData {

	/**
	 * The buildings current level.
	 */ 
	virtual public int level {get; set;}
}
