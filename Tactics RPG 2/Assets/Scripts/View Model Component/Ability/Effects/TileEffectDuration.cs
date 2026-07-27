using UnityEngine;
using System.Collections;

public class TileEffectDuration : MonoBehaviour
{
	public int duration = 3;

	void OnEnable ()
	{
		this.AddObserver(OnRoundBegan, TurnOrderController.RoundBeganNotification);
	}

	void OnDisable ()
	{
		this.RemoveObserver(OnRoundBegan, TurnOrderController.RoundBeganNotification);
	}

	void OnRoundBegan (object sender, object args)
	{
		duration--;
		if (duration <= 0)
			Destroy(gameObject);
	}
}
