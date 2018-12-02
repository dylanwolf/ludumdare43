using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour {

	static Dictionary<string, List<IPoolable>> pools = new Dictionary<string, List<IPoolable>>();


	[Serializable]
	public class PoolConfiguration
	{
		public string Name;
		public MonoBehaviour Prefab;
	}

	public static ObjectPooler Current;

	[Header("Configuration")]
	public PoolConfiguration[] Pools;

	Dictionary<string, PoolConfiguration> configDict = new Dictionary<string, PoolConfiguration>();

	void InitPools()
	{
		pools.Clear();
		foreach (var pool in Pools)
		{
			pools[pool.Name] = new List<IPoolable>();
			configDict[pool.Name] = pool;
		}
	}

	public void ResetState()
	{
		foreach (var pool in pools.Values)
			for(int i = 0; i < pool.Count; i++)
				Despawn(pool[i]);
	}

	public static void Despawn<T>(T obj) where T: IPoolable
	{
		obj.ClearState();
		obj.gameObject.SetActive(false);
	}

	public T Spawn<T>(string poolName, Action<T> initFunction) where T: IPoolable
	{
		var pool = pools[poolName];
		T tmp = null;

		// Find first deactivated item

		for (int i = 0; i < pool.Count; i++)
		{
			if (!pool[i].isActiveAndEnabled)
			{
				tmp = pool[i] as T;
				tmp.gameObject.SetActive(true);
				break;
			}
		}

		// If no item found, recreate
		if (tmp == null)
			tmp = InstantiateNewPooledObject<T>(poolName);
		
		// Reset state and init
		tmp.ResetState();
		initFunction(tmp);

		return tmp;
	}

	T InstantiateNewPooledObject<T>(string poolName) where T: IPoolable
	{
		T tmp = (T)Instantiate(configDict[poolName].Prefab);
		pools[poolName].Add(tmp);
		return tmp;
	}

	void Awake()
	{
		Current = this;
		InitPools();
	}

}
