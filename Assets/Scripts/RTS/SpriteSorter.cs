using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSorter : MonoBehaviour {

	SpriteRenderer _renderer;
	public int YMultiplier = 1000;
	public int ChildIncrement = 1;
	float LastY = float.MinValue;
	public SpriteRenderer[] Children;
	
	void Awake()
	{
		_renderer = GetComponent<SpriteRenderer>();
	}

	void Update () {
		if (!Mathf.Approximately(LastY, transform.position.y))
		{
			int value = -Mathf.CeilToInt(transform.position.y * YMultiplier);
			_renderer.sortingOrder = value;
			if (Children != null)
			{
				for (int i = 0; i < Children.Length; i++)
				{
					value += ChildIncrement;
					Children[i].sortingOrder = value;
				}
			}
			LastY = transform.position.y;
		}
	}
}
