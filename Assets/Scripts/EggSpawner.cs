using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggSpawner : MonoBehaviour {

	public float SpawnTime = 10f;
	public WorkerEgg Prefab;
	public WorkerEgg LastSpawned = null;

	public float Timer;

	void Start()
	{
		ResetState();
	}

	public void ResetState()
	{
		Timer = SpawnTime;
		LastSpawned = null;
	}

	bool CanSpawn()
	{
		return LastSpawned == null || LastSpawned.CurrentState != WorkerEgg.WorkerEggState.Unassigned;
	}

	void Update()
	{
		if (!GameEngine.Current.IsPlaying() || !CanSpawn())
			return;

		Timer -= Time.deltaTime;
		if (Timer < 0)
		{
			Timer = (SpawnTime + Timer);
			// TODO: Refactor with object pooling
			LastSpawned = Instantiate(Prefab, transform.position, transform.localRotation);
		}
	}	
}
