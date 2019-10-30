using UnityEngine;
using System.Collections;

public class MakeTransparent : MonoBehaviour {
    private Shader oldShader = null;
    private Color oldColor = Color.black;
    private float transparency = 0.4f;
    private const float targetTransparency = 0.4f;
    public float FadeSpeed = 0.1f;

    public void DoTransparency() {
        transparency = targetTransparency;

        if (oldShader == null) {
            // Keep references to the old shaders so that they can be restored later.
            oldShader = renderer.material.shader;
            oldColor = renderer.material.color;

            renderer.material.shader = Shader.Find("Transparent/Diffuse"); // Set the object to be ghosted's shader to Transparent/Diffuse,
                                                                           // which is a simple transparency shader.
        }
    }
    
    void Update() {
        if (transparency < 1) {
            Color color = renderer.material.color;
            color.a = transparency;          // Set the object's material's alpha to the target transparency...
            renderer.material.color = color; // ...and then put the changed material back on the object.
        } else {
            renderer.material.shader = oldShader; // Otherwise, restore the old shader.
            renderer.material.color = oldColor;

            Destroy(this); // And remove this script from the renderer.
        }

        transparency += ((1.0f - targetTransparency) * Time.deltaTime) / FadeSpeed; // Increase the transparency.
    }
}
