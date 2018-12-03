using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSMouseInput : MonoBehaviour {

	public LayerMask TargetLayers;
	RaycastHit2D[] hits = new RaycastHit2D[10];
	int hitCount;
	GameObject mouseDownOver = null;
	GameObject touchDownOver = null;
	TouchPoint.TouchPointEventArgs args = new TouchPoint.TouchPointEventArgs();

	void Update()
	{
		if (GameEngine.Current.IsPlaying())
		{
			// Determine if we got a mouse button down
			if (Input.GetMouseButtonDown(0) && GetHits(Input.mousePosition))
				ProcessHits(false);
			// If we were mousing down, detect a mouse up
			else if (mouseDownOver != null && !Input.GetMouseButton(0))
			{
				mouseDownOver.SendMessage("GotMouseUp", SendMessageOptions.DontRequireReceiver);
				mouseDownOver = null;
			}

			// If no mouse detected, look for touch
			if (Input.touchCount > 0)
			{
				var touch = Input.GetTouch(0);
				if (touch.phase == TouchPhase.Began && !Input.GetMouseButton(0) && GetHits(touch.position))
					ProcessHits(true);
				else if (touchDownOver != null && (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended))
				{
					touchDownOver.SendMessage("GotMouseUp", SendMessageOptions.DontRequireReceiver);
					touchDownOver = null;
				}
			}
			else if (touchDownOver != null)
			{
				touchDownOver.SendMessage("GotMouseUp", SendMessageOptions.DontRequireReceiver);
				touchDownOver = null;
			}
		}
	}

	bool GetHits(Vector3 pos)
	{
		hitCount = Physics2D.RaycastNonAlloc(
			Camera.main.ScreenToWorldPoint(pos),
			Vector3.forward,
			hits
		);

		return hitCount > 0;
	}

	void ProcessHits(bool isTouch)
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
					if (isTouch)
						touchDownOver = hits[i].collider.gameObject;
					else
						mouseDownOver = hits[i].collider.gameObject;
					break;
				}
			}
		}
	}
}
