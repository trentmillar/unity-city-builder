using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// User interface for handling zooming in and out.
/// </summary>
public class UIZoomControl : MonoBehaviour {
	
	#region private members
	
	/// <summary>
	/// The finger one identifier.
	/// </summary>
	private int fingerOneId;
	
	/// <summary>
	/// The finger two identifier.
	/// </summary>
	private int fingerTwoId;
	#endregion
	
	#region public members
	
	/// <summary>
	/// The camera whoose ortho distance will change to zoom in or out.
	/// </summary>
	public Camera zoomCamera;
	
	/// <summary>
	/// A list of panels that are affected by the zoom.
	/// </summary>
	public List<UIPanel> panels;
	
	public float min;

	public float max;

	/// <summary>
	/// The zoom factor.
	/// </summary>
	public float zoomFactor = 0.125f;
	
	/// <summary>
	/// The zoom factor used on touch devices.
	/// </summary>
	public float touchZoomFactor = 2f;
	
	#endregion

	#region private methods
	
	/// <summary>
	/// Does the zoom by changing ortho size.
	/// </summary>
	/// <param name='direction'>
	/// Amount to zoom.
	/// </param>
	private void DoZoom(float direction) {
		zoomCamera.orthographicSize *= (1.0f + direction);
		if (zoomCamera.orthographicSize < min) {
			zoomCamera.orthographicSize = min;
		} else if  (zoomCamera.orthographicSize>max) {
			zoomCamera.orthographicSize = max;
		}
		zoomCamera.GetComponent<UIDraggableCamera>().scale = Vector3.one * zoomCamera.orthographicSize;

	}
	
	#endregion
	
	#region unity hooks
	
	/// <summary>
	/// Unity update, function Check for Touch or scroll
	/// </summary>
	void Update () {
		if (Input.touchCount >= 2) {
			// Stop dragging whilst zooming - we could cahce the draggable camera seems to be okay for now
			zoomCamera.GetComponent<UIDraggableCamera>().enabled = false;
		} else {
			// Start it again
			zoomCamera.GetComponent<UIDraggableCamera>().enabled = true;
		}
		// In zoom mode
		if (fingerOneId != -1 && fingerTwoId != -1) {
			// If either finger released stopped zoom
			if (Input.touchCount != 2) {
				fingerOneId = -1;
				fingerTwoId = -1;
			} else {
				// We are sill zooming process zoom	
				Vector2 pointOne = Input.GetTouch(0).position;
				Vector2 pointTwo = Input.GetTouch(1).position;
				Vector2 previousPointOne = Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition;
				Vector2 previousPointTwo = Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition;
				
				// Distance to zoom
				float zoomAmount = (pointOne - pointTwo).magnitude / (previousPointOne - previousPointTwo).magnitude;
				Vector3 start = zoomCamera.ScreenToWorldPoint(Input.mousePosition);
				DoZoom((1 - zoomAmount) * touchZoomFactor );
				Vector3 end = zoomCamera.ScreenToWorldPoint(Input.mousePosition);
				zoomCamera.transform.Translate(start-end);			
			}
		} else {
			// Start zoom
			if (Input.touchCount == 2 && 
			    (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Began ) && 
			    (Input.GetTouch(1).phase == TouchPhase.Moved ||  Input.GetTouch(0).phase == TouchPhase.Began))  {
				fingerOneId = Input.GetTouch(0).fingerId;
				fingerTwoId = Input.GetTouch(1).fingerId;
			}
		}
		
		// Scroll
		if (Input.GetAxis("Mouse ScrollWheel") != 0) {
			Vector3 start = zoomCamera.ScreenToWorldPoint(Input.mousePosition);
			DoZoom(Input.GetAxis("Mouse ScrollWheel") * zoomFactor);
			Vector3 end = zoomCamera.ScreenToWorldPoint(Input.mousePosition);
			zoomCamera.transform.Translate(start-end);
		}
	}
	
	/// <summary>
	/// Unity awake hook, set the instance.
	/// </summary>
	void Awake(){
		Instance = this;	
	}
	
	#endregion
	
	#region public methods
	
	/// <summary>
	/// Reset the scale back to 1.
	/// </summary>
	public void Reset() {
		zoomCamera.orthographicSize = 1;
	}
	

	
	#endregion
	
	#region public static members
	
	/// <summary>
	/// Gets the instance of the zoom contorl.
	/// </summary>
	public static UIZoomControl Instance {
		get; private set;	
	}

	#endregion
}
