using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchPoint : MonoBehaviour {

	// Pass a click on to the immediate parent object

	IInteractableObject touchable;

	void Awake()
	{
		touchable = GetComponentInParent<IInteractableObject>();
	}

	void OnMouseDown()
	{
		if (!GameEngine.Current.IsPlaying()) return;

		touchable.Touched();
	}

	void OnMouseUp()
	{
		if (!GameEngine.Current.IsPlaying()) return;
		
		touchable.TouchEnded();
	}
}
