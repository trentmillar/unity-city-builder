using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Interface for any object that can be positioned on the grid
 */ 
public interface IGridObject {
	List<GridPosition> Shape{ get; }
	GridPosition Position{ get; set; }
}

