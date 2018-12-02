using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderDisplay : IPoolable {

	List<IngredientDisplay> ingredients = new List<IngredientDisplay>();

	public Transform IngredientDisplayContainer;
	public OrderData CurrentOrder;
	public Image TimerIcon;
	public Button AddButton;
	float timer;

	public Vector3 GetWorldPosition()
	{
		if (AddButton == null)
			AddButton = GetComponentInChildren<Button>();

		return AddButton.transform.position;
	}

	public void Populate(OrderData order)
	{
		CurrentOrder = order;
		timer = order.Timer;
		foreach (var key in order.Ingredients.Keys)
		{
			ingredients.Add(ObjectPooler.Current.Spawn<IngredientDisplay>("IngredientDisplay", x => {
				x.transform.SetParent(IngredientDisplayContainer);
				x.transform.localScale = Vector3.one;
				x.Populate(key, order.Ingredients[key]);
			}));
		}
	}

	void Update()
	{
		if (GameEngine.Current.IsPlaying())
		{
			timer -= Time.deltaTime;
			TimerIcon.fillAmount = Mathf.Clamp(1 - (timer / CurrentOrder.Timer), 0, 1);
			if (timer <= 0)
				Fail();
		}
	}

	public override void ClearState()
	{
		base.ClearState();

		for (int i = 0; i < ingredients.Count; i++)
		{
			ingredients[i].Despawn();
		}
		ingredients.Clear();
	}

	void Dequeue()
	{
		GameEngine.Current.DequeueOrder(this);
		CurrentOrder.Despawn();
		this.Despawn();
	}

	public void Fail()
	{
		// Lose the entire value of the order
		GameEngine.Current.AddScore(-CurrentOrder.Difficulty * 2);
		Dequeue();
	}

	public void ServeClicked()
	{
		GameEngine.Current.ServeOmelette(this);
	}

	public int ServeOmelette(Dictionary<GameEngine.ResourceType, int> omelette)
	{
		var score = CurrentOrder.ScoreOrder(omelette);
		Dequeue();
		return score;
	}
}
