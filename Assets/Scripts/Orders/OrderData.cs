using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderData {

	public static Queue<OrderData> Pool = new Queue<OrderData>();

	static OrderData Spawn()
	{
		if (Pool.Count > 0)
		{
			return Pool.Dequeue();
		}
		return new OrderData();
	}
	
	public static OrderData Generate(int difficulty)
	{
		var order = Spawn();
		order.Ingredients.Clear();
	
		// Calculate eggs
		var eggs = Mathf.CeilToInt(Random.Range(1, difficulty * EGG_MAX));
		difficulty -= eggs;
		order.Ingredients.Add(GameEngine.ResourceType.Egg, eggs);
		
		// Calculate other ingredients
		var count = difficulty;
		var actualDifficulty = eggs;
		while (count > 0 && order.Ingredients.Count < INGREDIENTS)
		{
			var ingredient = (GameEngine.ResourceType)Random.Range(1, 7);
			if (!order.Ingredients.ContainsKey(ingredient))
			{
				var amount = Mathf.Clamp(Random.Range(1, Mathf.CeilToInt(difficulty * INGREDIENT_MAX)), 1, count);
				count -= amount;
				actualDifficulty += amount;
				order.Ingredients[(GameEngine.ResourceType)ingredient] = amount;
			}
		}
		
		// Calculate timer
		order.Timer = GameEngine.Current.TimePerIngredient * actualDifficulty;
		order.Difficulty = actualDifficulty;
		
		return order;
	}

	public int ScoreOrder(Dictionary<GameEngine.ResourceType, int> omelette)
	{
		var score = 0;

		foreach (var ingredient in Ingredients.Keys)
		{
			// Entirely missing ingredients lose 2 points per quantity
			if (!omelette.ContainsKey(ingredient))
				score -= Ingredients[ingredient] * 2;
			// Lacking ingredients count 1 point per missing quantity
			else if (omelette[ingredient] < Ingredients[ingredient])
				score += (omelette[ingredient]- Ingredients[ingredient]);
			// Sufficient ingredients count 2 points per ordered quantity
			else
				score += Ingredients[ingredient] * 2;

		}

		// Extra ingredients lose 5 points per quantity
		foreach (var ingredient in omelette.Keys)
		{
			if (!Ingredients.ContainsKey(ingredient))
				score -= omelette[ingredient] * 5;
		}
		
		return score;
	}

	public int PotentialScore()
	{
		return Difficulty * 2;
	}

	public Dictionary<GameEngine.ResourceType, int> Ingredients = new Dictionary<GameEngine.ResourceType, int>();
	public float Timer;
	public int Difficulty;
	const float EGG_MAX = 0.3f; // Eggs can't make up more than this percent of an order
	const float INGREDIENT_MAX = 0.4f; // Other ingredients can't make up more than this percent of an order
	const int INGREDIENTS = 7;
	
	public void Despawn()
	{
		Pool.Enqueue(this);
	}
}
