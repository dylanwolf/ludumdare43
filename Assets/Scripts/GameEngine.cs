using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEngine : MonoBehaviour {

    public WorkerEgg SelectedWorker;
    public GameState CurrentState = GameState.Playing;
	
    public static GameEngine Current;

    public float LongtouchTime = 0.5f;
    public float EggProcessingTime = 5.0f;
    public int ProcessedEggQuantity = 12;

    
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

    public Dictionary<ResourceType, int> ResourceQuantity = new Dictionary<ResourceType, int>();

    public ResourceCount[] ResourceDisplays;

    void InitResources()
    {
        ResourceQuantity.Clear();
        foreach (ResourceType rt in System.Enum.GetValues(typeof(ResourceType)))
            ResourceQuantity[rt] = 0;
    }

    public void ResetState()
    {
        SelectedWorker = null;
        CurrentState = GameState.Playing;
        foreach (var key in ResourceQuantity.Keys)
        {
            ResourceQuantity[key] = 0;
            ResourceDisplays[(int)key].UpdateValue(0);
        }
    }

    public enum GameState
    {
        Playing
    }

    void Awake()
    {
        Current = this;
        InitResources();
        ResetState();
    }

    public bool IsPlaying()
    {
        return CurrentState == GameState.Playing;
    }

    public void UpdateResource(ResourceType resource, int quantity)
    {
        ResourceQuantity[resource] += quantity;
        ResourceDisplays[(int)resource].UpdateValue(ResourceQuantity[resource]);
    }

    public bool CanSpendResource(ResourceType resource)
    {
        return ResourceQuantity[resource] > 0;
    }

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

    public void HarvestWorker(WorkerEgg victim)
    {
        if (SelectedWorker != null)
            SelectedWorker.BeginEggHarvest(victim);
    }

    public bool PotentiallyHarvestingEgg(WorkerEgg newTarget)
    {
        // If the selected worker can be reassigned,
        // and is not the same as the new target,
        // have the new target process as a potential kill
        return (SelectedWorker != null &&
            SelectedWorker != newTarget &&
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

}
