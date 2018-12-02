using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Workstation : IInteractableObject {

	public WorkerEgg AssignedWorker;

	public override bool Touched()
	{
		// If no worker assigned, assign a worker
		// (Worker and GameEngine will determine eligibility)
		if (AssignedWorker == null)
			return GameEngine.Current.SendSelectedWorkerTo(this);
		return false;
	}

    public override void WorkerInteraction(WorkerEgg worker)
    {
		// If touched by the assigned workstation, process state changes
        if (worker.Destination == this)
			worker.TouchedWorkstation();
    }

	Transform _workerTargetTransform;
	BoxCollider2D _workerTargetCollider;

	void Start()
	{
		_workerTargetTransform = transform.Find("WorkerInteraction").transform;
		_workerTargetCollider = _workerTargetTransform.GetComponent<BoxCollider2D>();
	}

	public Vector3 GetWalkPoint()
	{
		return _workerTargetTransform.position + (Vector3)_workerTargetCollider.offset;
	}
}
