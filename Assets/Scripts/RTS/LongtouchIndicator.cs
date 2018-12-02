using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongtouchIndicator : MonoBehaviour {

	public Sprite[] Frames;
	SpriteRenderer _renderer;

	void Awake()
	{
		_renderer = GetComponent<SpriteRenderer>();
	}


	public void SetFrame(int index)
	{
		_renderer.sprite = Frames[Mathf.Clamp(index, 0, Frames.Length - 1)];
	}
}
