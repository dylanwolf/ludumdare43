using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceCount : MonoBehaviour {

	public Text TextDisplay;
	int lastValue = 0;

	public void UpdateValue(int value)
	{
		if (lastValue != value)
		{
			TextDisplay.text = value.ToString();
			lastValue = value;
		}
	}
}
