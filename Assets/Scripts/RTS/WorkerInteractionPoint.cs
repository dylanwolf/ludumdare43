using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerInteractionPoint : MonoBehaviour {

	// Pass interaction with a WorkerEgg in the immediate parent on to the collider's immediate parent

	WorkerEgg egg;

	void Awake()
	{
		egg = GetComponentInParent<WorkerEgg>();
	}

	//public void OnTriggerEnter2D(Collider2D collider)
	public void OnTriggerStay2D(Collider2D collider)
	{
		Debug.Log(string.Format("Sending message to {0}", collider.transform.parent.name));
		collider.transform.parent.SendMessage("WorkerInteraction", egg, SendMessageOptions.DontRequireReceiver);
	}
}
