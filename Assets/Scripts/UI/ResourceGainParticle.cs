using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceGainParticle : IPoolable {
	[Header("Configuration")]
	public float Speed = 2.0f;


	[Header("In-Game Data")]

	public GameEngine.ResourceType Resource;
	public int Quantity;
	public Vector3 Source;
	public Vector3 Destination;
	public SpriteRenderer Sprite;



	public float PercentTraveled = 0;

	public override void ResetState()
	{
		PercentTraveled = 0;
		transform.position = new Vector3(1000, 1000, 0);
	}

	public void SetSprite()
	{
		if (Sprite == null)
			Sprite = GetComponent<SpriteRenderer>();
		Sprite.sprite = GameEngine.Current.IngredientIcons[(int)Resource];
	}

	void Update()
	{
		if (GameEngine.Current.IsPlaying())
		{
			PercentTraveled += Time.deltaTime * Speed;
			PercentTraveled = Mathf.Clamp(PercentTraveled, 0, 1);

			var pos = transform.position;
			pos = Vector3.Lerp(Source, Destination, EasingFunction.EaseInOutQuad(0, 1, PercentTraveled));
			transform.position = pos;

			if (PercentTraveled >= 1)
			{
				GameEngine.Current.UpdateResource(Resource, Quantity);
				this.Despawn();
			}
		}
	}
}
