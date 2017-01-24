using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class LevelGenBehavior : MonoBehaviour
{
	public LevelGenMap Map
	{
		get
		{
			if (_map == null)
				_map = this.GetComponent<LevelGenMap>();
			return _map;
		}
	}

	public LevelGenManager Manager
	{
		get
		{
			if (_manager == null)
				_manager = this.GetComponent<LevelGenManager>();
			return _manager;
		}
	}

	/**
	 * Private
	 */
	private LevelGenMap _map = null;
	private LevelGenManager _manager;
}
