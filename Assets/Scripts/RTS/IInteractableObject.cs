using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IInteractableObject : IPoolable {

	public abstract void Touched();
	public virtual void TouchEnded() {}
	public abstract void WorkerInteraction(WorkerEgg worker);
}
