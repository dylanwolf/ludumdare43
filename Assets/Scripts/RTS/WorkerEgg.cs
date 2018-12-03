using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerEgg : IInteractableObject {
    [Header("Configuration")]
    public float SpeedMultiplier = 1.0f;
    public Sprite DeadEggSprite;
    public Sprite EyesSideSprite;
    public Sprite EyesForwardSprite;
    

    [Header("UI Elements")]
    public LongtouchIndicator Longtouch;
    public SpriteRenderer[] SelectedIndicators;
    public SpriteRenderer CarryImage;
    public SpriteRenderer EyesImage;
    public Animator FeetAnimator;
    public Animator ArmAnimator;

    [Header("In-Game Data")]
	public Workstation Destination;
    public MaterialSource Source;
    public Vector3 WalkTarget;
    public WorkerEggState CurrentState = WorkerEggState.Unassigned;
    public WorkerEgg Victim;

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
    
    #region State Tracking
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
    }

    public override void ResetState()
    {
        Source = null;
        Destination = null;
        CurrentState = WorkerEggState.Unassigned;
        Victim = null;
        selectingForHarvest = false;
        Longtouch.gameObject.SetActive(false);
        DeselectWorker();
        UpdateUI();
    }

    public override void ClearState()
    {
        ResetState();
    }

    void ChangeState(WorkerEggState state)
    {
        CurrentState = state;
        UpdateUI();
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
    #endregion

    #region Movement States
    bool IsWalking()
    {
        return CurrentState == WorkerEggState.Assigning ||
            CurrentState == WorkerEggState.HarvestingPantry ||
            CurrentState == WorkerEggState.HarvestingEgg ||
            CurrentState == WorkerEggState.RetrievingPantry ||
            CurrentState == WorkerEggState.RetrievingEgg;
    }

    void WalkTowardTarget()
    {
        if (selectingForHarvest)
            return;

        // If the worker is being assigned to a workstation or collecting a resource from the pantry, have it walk
        if (IsWalking())
        {
            // Follow a victim if we're harvesting another egg
            if (Victim != null)
                WalkTarget = Victim.transform.position;

            var leftToMove = WalkTarget - transform.position;
            var frameMovement = Vector3.Normalize(leftToMove) * SpeedMultiplier * GameEngine.Current.EggSpeed * Time.deltaTime;
            var actualMovement = (frameMovement.magnitude < leftToMove.magnitude) ? frameMovement : leftToMove;
            transform.Translate(actualMovement);
            UpdateUI();
        }
    }
    #endregion
    
    #region Events

    public void SelectWorker()
    {
        for (int i = 0; i < SelectedIndicators.Length; i++)
            SelectedIndicators[i].enabled = true;
    }

    public void DeselectWorker()
    {
        for (int i = 0; i < SelectedIndicators.Length; i++)
            SelectedIndicators[i].enabled = false;
    }

    public override bool Touched()
    {
        if (GameEngine.Current.PotentiallyHarvestingEgg(this))
        {
             StartCoroutine(ProcessLongtouch());
             return true;
        }
        else
        {
             return GameEngine.Current.SelectWorker(this);
        }
        
    }

    public override void TouchEnded()
    {
        selectingForHarvest = false;
    }

    public override void WorkerInteraction(WorkerEgg worker)
    {
        if (worker == Victim)
        {
            SoundBoard.Current.PlayEggDie();
            Victim.Killed();
            EndEggHarvest();
        }
    }
    #endregion

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

    public void BeginEggHarvest(WorkerEgg egg)
    {
        WalkTarget = egg.transform.position;
        ChangeState(WorkerEggState.HarvestingEgg);
        Victim = egg;
    }

    public void EndEggHarvest()
    {
        WalkTarget = Destination.GetWalkPoint();
        ChangeState(WorkerEggState.RetrievingEgg);
        Victim = null;
    }

    public void Killed()
    {
        if (Destination != null)
            Destination.AssignedWorker = null;

        this.Despawn();
    }
    #endregion


    #region Player Input
    public bool AssignWorker(Workstation workstation)
    {
        // If we're in a state where we can assign to a workstation, go from Unassigned to Assigning
        if (CanAssignToWorkstation())
        {
            Debug.Log(string.Format("Assigning worker {0} to station {1}", gameObject.name, workstation.gameObject.name));
            WalkTarget = workstation.GetWalkPoint();
            ChangeState(WorkerEggState.Assigning);
            Destination = workstation;
            return true;
        }

        return false;
    }

    public bool AssignResource(MaterialSource mats)
    {
        // If we assign to a new resource, go from any state to Harvesting
        if (CanAssignToResource() && mats != Source)
        {
            Debug.Log(string.Format("Worker {0} assigned to resource {1}", gameObject.name, mats.gameObject.name));
            if (Victim != null) Victim = null;
            Source = mats;
            DoPantryHarvest();
            return true;
        }

        return false;
    }
    #endregion

    #region Automatic Actions
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

    void DoPantryHarvest()
    {
        WalkTarget = Source.GetWalkPoint();
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
            WalkTarget = Destination.GetWalkPoint();
        }
    }

    IEnumerator DoProcessing()
    {
        SoundBoard.Current.PlayEggProcess();

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
            SpawnResourceParticle(Source.Resource, Source.ProcessingAmount);
            DoPantryHarvest();
        }
        else if (CurrentState == WorkerEggState.ProcessingEgg)
        {
            SpawnResourceParticle(GameEngine.ResourceType.Egg, GameEngine.Current.ProcessedEggQuantity);
            ChangeState(WorkerEggState.Waiting);

            // Go back to what we were doing
            if (Source != null)
                DoPantryHarvest();
        }
    }

    void SpawnResourceParticle(GameEngine.ResourceType resource, int quantity)
    {
        Debug.Log(string.Format("Spawning particle for {0} with quantity {1}", resource.ToString(), quantity));
        ObjectPooler.Current.Spawn<ResourceGainParticle>("ResourceGainParticle", x => {
            x.Source = this.transform.position;
            x.Destination = GameEngine.Current.ResourceDisplays[(int)resource].GetWorldPosition();
            x.Resource = resource;
            x.PercentTraveled = 0;
            x.Quantity = quantity;
            x.SetSprite();
        });
    }

    float GetAnimationX(Vector3 movement)
    {
        return Mathf.Abs(movement.x) >= Mathf.Abs(movement.y) ? Mathf.Sign(movement.x) : 0;
    }

    float GetAnimationY(Vector3 movement)
    {
        return Mathf.Abs(movement.y) > Mathf.Abs(movement.x) ? Mathf.Sign(movement.y) : 0;
    }

    bool IsFlippedX(Vector3 movement)
    {
        return GetAnimationX(movement) > 0;
    }

    bool IsFacingForward(Vector3 movement)
    {
        return GetAnimationY(movement) < 0;
    }

    bool IsFacingBackward(Vector3 movement)
    {
        return GetAnimationY(movement) > 0;
    }

    const string ANIM_IS_WAITING = "IsWaiting";
    const string ANIM_IS_WORKING = "IsWorking";
    const string ANIM_IS_CARRYING = "IsCarrying";
    const string ANIM_X_DIRECTION = "XDirection";
    const string ANIM_Y_DIRECTION = "YDirection";

    public void UpdateUI()
    {
        var movement = IsWalking() ?(WalkTarget - this.transform.position) : Vector3.zero;

        // Set carry object
        if (CurrentState == WorkerEggState.RetrievingEgg)
        {
            CarryImage.enabled = true;
            CarryImage.sprite = DeadEggSprite;
        }
        else if (CurrentState == WorkerEggState.RetrievingPantry)
        {
            CarryImage.enabled = true;
            CarryImage.sprite = GameEngine.Current.IngredientIcons[(int)Source.Resource];
        }
        else
        {
            CarryImage.enabled = false;
        }

        // Set eyes
        if (CurrentState == WorkerEggState.ProcessingEgg || CurrentState == WorkerEggState.ProcessingPantry)
        {
            EyesImage.enabled = false;
        }
        else if (CurrentState == WorkerEggState.Waiting || CurrentState == WorkerEggState.Unassigned)
        {
            EyesImage.enabled = true;
            EyesImage.sprite = EyesForwardSprite;
        }
        else
        {
            if (IsFacingForward(movement))
            {
                EyesImage.enabled = true;
                EyesImage.sprite = EyesForwardSprite;
                EyesImage.flipX = false;
            }
            else if (IsFacingBackward(movement))
            {
                EyesImage.enabled = false;
            }
            else
            {
                EyesImage.enabled = true;
                EyesImage.sprite = EyesSideSprite;
                EyesImage.flipX = IsFlippedX(movement);
            }
        }

        // Set feet
        FeetAnimator.SetBool(ANIM_IS_WAITING, 
            CurrentState == WorkerEggState.Waiting || CurrentState == WorkerEggState.Unassigned);

        FeetAnimator.SetBool(ANIM_IS_WORKING, 
            CurrentState == WorkerEggState.ProcessingEgg || CurrentState == WorkerEggState.ProcessingPantry);

        FeetAnimator.SetFloat(ANIM_X_DIRECTION, GetAnimationX(movement));
        FeetAnimator.SetFloat(ANIM_Y_DIRECTION, GetAnimationY(movement));

        // Set arms
        ArmAnimator.SetBool(ANIM_IS_WAITING, 
            CurrentState == WorkerEggState.Waiting || CurrentState == WorkerEggState.Unassigned);

        ArmAnimator.SetBool(ANIM_IS_WORKING, 
            CurrentState == WorkerEggState.ProcessingEgg || CurrentState == WorkerEggState.ProcessingPantry);

        ArmAnimator.SetBool(ANIM_IS_CARRYING, 
            CurrentState == WorkerEggState.RetrievingEgg || CurrentState == WorkerEggState.RetrievingPantry);

        ArmAnimator.SetFloat(ANIM_X_DIRECTION, GetAnimationX(movement));
        ArmAnimator.SetFloat(ANIM_Y_DIRECTION, GetAnimationY(movement));
    }
    #endregion
}
