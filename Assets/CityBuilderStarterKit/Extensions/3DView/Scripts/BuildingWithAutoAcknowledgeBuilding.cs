using UnityEngine;
using System.Collections;

/**
 * A building implementation that automatically finishes all tasks.
 */ 
public class BuildingWithAutoAcknowledgeBuilding : Building
{
	/**
	 * Finish building auto acknolwedge.
	 */ 
	override public void CompleteBuild() {
		State = BuildingState.READY;
		Acknowledge();
		// view.SendMessage ("UI_UpdateState");
	}
}

