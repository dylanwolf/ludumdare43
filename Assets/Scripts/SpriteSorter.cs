using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSorter : MonoBehaviour {

	SpriteRenderer _renderer;
	public float YMultiplier = 100f;
	float LastY = float.MinValue;
	
	void Awake()
	{
		_renderer = GetComponent<SpriteRenderer>();
	}

	void Update () {
		if (!Mathf.Approximately(LastY, transform.position.y))
		{
			_renderer.sortingOrder = -Mathf.CeilToInt(transform.position.y * YMultiplier);
			LastY = transform.position.y;
		}
	}
}
