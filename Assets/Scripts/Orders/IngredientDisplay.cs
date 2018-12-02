using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngredientDisplay : IPoolable {

	public Image IngredientIcon;
	public Text CountText;

	public void Populate(GameEngine.ResourceType resource, int quantity)
	{
		IngredientIcon.sprite = GameEngine.Current.IngredientIcons[(int)resource];
		CountText.text = quantity.ToString();
	}
}
