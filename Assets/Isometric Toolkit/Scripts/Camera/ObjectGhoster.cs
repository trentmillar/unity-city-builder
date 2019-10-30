using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Isometric Toolkit/Camera/Ghoster")]
[RequireComponent(typeof(Camera))]
public class ObjectGhoster : MonoBehaviour {
    public float FadeSpeed = 0.1f;
    public LayerMask GhostLayers = -1; // The layers to ghost. Defaults to everything, but you should uncheck any layers that you don't want ghosted.
    public List<Transform> Targets; // The list of important objects. Anything between them and the camera will be ghosted.
    public bool ImpreciseMode = true; // Raycast towards the object's origin instead of center. Slightly faster and doesn't require the object to have a renderer.

    void Update() {
        foreach (Transform target in Targets) { // For each target:
            if (!ImpreciseMode && !target.renderer) {
                Debug.LogError("Target doesn't have a renderer! Switch to imprecise ghosting if you need to have targets without renderers.");
            }

            var originPos = transform.position;
            var targetPos = (ImpreciseMode ? target.position : target.renderer.bounds.center);
			
//			var target2 = GameObject.Find("Character").transform;
//          targetPos = (ImpreciseMode ? target2.position : target2.renderer.bounds.center);

            var direction = targetPos - originPos;
            var distance = Vector3.Distance(originPos, targetPos);
			
            RaycastHit[] hits = Physics.RaycastAll(originPos, direction, distance, GhostLayers); // Raycast from the camera to the target.

            foreach (var hit in hits) {
				if (!Targets.Contains(hit.transform)) {
	                Renderer r = hit.collider.renderer; // Get collided object's renderer.

	                if (r != null) {
	                    var mt = r.GetComponent<MakeTransparent>(); // Get the renderer's MakeTransparent component.

	                    if (mt == null) { // If it doesn't have one, add one.
	                        mt = r.gameObject.AddComponent<MakeTransparent>();
	                    }

	                    mt.FadeSpeed = FadeSpeed;
	                    mt.DoTransparency(); // Do the transparency.
	                }
				}
            }
        }
    }
}
