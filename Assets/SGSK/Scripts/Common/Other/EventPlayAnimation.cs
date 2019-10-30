using UnityEngine;

/// <summary>
/// TODO: Get rid of this class, or replace it with a SendMessage-based approach.
/// </summary>

[AddComponentMenu("Common/Event Listener - Play Animation")]
public class EventPlayAnimation : EventListener
{
	override protected void Trigger ()
	{
		Animation anim = animation;

		if (anim != null && !anim.isPlaying)
		{
			anim.Play();
		}
	}
}