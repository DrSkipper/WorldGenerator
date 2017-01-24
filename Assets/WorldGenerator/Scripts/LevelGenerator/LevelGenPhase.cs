using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class LevelGenPhase
{
	public delegate void PhaseUpdate(int frames);

	public int FramesElapsed { get { return _framesElapsed; } }

	public LevelGenPhase(PhaseUpdate updateCallback)
	{
		_updateCallback = updateCallback;
	}

	public void RunFrames(int frames)
	{
		_updateCallback(frames);
		++_framesElapsed;
	}

	/**
	 * Private
	 */
	public int _framesElapsed;
	private PhaseUpdate _updateCallback;
}
