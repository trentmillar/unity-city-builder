using UnityEngine;
using System.Collections;

/**
 * A building view built using NGUI.
 */ 
public class BuildingView : UIDraggableGridObject {

	/**
	 * Building Sprite
	 */ 
	public UISprite buildingSprite;

	/**
	 * Activity icon.
	 */ 
	public UISprite currentActivity;

	/**
	 * Game object wrapping the various
	 * widgets used to indicate progress. Also
	 * acts as a button.
	 */ 
	public AcknowledgeBuildingButton progressIndicator;

	/**
	 * Sprites showing progress of current activity.
	 */ 
	public UIFilledSprite[] progressRings;

	/* The UIWidget that is being dragged */
	protected UIWidget widget;
	
	/**
	 * Reference to the camera showing this object.
	 */ 
	protected UIDraggableCamera draggableCamera;

	/**
	 * Building this view references.
	 */ 
	private Building building;

	
	/**
	 * Internal initialisation.
	 */
	void Awake(){
		target = transform;	
		myPosition = target.position;
		grid = GameObject.FindObjectOfType(typeof(AbstractGrid)) as AbstractGrid;
		draggableCamera = GameObject.FindObjectOfType(typeof(UIDraggableCamera)) as UIDraggableCamera;
	}

	/**
	 * Initialise the building view.
	 */ 
	public void UI_Init(Building building) {
		this.building = building;
		widget = buildingSprite;
		buildingSprite.spriteName = building.Type.spriteName;
		buildingSprite.MakePixelPerfect();
		progressIndicator.building = building;
		SetColor (building.Position);
		myPosition = transform.localPosition;
		SnapToGrid();
	}

	/**
	 * Update objects position
	 */ 
	override public void SetPosition(GridPosition pos) {
		Vector3 position = grid.GridPositionToWorldPosition(pos);
		widget.depth = 1000 - (int)position.z;
		position.z = target.localPosition.z;
		target.localPosition = position;
		myPosition = target.localPosition;
	}

	/**
	 * When object is dragged, move object then snap it to the grid.
	 */ 
	void OnDrag (Vector2 delta) {
		if (CanDrag && enabled && target != null) {
			UICamera.currentTouch.clickNotification = UICamera.ClickNotification.BasedOnDelta;
			myPosition = myPosition + (Vector3)(delta * (draggableCamera != null ? draggableCamera.scale.x : 1));
			GridPosition pos = SnapToGrid();
			PostDrag(pos);
		}
	}

	/**
	 * NGUI Click event... building clicked.
	 */ 
	public void OnClick() {
		if (building.Type.isObstacle) {
			if (building.CurrentActivity == null && building.CompletedActivity == null) {
				UIGamePanel.ShowPanel (PanelType.OBSTACLE_INFO);
				if (UIGamePanel.activePanel is UIBuildingInfoPanel) ((UIBuildingInfoPanel)UIGamePanel.activePanel).InitialiseWithBuilding(building);
			}
		} else if (building.State == BuildingState.BUILT) {
			UIGamePanel.ShowPanel (PanelType.BUILDING_INFO);
			if (UIGamePanel.activePanel is UIBuildingInfoPanel) ((UIBuildingInfoPanel)UIGamePanel.activePanel).InitialiseWithBuilding(building);
		}
	}

	/**
	 * Snap object to grid. 
	 */
	override protected GridPosition SnapToGrid() {
		GridPosition pos = grid.WorldPositionToGridPosition(myPosition);
		Vector3 position = grid.GridPositionToWorldPosition(pos);
		widget.depth = 1000 - (int)position.z;
		position.z = target.localPosition.z;
		target.localPosition = position;
		return pos;
	}

	/**
	 * Can we drag this object.
	 */
	override protected bool CanDrag {
		get {
			if (building.State == BuildingState.PLACING) return true;
			if (building.State == BuildingState.MOVING) return true;
			return false;
		}
		private set {
			// Do nothing
		}
	}

	/**
	 * Building state changed, update view.
	 */ 
	public void UI_UpdateState() {
		switch (building.State) {
			case BuildingState.IN_PROGRESS :
				progressIndicator.gameObject.SetActive(true);
				buildingSprite.color = Color.white;
				buildingSprite.spriteName = "InProgress2x2";
				progressIndicator.gameObject.SetActive(true);
				currentActivity.color = UIColor.DESATURATE;
				progressRings[0].color = UIColor.BUILD;
				foreach (UIFilledSprite progressRing in progressRings) {
					progressRing.fillAmount = 0.0f;
				}
				currentActivity.spriteName = "build_icon";
				break;
			case BuildingState.READY :
				progressIndicator.gameObject.SetActive(true);
				currentActivity.color = Color.white;
				progressRings[0].color = UIColor.BUILD;
				foreach (UIFilledSprite progressRing in progressRings) {
					progressRing.fillAmount = 1.0f;
				}
				StartCoroutine("DoBobble");
				break;
			case BuildingState.BUILT :
				progressIndicator.gameObject.SetActive(false);
				buildingSprite.color = Color.white;
				buildingSprite.spriteName = building.Type.spriteName;
				buildingSprite.color = Color.white;
				break;
			case BuildingState.MOVING :
				SetColor (building.Position);
				break;
		}
	}

	/**
	 * Activity completed.
	 */ 
	public void UI_StartActivity(Activity activity) {
		if (activity.Type != ActivityType.BUILD) {
			progressIndicator.gameObject.SetActive(true);
			StopCoroutine("DoBobble");
			currentActivity.color = UIColor.DESATURATE;
			progressRings[0].color = UIColor.GetColourForActivityType (activity.Type);
			foreach (UIFilledSprite progressRing in progressRings) {
				progressRing.fillAmount = activity.PercentageComplete;
			}
			currentActivity.spriteName = activity.Type.ToString().ToLower() + "_icon";
		}
	}


	/**
	 * Indicate progress on the progress ring.
	 */
	public void UI_UpdateProgress(Activity activity) {
		foreach (UIFilledSprite progressRing in progressRings) {
			progressRing.fillAmount = activity.PercentageComplete;
		}
	}

	/**
	 * Activity completed.
	 */ 
	public void UI_CompleteActivity(string type) {
		progressIndicator.gameObject.SetActive(true);
		currentActivity.color = Color.white;
		progressRings[0].color = UIColor.GetColourForActivityType(type);
		foreach (UIFilledSprite progressRing in progressRings) {
			progressRing.fillAmount = 1.0f;
		}
		StartCoroutine("DoBobble");
	}
	
	/**
	 * Store of auto generated rewards is full.
	 */ 
	public void UI_StoreFull() {
		if (!building.ActivityInProgress) {
			progressIndicator.gameObject.SetActive(true);
			currentActivity.spriteName = building.Type.generationType.ToString().ToLower() + "_icon";
			currentActivity.color = Color.white;
			progressRings[0].color = UIColor.GetColourForRewardType(building.Type.generationType);
			foreach (UIFilledSprite progressRing in progressRings) {
				progressRing.fillAmount = 1.0f;
			}
			StartCoroutine("DoBobble");
		}
	}
	
	/**
	 * Activity acknowledged.
	 */ 
	public void UI_AcknowledgeActivity() {
		if (!building.ActivityInProgress) {
			if (building.StoreFull) UI_StoreFull ();		
			else progressIndicator.gameObject.SetActive(false);	
		}
	}

	/**
	 * After object dragged set color
	 */ 
	override protected void PostDrag (GridPosition pos) {
		if (building.State != BuildingState.MOVING) {
			building.Position = pos;
		} else {
			building.MovePosition = pos;
		}
		SetColor (pos);
	}

	virtual protected void SetColor(GridPosition pos) {
		if (BuildingModeGrid.GetInstance ().CanObjectBePlacedAtPosition (building, pos)) {
			buildingSprite.color = UIColor.PLACING_COLOR;
		} else {
			buildingSprite.color = UIColor.PLACING_COLOR_BLOCKED;
		}
	}
	
	private IEnumerator DoBobble() {
		while (progressIndicator.gameObject.activeInHierarchy) {
			iTween.PunchPosition(progressIndicator.gameObject, new Vector3(0, 0.035f, 0), 1.5f);
			yield return new WaitForSeconds(Random.Range (1.0f, 1.5f));
		}
	}
}
