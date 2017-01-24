using UnityEngine;
using System.Collections.Generic;

/**
 * Runs CA passes and then creates some rooms on top
 */
public class CAPlusRoomGenerator : BaseLevelGenerator
{
    public List<CAGenerator.CAGenerationParams> CaParams = null;
    public RoomGenerator.RoomGenerationParams RoomParams;

    public void Reset()
    {
        this.GeneratorName = "CA + Room Generator";
    }

    public override void SetupGeneration()
    {
        base.SetupGeneration();
        _outputs = new List<LevelGenOutput>();
        cleanGenerator();
        this.AddPhase(this.CAInitPhase);
        this.AddPhase(this.RunPhase);
        this.AddPhase(this.RoomInitPhase);
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

    public void CAInitPhase(int frames)
    {
        MultiCAGenerator caGenerator = this.gameObject.AddComponent<MultiCAGenerator>();
        caGenerator.CaParams = this.CaParams;
        caGenerator.SetupGeneration();
        _currentGenerator = caGenerator;
        this.NextPhase();
    }

    public void RoomInitPhase(int frames)
    {
        cleanGenerator();
        RoomGenerator roomGenerator = this.gameObject.AddComponent<RoomGenerator>();
        roomGenerator.ApplyParams(this.RoomParams);
        roomGenerator.SetupGeneration();
        _currentGenerator = roomGenerator;
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
