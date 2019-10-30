using UnityEngine;

/// <summary>
/// Change the shader level-of-detail to match the quality settings.
/// Also allows changing of the quality level from within the editor without having
/// to open up the quality preferences, seeing the results right away.
/// In the future this class will also auto-adjust the quality depending on framerate.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("Common/Performance Monitor")]
public class PerformanceMonitor : MonoBehaviour
{
	public QualityLevel qualityLevel = QualityLevel.Fantastic;

	void Update ()
	{
		if (qualityLevel != QualitySettings.currentLevel)
		{
			QualitySettings.currentLevel = qualityLevel;
			Shader.globalMaximumLOD = ((int)qualityLevel + 1) * 100;
		}
	}
}
