using UnityEngine;
using System.Collections;

public class Timer
{
	public delegate void TimerCallback();
	public TimerCallback Callback;
	public bool Paused;
	public bool Loops;
	public bool Completed { get; private set; }
    public int FramesRemaining { get { return _framesRemaining; } }

	public Timer(int numFrames, bool loops = false, bool startsImmediately = true, TimerCallback callback = null)
	{
		_framesRemaining = _numFrames = numFrames;
		this.Loops = loops;
		this.Callback = callback;
		this.Paused = !startsImmediately;
	}

	public void start()
	{
		this.Paused = false;
	}

    public void reset()
    {
        _framesRemaining = _numFrames;
        this.Completed = false;
    }

    public void reset(int numFrames)
    {
        _numFrames = numFrames;
        this.reset();
    }

    public void complete()
    {
        if (this.Callback != null)
            this.Callback();

        if (this.Loops)
            _framesRemaining = _numFrames;
        else
            this.Completed = true;
    }

	public void update(int dFrames = 1)
	{
		if (!this.Paused && !this.Completed)
		{
			_framesRemaining -= dFrames;

			if (_framesRemaining <= 0)
                this.complete();
		}
	}

	public void invalidate()
	{
		this.Callback = null;
		this.Completed = true;
	}

	/**
	 * Private
	 */
	private int _numFrames;
	private int _framesRemaining;
}
