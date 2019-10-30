using System.Collections.Generic;

/**
 * All data required for a saved game.
 */ 
[System.Serializable]
public class SaveGameData  {

	public virtual List<BuildingData> buildings {get; set;}
	
	public virtual int resources {get; set;}
	
	public virtual int gold {get; set;}

	public virtual List<Activity> activities {get; set; }
}
