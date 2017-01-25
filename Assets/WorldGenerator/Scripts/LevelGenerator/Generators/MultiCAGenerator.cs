using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/**
 * Runs multiple CA generation passes, resulting in organic splotches of different tile types within each other
 */
public class MultiCAGenerator : BaseLevelGenerator
{
    public List<CAGenerator.CAGenerationParams> CaParams = null;

    public void Reset()
    {
        this.GeneratorName = "Multi CA Generator";
    }

    public override void SetupGeneration(LevelGenMap inputMap)
    {
        base.SetupGeneration(inputMap);
        _outputs = new List<LevelGenOutput>();
        cleanGenerator();

        for (int i = 0; i < this.CaParams.Count; ++i)
        {
            this.AddPhase(this.PrepareCAPhase);
            this.AddPhase(this.RunCAPhase);
        }

        this.AddPhase(this.CleanupPhase);
    }

    public override LevelGenOutput GetOutput()
    {
        LevelGenOutput finalOutput = base.GetOutput();
        foreach (LevelGenOutput output in _outputs)
        {
            finalOutput.AppendOutput(output);
        }
        return finalOutput;
    }

    /**
     * Phases
     */
    public void PrepareCAPhase(int frames)
    {
        if (_caGenerator == null)
            _caGenerator = this.gameObject.AddComponent<CAGenerator>();

        CAGenerator.CAGenerationParams currentParams = this.CaParams[_paramsIndex];
        _caGenerator.ApplyParams(currentParams);
        _caGenerator.SetupGeneration(this.InputMap);
        this.NextPhase();
    }

    public void RunCAPhase(int frames)
    {
        _caGenerator.RunGenerationFrames(frames);
        if (_caGenerator.IsFinished)
        {
            ++_paramsIndex;
            _outputs.Add(_caGenerator.GetOutput());
            this.NextPhase();
        }
    }

    public void CleanupPhase(int frames)
    {
        cleanGenerator();
        this.NextPhase();
    }

    /**
     * Private
     */
    private CAGenerator _caGenerator;
    private int _paramsIndex;
    List<LevelGenOutput> _outputs;

    private void cleanGenerator()
    {
        if (_caGenerator != null)
        {
            Destroy(_caGenerator);
            _caGenerator = null;
        }

        _paramsIndex = 0;
    }
}
