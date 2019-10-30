using UnityEngine;
using System.Collections;

public class ExampleGUI : MonoBehaviour {
    public Transform targetTransform;
    public Transform cameraTransform;

    private CameraMovement cameraMovement;
    private ObjectGhoster objectGhoster;

	void Start() {
	   cameraMovement = targetTransform.GetComponent<CameraMovement>();
       objectGhoster = cameraTransform.GetComponent<ObjectGhoster>();
	}

    void OnGUI() {
		//return;
        GUI.Box(new Rect(15, 1, 330, 69), "Camera");

        GUI.BeginGroup(new Rect(20, 20, 100, 45));
        GUI.Box(new Rect(0, 0, 100, 45), "Base Speed");
        cameraMovement.BaseMovementMultiplier = GUI.HorizontalSlider(new Rect(10, 25, 80, 75), cameraMovement.BaseMovementMultiplier, 0, 30);
        GUI.EndGroup();

        GUI.BeginGroup(new Rect(130, 20, 100, 45));
        GUI.Box(new Rect(0, 0, 100, 45), "Scroll Speed");
        cameraMovement.ScrollMultiplier = (int) GUI.HorizontalSlider(new Rect(10, 25, 80, 75), cameraMovement.ScrollMultiplier, 0, 30);
        GUI.EndGroup();

        GUI.BeginGroup(new Rect(240, 20, 100, 45));
        GUI.Box(new Rect(0, 0, 100, 45), "Edge Speed");
        cameraMovement.ScreenEdgeMovementMultiplier = (int) GUI.HorizontalSlider(new Rect(10, 25, 80, 75), cameraMovement.ScreenEdgeMovementMultiplier, 0, 10);
        GUI.EndGroup();

        GUI.Box(new Rect(Screen.width - 235, 1, 220, 69), "Ghosting");

        GUI.BeginGroup(new Rect(Screen.width - 120, 20, 100, 45));
        GUI.Box(new Rect(0, 0, 100, 45), "Fade Speed");
        objectGhoster.FadeSpeed = 0.5f - GUI.HorizontalSlider(new Rect(10, 25, 80, 75), 0.5f - objectGhoster.FadeSpeed, 0.01f, 0.5f);
        GUI.EndGroup();

        GUI.BeginGroup(new Rect(Screen.width - 230, 20, 100, 45));
        GUI.Box(new Rect(0, 0, 100, 45), "Imprecise Mode");
        objectGhoster.ImpreciseMode = GUI.Toggle(new Rect(10, 20, 80, 75), objectGhoster.ImpreciseMode, "Enabled");
        GUI.EndGroup();

        GUI.BeginGroup(new Rect(20, Screen.height - 65, Screen.width - 40, 45));
        GUI.Box(new Rect(0, 0, Screen.width - 40, 45), "Instructions");
        GUI.skin.label.alignment = TextAnchor.UpperCenter;
        GUI.Label(new Rect(5, 20, Screen.width - 45, 45), "Use the movement keys or the mouse to move. Drag with middle mouse to rotate. " +
            "Double-click middle mouse to reset rotation.");
        GUI.skin.label.alignment = TextAnchor.UpperLeft;
        GUI.EndGroup();

        //if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height - 115, 200, 40), "Buy Isometric Toolkit ($10)"))
        //    Application.OpenURL("http://u3d.as/content/trinomial-games/isometric-toolkit");
    }
}
