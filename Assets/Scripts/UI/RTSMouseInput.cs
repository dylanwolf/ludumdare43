using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSMouseInput : MonoBehaviour {

	public LayerMask TargetLayers;
	RaycastHit2D[] hits = new RaycastHit2D[10];
	int hitCount;
	GameObject mouseDownOver = null;
	TouchPoint.TouchPointEventArgs args = new TouchPoint.TouchPointEventArgs();

	void Update()
	{
		if (GameEngine.Current.IsPlaying())
		{
			// Determine if we got a mouse button down
			if (Input.GetMouseButtonDown(0) && GetHits())
				ProcessHits();
			// If we were mousing down, detect a mouse up
			else if (mouseDownOver != null && !Input.GetMouseButton(0))
				mouseDownOver.SendMessage("GotMouseUp", SendMessageOptions.DontRequireReceiver);
		}
	}

	bool GetHits()
	{
		hitCount = Physics2D.RaycastNonAlloc(
			Camera.main.ScreenToWorldPoint(Input.mousePosition),
			Vector3.forward,
			hits
		);

		return hitCount > 0;
	}

	void ProcessHits()
	{
		Debug.Log(string.Format("Got {0} hits", hitCount));
		for (int i = 0; i < hitCount; i++)
		{
			if (hits[i] != null)
			{
				args.Handled = false;
				// Process clicks until something actually does something
				hits[i].collider.SendMessage("GotMouseDown", args, SendMessageOptions.DontRequireReceiver);
				if (args.Handled)
				{
					mouseDownOver = hits[i].collider.gameObject;
					break;
				}
			}
		}
	}
}
