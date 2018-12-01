using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSource : IInteractableObject {

    public float ProcessingTime = 1.0f;
    public int ProcessingAmount = 4;
    public GameEngine.ResourceType Resource;

	public override void Touched()
	{
        // Try assinging the selected worker to the resource
        // (Worker and GameEngine will determine eligibility)
		GameEngine.Current.SendSelectedWorkerTo(this);
	}

    public override void WorkerInteraction(WorkerEgg worker)
    {
        // If touched by the worker harvesting from it, complete the harvest action
        if (worker.Source == this)
			worker.HarvestingComplete();
    }
}
