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
	public int lastValue = 0;

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
		if (GameEngine.Current.SelectedGriddle != null &&
			GameEngine.Current.IsPlaying() &&
			GameEngine.Current.CanSpendResource(Resource, 1))
		{
			SpawnResourceParticle();
			GameEngine.Current.UpdateResource(Resource, -1);
			RefreshAddButtonState();
		}
	}

	public void RefreshAddButtonState()
	{
		var state = (GameEngine.Current.SelectedGriddle != null && lastValue > 0);
		if (state != AddButton.activeSelf)
			AddButton.SetActive(state);
	}

    void SpawnResourceParticle()
    {
        ObjectPooler.Current.Spawn<ResourceSpendParticle>("ResourceSpendParticle", x => {
            x.Source = this.transform.position;
            x.Destination = GameEngine.Current.SelectedGriddle.GetWorldPosition();
			x.DestinationGriddle = GameEngine.Current.SelectedGriddle;
            x.Resource = Resource;
            x.PercentTraveled = 0;
            x.Quantity = 1;
            x.SetSprite();
        });
    }
}
