using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerEgg : IInteractableObject {

	public Workstation Destination;
    public MaterialSource Source;
    public LongtouchIndicator Longtouch;
    public SpriteRenderer SelectedIndicator;

    public enum WorkerEggState : int
    {
        Unassigned = 0,
        Assigning = 1,
        Waiting = 2,
        HarvestingPantry = 3,
        RetrievingPantry = 4,
        ProcessingPantry = 5,
        HarvestingEgg = 7,
        RetrievingEgg = 8,
        ProcessingEgg = 9,
        Targeted = 10
    }

    public float Speed = 1.0f;
    public Vector3 WalkTarget;
    public WorkerEggState CurrentState = WorkerEggState.Unassigned;

    void Start()
    {
        ResetState();
    }

    void Update()
    {
        if (GameEngine.Current.IsPlaying())
        {
            WalkTowardTarget();
        }
    }

    void ResetState()
    {
        Source = null;
        Destination = null;
        CurrentState = WorkerEggState.Unassigned;
        Victim = null;
        selectingForHarvest = false;
        Longtouch.gameObject.SetActive(false);
        DeselectWorker();
    }

    void ChangeState(WorkerEggState state)
    {
        CurrentState = state;
    }

    void WalkTowardTarget()
    {
        if (selectingForHarvest)
            return;

        // If the worker is being assigned to a workstation or collecting a resource from the pantry, have it walk
        if (CurrentState == WorkerEggState.Assigning ||
            CurrentState == WorkerEggState.HarvestingPantry ||
            CurrentState == WorkerEggState.HarvestingEgg ||
            CurrentState == WorkerEggState.RetrievingPantry ||
            CurrentState == WorkerEggState.RetrievingEgg)
        {
            var leftToMove = WalkTarget - transform.position;
            var frameMovement = Vector3.Normalize(leftToMove) * Speed * Time.deltaTime;
            var actualMovement = (frameMovement.magnitude < leftToMove.magnitude) ? frameMovement : leftToMove;
            transform.Translate(actualMovement);
        }
    }

    public void SelectWorker()
    {
        SelectedIndicator.enabled = true;
    }

    public void DeselectWorker()
    {
        SelectedIndicator.enabled = false;
    }

    public override void Touched()
    {
        if (GameEngine.Current.PotentiallyHarvestingEgg(this))
        {
             StartCoroutine(ProcessLongtouch());
        }
        else
        {
             GameEngine.Current.SelectWorker(this);
        }
        
    }

    public override void TouchEnded()
    {
        selectingForHarvest = false;
    }

    #region Egg Harvesting
    bool selectingForHarvest = false;
    IEnumerator ProcessLongtouch()
    {
        // Init longtouch indicator control
        Longtouch.SetFrame(0);
        Longtouch.gameObject.SetActive(true);

        // Init coroutine variables
        selectingForHarvest = true;
        var timer = GameEngine.Current.LongtouchTime;
        var lastFrame = 0;

        // Pause to ensure player wants to kill the target
        while (timer > 0 && selectingForHarvest)
        {
            if (!GameEngine.Current.IsPlaying())
                yield return null;

            timer -= Time.deltaTime;
            var frame = Mathf.FloorToInt((1 - (timer / GameEngine.Current.LongtouchTime)) * Longtouch.Frames.Length);
            if (frame != lastFrame)
            {
                Longtouch.SetFrame(frame);
                lastFrame = frame;
            }
            yield return null;
        }

        // If held, kill the target
        if (selectingForHarvest)
        {
            Debug.Log("Selected for harvest");
            GameEngine.Current.HarvestWorker(this);
        }
        // Otherwise, select the target
        else
        {
            GameEngine.Current.SelectWorker(this);
        }

        // Clean up state
        Longtouch.gameObject.SetActive(false);
        selectingForHarvest = false;
    }

    public WorkerEgg Victim;

    public void BeginEggHarvest(WorkerEgg egg)
    {
        ChangeState(WorkerEggState.HarvestingEgg);
        Victim = egg;
        egg.ChangeState(WorkerEggState.Targeted);
        WalkTarget = egg.transform.position;
    }

    public void EndEggHarvest()
    {
        ChangeState(WorkerEggState.RetrievingEgg);
        WalkTarget = Destination.transform.position;
        Victim = null;
    }

    public void Killed()
    {
        // TODO: Replace with object pooling
        if (Destination != null)
            Destination.AssignedWorker = null;

        Destroy(gameObject);
    }
    #endregion

    public override void WorkerInteraction(WorkerEgg worker)
    {
        if (worker == Victim)
        {
            Victim.Killed();
            EndEggHarvest();
        }
    }

    public void AssignWorker(Workstation workstation)
    {
        // If we're in a state where we can assign to a workstation, go from Unassigned to Assigning
        if (CanAssignToWorkstation())
        {
            Debug.Log(string.Format("Assigning worker {0} to station {1}", gameObject.name, workstation.gameObject.name));
            WalkTarget = workstation.transform.position;
            ChangeState(WorkerEggState.Assigning);
            Destination = workstation;
        }
    }

    public void TouchedWorkstation()
    {
        // If we finished an assignment, go from Assigning to Waiting
        if (CurrentState == WorkerEggState.Assigning)
        {
            Debug.Log(string.Format("Worker {0} is waiting", gameObject.name));
            ChangeState(WorkerEggState.Waiting);
            Destination.AssignedWorker = this;
        }
        // If we finished a pantry retrieval, go from Retriving to Processing
        else if (CurrentState == WorkerEggState.RetrievingPantry || CurrentState == WorkerEggState.RetrievingEgg)
        {
            Debug.Log(string.Format("Worker {0} completed assignment", gameObject.name));
            StartCoroutine(DoProcessing());
        }
    }

    bool CanAssignToWorkstation()
    {
        return CurrentState == WorkerEggState.Unassigned;
    }

    public bool CanAssignToResource()
    {
        // We can have a worker change the resource they're harvesting mid-task
        // (but the current chunk of work will be lost)
        return CurrentState == WorkerEggState.Waiting ||
            CurrentState == WorkerEggState.HarvestingPantry ||
            CurrentState == WorkerEggState.HarvestingEgg ||
            CurrentState == WorkerEggState.ProcessingPantry ||
            CurrentState == WorkerEggState.RetrievingPantry;
    }

    public void AssignResource(MaterialSource mats)
    {
        // If we assign to a new resource, go from any state to Harvesting
        if (CanAssignToResource() && mats != Source)
        {
            Debug.Log(string.Format("Worker {0} assigned to resource {1}", gameObject.name, mats.gameObject.name));
            if (Victim != null) Victim = null;
            Source = mats;
            DoPantryHarvest();
        }
    }

    void DoPantryHarvest()
    {
        WalkTarget = Source.transform.position;
        ChangeState(WorkerEggState.HarvestingPantry);
    }

    public void HarvestingComplete()
    {
        // If we touched our pantry source, go from Harvesting to Retrieving (walk back to the workstation)
        if (CurrentState == WorkerEggState.HarvestingPantry || CurrentState == WorkerEggState.HarvestingEgg)
        {
            Debug.Log(string.Format("Worker {0} completed harvesting", gameObject.name));
            ChangeState((CurrentState == WorkerEggState.HarvestingPantry) ?
                WorkerEggState.RetrievingPantry : 
                WorkerEggState.RetrievingEgg);
            WalkTarget = Destination.transform.position;
        }
    }

    IEnumerator DoProcessing()
    {
        // Have the worker process the food they received from the pantry
        float timer = 0;
        if (CurrentState == WorkerEggState.RetrievingPantry)
        {
            ChangeState(WorkerEggState.ProcessingPantry);
            timer = Source.ProcessingTime;
        }
        else
        {
            ChangeState(WorkerEggState.ProcessingEgg);
            timer = GameEngine.Current.EggProcessingTime;
        }
        // If we changed tasks, abandon the current chunk of work
        while (timer > 0 && (CurrentState == WorkerEggState.ProcessingPantry || CurrentState == WorkerEggState.ProcessingEgg))
        {
            if (!GameEngine.Current.IsPlaying())
                yield return null;

            timer -= Time.deltaTime;
            yield return null;
        }

        // If we finished the current chunk of work, add resource to our collection
        // Go from Processing to Harvest, and walk back to the pantry
        if (CurrentState == WorkerEggState.ProcessingPantry)
        {
            // TODO: Animate number flying to the total
            GameEngine.Current.UpdateResource(Source.Resource, Source.ProcessingAmount);
            DoPantryHarvest();
        }
        else if (CurrentState == WorkerEggState.ProcessingEgg)
        {
            // TODO: Add resource
            GameEngine.Current.UpdateResource(GameEngine.ResourceType.Egg, GameEngine.Current.ProcessedEggQuantity);
            ChangeState(WorkerEggState.Waiting);

            // Go back to what we were doing
            if (Source != null)
                DoPantryHarvest();
        }
    }
}
