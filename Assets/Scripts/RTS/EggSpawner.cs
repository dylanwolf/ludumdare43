using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggSpawner : MonoBehaviour {

	public float SpawnTime = 10f;
	public float SpawnOffset = 5f;
	public WorkerEgg Prefab;
	public WorkerEgg LastSpawned = null;

	public float Timer;

	void Start()
	{
		ResetState();
	}

	public void ResetState()
	{
		Timer = SpawnTime + SpawnOffset;
		LastSpawned = null;
	}

	bool CanSpawn()
	{
		return LastSpawned == null || !LastSpawned.isActiveAndEnabled || LastSpawned.CurrentState != WorkerEgg.WorkerEggState.Unassigned;
	}

	void Update()
	{
		if (!GameEngine.Current.IsPlaying() || !CanSpawn())
			return;

		Timer -= Time.deltaTime;
		if (Timer < 0)
		{
			Timer = (SpawnTime + Timer);
			SoundBoard.Current.PlayEggSpawn();
			LastSpawned = ObjectPooler.Current.Spawn<WorkerEgg>("WorkerEgg", x => {
				x.transform.position = transform.position;
				x.UpdateUI();
			});
		}
	}	
}
