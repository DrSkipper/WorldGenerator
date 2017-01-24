using UnityEngine;

public static class PauseController
{
    [System.Serializable]
    public enum PauseGroup
    {
        None = 0, // Don't pause (i.e. pause menu)
        SequencedPause = 2, // Pause during transitions and scripted sequences (gameplay entites, also pauses during User Pause)
        UserPause = 6 // Pause when user pauses game (pretty much everything except pause menu)
    }

    public static void UserPause()
    {
        _pauseEvent.PauseGroup = PauseGroup.UserPause;
        _pauseEvent.Tag = null;
        GlobalEvents.Notifier.SendEvent(_pauseEvent);
    }

    public static void UserResume()
    {
        _resumeEvent.PauseGroup = PauseGroup.UserPause;
        _resumeEvent.Tag = null;
        GlobalEvents.Notifier.SendEvent(_resumeEvent);
    }

    public static void BeginSequence(string sequenceTag)
    {
        _pauseEvent.PauseGroup = PauseGroup.SequencedPause;
        _pauseEvent.Tag = sequenceTag;
        GlobalEvents.Notifier.SendEvent(_pauseEvent);
    }

    public static void EndSequence(string sequenceTag)
    {
        _resumeEvent.PauseGroup = PauseGroup.SequencedPause;
        _resumeEvent.Tag = sequenceTag;
        GlobalEvents.Notifier.SendEvent(_resumeEvent);
    }

    /**
     * Private
     */
    private static PauseEvent _pauseEvent = new PauseEvent(PauseGroup.None);
    private static ResumeEvent _resumeEvent = new ResumeEvent(PauseGroup.None);
}

public interface IPausable
{
    // Currently only used for identifying pausable components on objects
}
