using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEngine : MonoBehaviour {
    [Header("Global Configuration")]
    public float LongtouchTime = 0.5f;
    public float EggSpeed = 5.0f;
    public float EggProcessingTime = 5.0f;
    public int ProcessedEggQuantity = 12;
    [Header("UI Elements")]
    public ResourceCount[] ResourceDisplays;

    [Header("In-Game Data")]
    public WorkerEgg SelectedWorker;
    public GameState CurrentState = GameState.Playing;

    void Awake()
    {
        Current = this;
        InitResources();
        ResetState();
    }


    #region Selections
    public void SelectWorker(WorkerEgg egg)
    {
        if (egg == SelectedWorker)
            return;

        if (SelectedWorker != null)
            SelectedWorker.DeselectWorker();

        Debug.Log(string.Format("Selecting worker {0}", egg.gameObject.name));
        SelectedWorker = egg;
        egg.SelectWorker();
    }

    public void DeselectWorker()
    {
        if (SelectedWorker == null)
            return;

        SelectedWorker.DeselectWorker();
        SelectedWorker = null;
    }

    #endregion
	
    #region Game State
    public static GameEngine Current;

    public enum GameState
    {
        Playing
    }

    public void ChangeState(GameState state)
    {
        CurrentState = state;
    }

    public bool IsPlaying()
    {
        return CurrentState == GameState.Playing;
    }

    public void ResetState()
    {
        DeselectWorker();
        ResetResources();
        ChangeState(GameState.Playing);
    }
    #endregion

    #region Resource Tracking
    public enum ResourceType : int
    {
        Egg = 0,
        Peppers = 1,
        Tomato = 2,
        Onion = 3,
        Mushrooms = 4,
        Bacon = 5,
        Cheese = 6
    }

    Dictionary<ResourceType, int> ResourceQuantity;


    void InitResources()
    {
        ResourceQuantity = new Dictionary<ResourceType, int>();
        foreach (ResourceType rt in System.Enum.GetValues(typeof(ResourceType)))
            ResourceQuantity[rt] = 0;
    }

    void ResetResources()
    {
        foreach (ResourceType rt in System.Enum.GetValues(typeof(ResourceType)))
        {
            ResourceQuantity[rt] = 0;
            ResourceDisplays[(int)rt].UpdateValue(0);
        }
    }

    public void UpdateResource(ResourceType resource, int quantity)
    {
        ResourceQuantity[resource] += quantity;
        ResourceDisplays[(int)resource].UpdateValue(ResourceQuantity[resource]);
    }

    public bool CanSpendResource(ResourceType resource, int quantity)
    {
        return (ResourceQuantity[resource] - quantity) >= 0;
    }
    #endregion

    #region Game Commands
    public void HarvestWorker(WorkerEgg victim)
    {
        if (SelectedWorker != null)
            SelectedWorker.BeginEggHarvest(victim);
    }

    public bool PotentiallyHarvestingEgg(WorkerEgg newTarget)
    {
        // If the selected worker can be reassigned,
        // and is not the same as the new target,
        // and is not targeting or being targeted by the target,
        // have the new target process as a potential kill
        return (SelectedWorker != null &&
            SelectedWorker != newTarget &&
            (SelectedWorker.Victim == null || SelectedWorker.Victim != newTarget) &&
            (newTarget.Victim == null || newTarget.Victim != SelectedWorker) &&
            SelectedWorker.CanAssignToResource());
    }

    public void SendSelectedWorkerTo(Workstation workstation)
    {
        if (SelectedWorker != null)
            SelectedWorker.AssignWorker(workstation);
    }

    public void SendSelectedWorkerTo(MaterialSource mat)
    {
        if (SelectedWorker != null)
            SelectedWorker.AssignResource(mat);
    }
    #endregion

    #region Spawns
    // TODO: Track spawners so we can trigger ResetState()
    #endregion
}
