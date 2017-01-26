using UnityEngine;
using System.Collections.Generic;

/**
 * Runs CA passes and then creates some rooms on top
 */
public class BSPPlusCAGenerator : BaseLevelGenerator
{
    public CAGenerator.CAGenerationParams CAParams;
    public BSPGenerator.BSPGenerationParams BSPParams;
    public int BSPBoundsReduction = 0;
    public int CABoundsReduction = 8;

    public void Reset()
    {
        this.GeneratorName = "BSP + CA Generator";
    }

    public override void SetupGeneration(LevelGenMap inputMap, LevelGenMap outputMap, IntegerRect bounds)
    {
        base.SetupGeneration(inputMap, outputMap, bounds);
        _outputs = new List<LevelGenOutput>();
        cleanGenerator();
        this.AddPhase(this.BSPInitPhase);
        this.AddPhase(this.RunPhase);
        this.AddPhase(this.CAInitPhase);
        this.AddPhase(this.RunPhase);
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

    public void BSPInitPhase(int frames)
    {
        BSPGenerator bspGenerator = this.gameObject.AddComponent<BSPGenerator>();
        IntegerRect bounds = IntegerRect.ConstructRectFromMinAndSize(this.Bounds.Min.X + this.BSPBoundsReduction, this.Bounds.Min.Y + this.BSPBoundsReduction, this.Bounds.Size.X - this.BSPBoundsReduction * 2, this.Bounds.Size.Y - this.BSPBoundsReduction * 2);
        bspGenerator.ApplyParams(this.BSPParams);
        bspGenerator.SetupGeneration(this.InputMap, this.OutputMap, bounds);
        _currentGenerator = bspGenerator;
        this.NextPhase();
    }

    public void CAInitPhase(int frames)
    {
        cleanGenerator();
        CAGenerator caGenerator = this.gameObject.AddComponent<CAGenerator>();
        IntegerRect bounds = IntegerRect.ConstructRectFromMinAndSize(this.Bounds.Min.X + this.CABoundsReduction, this.Bounds.Min.Y + this.CABoundsReduction, this.Bounds.Size.X - this.CABoundsReduction * 2, this.Bounds.Size.Y - this.CABoundsReduction * 2);
        caGenerator.ApplyParams(this.CAParams);
        caGenerator.SetupGeneration(this.InputMap, this.OutputMap, bounds);
        _currentGenerator = caGenerator;
        this.NextPhase();
    }

    public void RunPhase(int frames)
    {
        _currentGenerator.RunGenerationFrames(frames);
        if (_currentGenerator.IsFinished)
        {
            _outputs.Add(_currentGenerator.GetOutput());
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
    BaseLevelGenerator _currentGenerator;
    List<LevelGenOutput> _outputs;

    private void cleanGenerator()
    {
        if (_currentGenerator != null)
        {
            Destroy(_currentGenerator);
            _currentGenerator = null;
        }
    }
}
