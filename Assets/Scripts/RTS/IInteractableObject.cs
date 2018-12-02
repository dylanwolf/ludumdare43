using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IInteractableObject : IPoolable {

	public abstract bool Touched();
	public virtual void TouchEnded() {}
	public abstract void WorkerInteraction(WorkerEgg worker);
}
