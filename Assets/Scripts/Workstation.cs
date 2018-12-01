using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Workstation : IInteractableObject {

	public WorkerEgg AssignedWorker;

	public override void Touched()
	{
		// If no worker assigned, assign a worker
		// (Worker and GameEngine will determine eligibility)
		if (AssignedWorker == null)
			GameEngine.Current.SendSelectedWorkerTo(this);
	}

    public override void WorkerInteraction(WorkerEgg worker)
    {
		// If touched by the assigned workstation, process state changes
        if (worker.Destination == this)
			worker.TouchedWorkstation();
    }
}
