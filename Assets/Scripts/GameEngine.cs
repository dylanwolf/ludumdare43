using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEngine : MonoBehaviour {
    [Header("Global Configuration")]
    public float LongtouchTime = 0.5f;
    public float EggSpeed = 5.0f;
    public float EggProcessingTime = 5.0f;
    public int ProcessedEggQuantity = 4;
    public Sprite[] IngredientIcons;
    public Sprite[] WorkerCarrySprites;
    public float[] TimeBetweenDifficultyIncreases;
    public float[] TimeBetweenNewOrders;
    public int MaximumOrders = 4;
    public float TimePerIngredient = 3.0f;

    [Header("UI Elements")]
    public ResourceCount[] ResourceDisplays;
    public Griddle[] Griddles;
    public EggSpawner[] EggSpawners;
    public Transform OrderDisplayContainer;
    public Text ScoreText;

    [Header("In-Game Data")]
    public WorkerEgg SelectedWorker;
    public Griddle SelectedGriddle;
    public GameState CurrentState = GameState.Playing;
    public int Score = 0;
    public int CurrentDifficulty = 1;
    public List<OrderDisplay> OrderQueue = new List<OrderDisplay>();

    void Awake()
    {
        Current = this;
        InitResources();
    }

    void Start()
    {
        ResetState();
    }

    #region Orders and Difficulty
    
    const string COROUTINE_NEW_ORDERS = "Coroutine_NewOrders";
    const string COROUTINE_DIFFICULTY_INCREASES = "Coroutine_DifficultyIncreases";

    public void DequeueOrder(OrderDisplay order)
    {
        OrderQueue.Remove(order);
    }

    void SpawnOrder(int difficulty)
    {
        if (OrderQueue.Count == MaximumOrders)
            return;

        SoundBoard.Current.PlayOmeletteNewOrder();
        OrderQueue.Add(ObjectPooler.Current.Spawn<OrderDisplay>("OrderDisplay", x => {
            x.transform.SetParent(OrderDisplayContainer);
            x.transform.SetAsFirstSibling();
            x.transform.localScale = Vector3.one;
            x.Populate(OrderData.Generate(difficulty));
        }));
    }

    IEnumerator Coroutine_NewOrders()
    {
        float timer = 0;
        while (true)
        {
            if (!IsPlaying() || OrderQueue.Count == MaximumOrders)
                yield return null;

            timer += Time.deltaTime;
            if (timer >= TimeBetweenNewOrders[CurrentDifficulty-1])
            {
                SpawnOrder(CurrentDifficulty);
                timer = 0;
            }

            yield return null;
        }
    }

    IEnumerator Coroutine_DifficultyIncreases()
    {
        float timer = 0;
        while (true)
        {
            if (!IsPlaying())
                yield return null;

            timer += Time.deltaTime;
            if (timer >= TimeBetweenDifficultyIncreases[CurrentDifficulty-1])
            {
                CurrentDifficulty++;
                timer = 0;
                if (CurrentDifficulty == TimeBetweenDifficultyIncreases.Length)
                    break;
            }

            yield return null;
        }
    }
    #endregion

    #region Selections
    public bool SelectWorker(WorkerEgg egg)
    {
        if (egg == SelectedWorker)
            return false;

        if (SelectedWorker != null)
            SelectedWorker.DeselectWorker();

        Debug.Log(string.Format("Selecting worker {0}", egg.gameObject.name));
        SelectedWorker = egg;
        egg.SelectWorker();
        return true;
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
        StopCoroutine(COROUTINE_DIFFICULTY_INCREASES);
        StopCoroutine(COROUTINE_NEW_ORDERS);

        DeselectWorker();
        DeselectGriddle();
        ResetResources();
        ResetEggSpawners();
        ResetScore();
        ResetGriddles();
        ChangeState(GameState.Playing);
        Score = 0;
        CurrentDifficulty = 1;

        StartCoroutine(COROUTINE_DIFFICULTY_INCREASES);
        StartCoroutine(COROUTINE_NEW_ORDERS);
        SpawnOrder(1);
    }

    void ResetEggSpawners()
    {
        foreach (var spawner in EggSpawners)
        {
            spawner.ResetState();
        }
    }

    void ResetGriddles()
    {
        foreach (var griddle in Griddles)
        {
            griddle.ResetState();
        }
    }

    public void AddScore(int modifier)
    {
        if (modifier != 0)
        {
            Score = Mathf.Clamp(Score + modifier, 0, int.MaxValue);
            UpdateScore();
        }
    }

    void ResetScore()
    {
        Score = 0;
        UpdateScore();
    }

    void UpdateScore()
    {
        ScoreText.text = Score.ToString();
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

    public Dictionary<ResourceType, int> ResourceQuantity;


    void InitResources()
    {
        ResourceQuantity = new Dictionary<ResourceType, int>();
        foreach (ResourceType rt in System.Enum.GetValues(typeof(ResourceType)))
        {
            ResourceQuantity[rt] = 0;
            ResourceDisplays[(int)rt].Icon.sprite = IngredientIcons[(int)rt];
        }
    }

    void ResetResources()
    {
        foreach (ResourceType rt in System.Enum.GetValues(typeof(ResourceType)))
        {
            ResourceQuantity[rt] = 0;
            ResourceDisplays[(int)rt].UpdateValue(ResourceQuantity[rt]);
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

    #region UI Helpers
    void RefreshAddButtonStates()
    {
        for (int i = 0; i < ResourceDisplays.Length; i++)
            ResourceDisplays[i].RefreshAddButtonState();   
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

    public bool SendSelectedWorkerTo(Workstation workstation)
    {
        if (SelectedWorker != null)
            return SelectedWorker.AssignWorker(workstation);
        return false;
    }

    public bool SendSelectedWorkerTo(MaterialSource mat)
    {
        if (SelectedWorker != null)
            return SelectedWorker.AssignResource(mat);
        return false;
    }

    public void SelectGriddle(Griddle griddle)
    {
        if (griddle == SelectedGriddle || !IsPlaying())
            return;

        if (SelectedGriddle != null)
            SelectedGriddle.DeselectGriddle();

        SelectedGriddle = griddle;
        griddle.SelectGriddle();
        RefreshAddButtonStates();
    }

    public void ClearGriddle()
    {
        if (SelectedGriddle != null)
            SelectedGriddle.ClearGriddle();
    }

    public void DeselectGriddle()
    {
        if (SelectedGriddle != null)
            SelectedGriddle.DeselectGriddle();
        SelectedGriddle = null;
        RefreshAddButtonStates();
    }

    void SpawnScoreParticle(Griddle griddle, OrderDisplay target, int points)
    {
        ObjectPooler.Current.Spawn<ScoreGainParticle>("ScoreGainParticle", x => {
            x.Source = griddle.GetWorldPosition();
            x.Destination = target.GetWorldPosition();
            x.PercentTraveled = 0;
            x.Points = points;
        });
    }

    public void ServeOmelette(OrderDisplay order)
    {
        if (SelectedGriddle != null && IsPlaying() && SelectedGriddle.HasIngredients && !SelectedGriddle.IsCooking())
        {
            var score = order.ServeOmelette(SelectedGriddle.Ingredients);
            if (score < order.CurrentOrder.PotentialScore())
                SoundBoard.Current.PlayOmeletteFailure();
            else
                SoundBoard.Current.PlayOmeletteSuccess();
                
            SpawnScoreParticle(SelectedGriddle, order, score);
            SelectedGriddle.ClearGriddle();            
        }
    }
    #endregion
}
