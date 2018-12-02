using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IPoolable : MonoBehaviour {

	public virtual void ResetState() {}
	public virtual void ClearState() {}

	public void Despawn()
	{
		ObjectPooler.Despawn(this);
	}
}
