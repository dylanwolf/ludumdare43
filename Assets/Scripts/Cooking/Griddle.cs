using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Griddle : MonoBehaviour {

	public Dictionary<GameEngine.ResourceType, int> Ingredients = new Dictionary<GameEngine.ResourceType, int>();
	public Image Selecter;
	public bool HasIngredients;
	public Image CookGauge;

	public float CookTime = 1.0f;
	float timer = 0;

	void Start () {
		ResetState();		
	}

	void Update()
	{
		if (IsCooking() && GameEngine.Current.IsPlaying())
		{
			timer -= Time.deltaTime;
			CookGauge.fillAmount = Mathf.Clamp(1 - (timer / CookTime), 0, 1);
			if (timer <= 0)
				EndCooking();
		}
	}

	public bool IsCooking()
	{
		return timer > 0;
	}

	public void ResetState()
	{
		ClearGriddle();
		DeselectGriddle();
	}

	public void GriddleTouched()
	{
		GameEngine.Current.SelectGriddle(this);
	}

	public void SelectGriddle()
	{
		Selecter.enabled = true;
	}

	public void DeselectGriddle()
	{
		Selecter.enabled = false;
	}

	public void ClearGriddle()
	{
		Ingredients.Clear();
		HasIngredients = false;
		EndCooking();
		UpdateGriddleUI();
	}

	void UpdateGriddleUI()
	{
		// TODO: Add animations
	}

	void StartCooking()
	{
		HasIngredients = true;
		timer = CookTime;
		CookGauge.enabled = true;
	}

	void EndCooking()
	{
		timer = 0;
		CookGauge.fillAmount = 0;
		CookGauge.enabled = false;	
	}

	public void AddResource(GameEngine.ResourceType resource, int amount)
	{
		if (!Ingredients.ContainsKey(resource))
			Ingredients[resource] = 0;

		Ingredients[resource] += amount;
		if (amount > 0)
			StartCooking();

		UpdateGriddleUI();
	}
}
