using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundBoard : MonoBehaviour {

	public AudioSource Source;
	public AudioClip EggProcess;
	public AudioClip EggDie;
	public AudioClip EggSpawn;
	public AudioClip OmeletteSuccess;
	public AudioClip OmeletteFailure;
	public AudioClip OmeletteNewOrder;
	public AudioClip IngredientAdded;
	

	public void PlayEggProcess()
	{
		Source.PlayOneShot(EggProcess);
	}

	public void PlayEggDie()
	{
		Source.PlayOneShot(EggDie);
	}

	public void PlayEggSpawn()
	{
		Source.PlayOneShot(EggSpawn);
	}

	public void PlayOmeletteSuccess()
	{
		Source.PlayOneShot(OmeletteSuccess);
	}

	public void PlayOmeletteFailure()
	{
		Source.PlayOneShot(OmeletteFailure);
	}

	public void PlayOmeletteNewOrder()
	{
		Source.PlayOneShot(OmeletteNewOrder);
	}

	public void PlayIngredientAdded()
	{
		Source.PlayOneShot(IngredientAdded);
	}

	public static SoundBoard Current;

	void Awake()
	{
		Current = this;
	}
}
