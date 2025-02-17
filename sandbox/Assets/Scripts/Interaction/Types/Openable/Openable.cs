﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Openable : Interactable
{
	public UnlockAction.Types unlockActionType;

	protected Animator anim;

	public static event Action<int, OpenableTuple> OnOpenableStateChanged;
	public event Action OnDoorOpened;

	public int id;
	public bool isOpen;
	public bool isLocked;

	protected FMOD.Studio.EventInstance lockedNoise;

	// Use this for initialization
	protected new void Start()
	{
		base.Start();
		anim = GetComponent<Animator>();

		OpenableTuple tuple = GameData.data.saveData.ReadOpenableState(id, gameObject.name);
		isOpen = tuple.isOpen;
		isLocked = tuple.isLocked;

		anim.SetBool("isOpen", isOpen);
		anim.SetBool("isLocked", isLocked);

		lockedNoise = FMODUnity.RuntimeManager.CreateInstance("event:/Environment/locked");
		lockedNoise.setVolume(PlayerPrefs.GetFloat("SfxVolume") * PlayerPrefs.GetFloat("MasterVolume"));
	}

	protected void ToggleState()
	{
		OpenableTuple tuple = new OpenableTuple();
		tuple.isOpen = isOpen;
		tuple.isLocked = isLocked;
		OnOpenableStateChanged(id, tuple);
		if (isOpen)
		{
			if (OnDoorOpened != null)
			{
				OnDoorOpened();
			}
		}
	}

	public void Unlock()
	{
		if (isLocked)
		{
			isLocked = false;
			anim.SetBool("isLocked", isLocked);
			ToggleState();
			lockedNoise.start();
		}
	}

	public void TryUnlock()
	{
		if (unlockActionType == UnlockAction.Types.HaveKey)
		{
			GetComponent<HaveKeyUnlockAction>().TryKey();
		}
		else
		{
			EventDisplay.instance.AddEvent("Locked.");
			lockedNoise.start();
		}
	}
}
