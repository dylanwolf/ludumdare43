using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceCount : MonoBehaviour {

	public GameEngine.ResourceType Resource;
	public Text TextDisplay;
	public Image Icon;
	public GameObject AddButton;
	RectTransform _r;
	int lastValue = 0;

	void Start()
	{
		_r = (RectTransform)transform;
	}

	public Vector3 GetWorldPosition()
	{
		return TextDisplay.transform.position;
	}

	public void UpdateValue(int value)
	{
		if (lastValue != value)
		{
			TextDisplay.text = value.ToString();
			lastValue = value;
			RefreshAddButtonState();
		}
	}

	public void AddToGriddle()
	{
		if (lastValue > 0 && GameEngine.Current.AddResourceToSelectedGriddle(Resource, 1))
		{
			UpdateValue(lastValue - 1);
			RefreshAddButtonState();
		}
	}

	public void RefreshAddButtonState()
	{
		var state = (GameEngine.Current.SelectedGriddle != null && lastValue > 0);
		if (state != AddButton.activeSelf)
			AddButton.SetActive(state);
	}
}
