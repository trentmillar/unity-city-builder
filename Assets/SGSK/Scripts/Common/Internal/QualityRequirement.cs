using UnityEngine;

/// <summary>
/// Place on an object you wish to destroy if the application quality level is less than expected.
/// </summary>

[AddComponentMenu("Common/Quality Requirement")]
public class QualityRequirement : MonoBehaviour
{
	public QualityLevel minimumQuality = QualityLevel.Good;

	void Start ()
	{
		if ((int)QualitySettings.currentLevel < (int)minimumQuality)
		{
			Destroy(gameObject);
		}
	}
}