using UnityEngine;
using System;
using System.Collections;
using System.Linq;


	/// <summary>
	/// Camera position relative to terrain.
	/// </summary>
	public enum CameraPosition {
		Over,
		Under,
		Unknown,
		OK
	}

[AddComponentMenu("Isometric Toolkit/Target/Movement")]
public class CameraMovement : MonoBehaviour {
    public float BaseMovementMultiplier = 14; // Controls how fast the camera moves. Affects all types of movement.
    public float ScreenEdgeMovementMultiplier = 1; // An additional multiplier that only affects movement with the mouse.
    public int ScrollMultiplier = 50; // Controls how fast the camera moves up and down when the user scrolls.
    public int MinimumHeight = 15; // The lowest height that the camera can be at.
    public int MaximumHeight = 100; // The highest height.
    public int ScreenBorderSize = 20; // The distance away from the edge of the screen that the user has to move the mouse to move the view.
    public Transform OrbitTarget = null; // The object to orbit around. Setting this to null will fall back on using the ghoster's target list, 
                                         // and that will fall back on using the older, worse method if the ghoster isn't enabled or it doesn't have any targets.

    private Transform cameraTransform; // Stores a reference to the transform of the child camera.
    private Quaternion defaultRotation; // The camera's default rotation. When the user double-clicks the middle mouse button, the rotation reverts to this value.
    private float[] previousMouseMovementX = new float[2]; // The previous two mouse movement values on the X axis. Used for smoothing.
    private float[] previousMouseMovementY = new float[2]; // The previous two mouse movement values on the Y axis. Used for smoothing.
    private int lastXMovementArrayIndex = 0, lastYMovementArrayIndex = 0; // The index in previousMouseMovement that was last written to.
	

	
	void Start() {
        defaultRotation = transform.rotation; // Set the default rotation.

        foreach (Transform child in transform) {
            if (child.camera != null) {
                cameraTransform = child; // Set the camera transform.
                break;
            }
        }
	}
	
	#region Camera Normalization
	
	private const float CameraNormalizationFactor = 0.01f;
	
	CameraPosition GetCameraPosition() {
		RaycastHit hit;
		float distance;
		if (Physics.Raycast(cameraTransform.position, -Vector3.up, out hit, Constants.TerrainLayerMask)) {
			distance = (hit.point - cameraTransform.position).magnitude;
			if(distance < Constants.MinimumCameraFloat) {
				return CameraPosition.Over;
			}
		}
		else {
			// no hit, maybe we are below terrain?
			if (Physics.Raycast(cameraTransform.position, Vector3.up, out hit, Constants.TerrainLayerMask)) {
				distance = (hit.point - cameraTransform.position).magnitude;
				if(distance < Constants.MinimumCameraFloat) {
					return CameraPosition.Under;
				}
			}
			else {
				// camera is above no terrain
				return CameraPosition.Unknown;
			}
		}
		return CameraPosition.OK;
	}
	
	/// <summary>
	/// Normalizes the camera over uneven terrain.
	/// </summary>
	void NormalizeCameraPosition() {
		for(int i=0;;i++){
			switch(GetCameraPosition()) {
				case CameraPosition.OK:
				case CameraPosition.Unknown:
					return;
				case CameraPosition.Over:
				case CameraPosition.Under:
					TranslateTargetPosition(new Vector3(0, CameraNormalizationFactor, 0));
					break;;
			}
		}
		return;
	}
	
	#endregion
	
	#region Camera Rotation (horz & vert)
	
	void DoHorizontalRotation(float movement) {
		var mouseMovementX = movement; // get the amount the mouse has moved on the X axis since last update.
        var averageMovement = previousMouseMovementX.Average(); // Average the amount the mouse moved on the last two updates.

            if (mouseMovementX != 0) {
                if (OrbitTarget != null)
                    transform.RotateAround(OrbitTarget.position, Vector3.up, averageMovement * 3); // Orbit the camera around the target.
                else if (cameraTransform.GetComponent<ObjectGhoster>() != null && cameraTransform.GetComponent<ObjectGhoster>().Targets.Count > 0)
                    transform.RotateAround(cameraTransform.GetComponent<ObjectGhoster>().Targets[0].position, Vector3.up, averageMovement * 3);
                else
                    transform.Rotate(Vector3.up * (averageMovement * 3), Space.World);

				//is under ceiling?
            }
            previousMouseMovementX[lastXMovementArrayIndex] = mouseMovementX; // Store the mouse movement in the previous mouse movement array.
            lastXMovementArrayIndex++;

            if (lastXMovementArrayIndex >= previousMouseMovementX.Length)
                lastXMovementArrayIndex = 0; // Loop back to the start of the array if the index goes over.	
	}
	
	void DoVerticalRotation(float amount){
		var mouseMovementY = amount; // get the amount the mouse has moved on the Y axis since last update.
		var averageMovement = previousMouseMovementY.Average(); // Average the amount the mouse moved on the last two updates.
		
		if (mouseMovementY != 0)
		{
			if (OrbitTarget != null) 
			{
				if( (transform.eulerAngles.x > 10 && mouseMovementY < 0) ||
					(transform.eulerAngles.x < 45 && mouseMovementY > 0) ) 
				{
					var rot = transform.right * averageMovement;					
					if(mouseMovementY < 0){
						rot *= -1;
					}					
					transform.RotateAround(OrbitTarget.position, rot, averageMovement * 3);
				}
			}
			//else if (cameraTransform.GetComponent<ObjectGhoster>() != null && cameraTransform.GetComponent<ObjectGhoster>().Targets.Count > 0)
			//	transform.RotateAround(Vector3.up, cameraTransform.GetComponent<ObjectGhoster>().Targets[0].position, averageMovement * 3);
			else
			{
				//transform.Rotate(Vector3.up * (averageMovement * 3), Space.World);
				throw new NotSupportedException("not sure how to handle rotation");
			}
        }

            previousMouseMovementY[lastYMovementArrayIndex] = mouseMovementY; // Store the mouse movement in the previous mouse movement array.
            lastYMovementArrayIndex++;

            if (lastYMovementArrayIndex >= previousMouseMovementY.Length)
                lastYMovementArrayIndex = 0; // Loop back to the start of the array if the index goes over.	
	}
	
#endregion

    void FixedUpdate() {
        // Orbit the camera.
        if (Input.GetMouseButton(2)) {
			
			var x = Input.GetAxis("Mouse X");
			var y = Input.GetAxis("Mouse Y");
			
			if(Math.Abs(x) > Math.Abs(y)){
				DoHorizontalRotation(x);
			}
			else {
				DoVerticalRotation(y);
			}
			NormalizeCameraPosition();
        }
		
    }

    void OnGUI() {
        var e = Event.current;

        if (e.isMouse) {
            if (e.button == 2 && e.clickCount == 2) {
                transform.rotation = defaultRotation; // If the user double-clicks middle mouse, restore the rotation to default.
            }
        }
    }
	
	void Update() {
        float movementX = Input.GetAxis("Horizontal"); // Store the horizontal
        float movementZ = Input.GetAxis("Vertical");   // and vertical keyboard movement.

        Vector3 mousePosition = Input.mousePosition;   // Store the mouse position.

        if (!Input.GetMouseButton(2)) {
            if (mousePosition.x > Screen.width - ScreenBorderSize)
                movementX += ScreenEdgeMovementMultiplier; // If the mouse's x is near the right edge of the screen, increase the x movement variable.
            else if (mousePosition.x < ScreenBorderSize)
                movementX -= ScreenEdgeMovementMultiplier; // If the mouse's x is near the left edge of the screen, decrease the x movement variable.

            if (mousePosition.y > Screen.height - ScreenBorderSize)
                movementZ += ScreenEdgeMovementMultiplier; // If the mouse's y is near the bottom edge of the screen, increase the z movement variable.
            else if (mousePosition.y < ScreenBorderSize)
                movementZ -= ScreenEdgeMovementMultiplier; // If the mouse's y is near the top edge of the screen, decrease the z movement variable.
        }

        float moveMultiplier = (cameraTransform.camera.orthographic ? cameraTransform.camera.orthographicSize : transform.position.y) > BaseMovementMultiplier ? 
            (cameraTransform.camera.orthographic ? cameraTransform.camera.orthographicSize : transform.position.y) : BaseMovementMultiplier;
        // Big complicated line that calculates a multiplier based on the target's y position (or, in the case of an orthographic camera, the camera's orthographic size).
     
        if (movementX != 0) {
            TranslateTargetPosition(new Vector3(transform.right.x * movementX, 0, transform.right.z * movementX) * Time.deltaTime * moveMultiplier);
            // Move on the x axis if necessary.
        }
     
        if (movementZ != 0) {
            TranslateTargetPosition(new Vector3(transform.forward.x * movementZ, 0, transform.forward.z * movementZ) * Time.deltaTime * moveMultiplier);
            // Move on the z axis if necessary.
        }

        float scrollAmount = Input.GetAxis("Mouse ScrollWheel"); // Get the amount the user has scrolled.
		print (scrollAmount);
        if (scrollAmount != 0) {
            TranslateTargetPosition(new Vector3(0, scrollAmount * ScrollMultiplier * -1.0f, 0));
        }
		
		NormalizeCameraPosition();
        CapCameraY(); // Cap the camera height or orthographic size.
	}

    private void CapCameraY() {
        if (cameraTransform.camera.orthographic) {

            if (cameraTransform.camera.orthographicSize < MinimumHeight)
                cameraTransform.camera.orthographicSize = MinimumHeight;
            else if (cameraTransform.camera.orthographicSize > MaximumHeight)
                cameraTransform.camera.orthographicSize = MaximumHeight;

        } else {

            if (transform.position.y < MinimumHeight)
                transform.position = new Vector3(transform.position.x, MinimumHeight, transform.position.z);
            else if (transform.position.y > MaximumHeight)
                transform.position = new Vector3(transform.position.x, MaximumHeight, transform.position.z);

        }
    }

    /// API STARTS HERE ///

    // Call this method if the rotation of the camera has changed and you want the change to be permanent.
    // If you don't, it will go back to the default rotation when the user double-clicks the middle mouse button.
    public void UpdateDefaultRotation() {
        defaultRotation = transform.rotation;
    }

    // Call this method to move the target (and by extension, the camera) to an absolute position in the world space.
    // It achieves the same effect whether you're using a perspective or orthographic camera.
    public void SetTargetPosition(Vector3 newPosition) {
        if (cameraTransform.camera.orthographic) {
            transform.position = new Vector3(newPosition.x, transform.position.y, newPosition.z);
            cameraTransform.camera.orthographicSize = newPosition.y;
        } else {
            transform.position = newPosition;
        }
    }

    // Call this method to move the target by a specific amount on each axis in the world space.
    // It achieves the same effect whether you're using a perspective or orthographic camera.
    public void TranslateTargetPosition(Vector3 moveBy) {
        if (cameraTransform.camera.orthographic) {
            transform.Translate(new Vector3(moveBy.x, 0, moveBy.z), Space.World);
            cameraTransform.camera.orthographicSize += moveBy.y;
        } else {
            transform.Translate(moveBy, Space.World);
        }
    }
}
