using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchPoint : MonoBehaviour {

	// Pass a click on to the immediate parent object

	public class TouchPointEventArgs
	{
		public bool Handled = false;
	}

	IInteractableObject touchable;

	void Awake()
	{
		touchable = GetComponentInParent<IInteractableObject>();
	}

	public void GotMouseDown(TouchPointEventArgs args)
	{
		if (!GameEngine.Current.IsPlaying()) return;
		args.Handled = touchable.Touched();
	}

	public void GotMouseUp()
	{
		if (!GameEngine.Current.IsPlaying()) return;
		touchable.TouchEnded();
	}

	// void OnMouseDown()
	// {
	// 	GotMouseDown();
	// }

	// void OnMouseUp()
	// {
	// 	GotMouseUp();
	// }
}
