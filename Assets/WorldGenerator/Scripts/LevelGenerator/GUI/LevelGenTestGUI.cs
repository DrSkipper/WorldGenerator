using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using Newtonsoft.Json;

public class LevelGenTestGUI : LevelGenBehavior
{
	public bool GenerateOnStart = true;

	void Awake()
	{
		_generators = this.GetComponents<BaseLevelGenerator>().ToList();
	}

	void Start()
	{
		if (this.GenerateOnStart && _generators.Count > 0)
		{
			this.Manager.InitiateGeneration(null);
			_beganGeneration = true;
		}
	}

	void Update()
	{
		if (_beganGeneration && this.Manager.Finished)
		{
			_beganGeneration = false;

            // Dump json of level gen output
            /*LevelGenOutput output = this.Manager.GetOutput();
            string json = JsonConvert.SerializeObject(output, Formatting.None);
            Debug.Log("json of level gen output:\n" + json);*/
            Debug.Log("level generation complete, json output not enabled");

			this.Manager.Cleanup();
		}
	}

	void OnGUI()
	{
		// Normal code path for level generation in the game
		if (GUILayout.Button("Use Test Story Output"))
		{
			this.Manager.Cleanup();
			this.Manager.InitiateGeneration(null);
			_beganGeneration = true;
		}

		// Directly test specific generators with values from Unity inspector
		foreach (BaseLevelGenerator generator in _generators)
		{
			if (GUILayout.Button(generator.GeneratorName))
			{
				this.Manager.Cleanup();
				this.Manager.TestGenerator(generator);
				_beganGeneration = true;
			}
		}
	}

	/**
	 * Private
	 */
	private List<BaseLevelGenerator> _generators;
	private bool _beganGeneration;
}
