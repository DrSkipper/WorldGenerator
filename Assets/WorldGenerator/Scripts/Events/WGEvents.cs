using UnityEngine;

public class CollisionEvent : LocalEventNotifier.Event
{
    public const string NAME = "COLLISION";
    public GameObject[] Hits;
    public Vector2 VelocityAtHit; // Velocity of actor at time collision was detected, before being multiplied by Time.deltaTime
    public Vector2 VelocityApplied; // How much of the velocity, AFTER Time.deltaTime multiplier, was applied before detecting the collision

    public CollisionEvent(GameObject[] hits, Vector2 velocity, Vector2 velocityApplied)
    {
        this.Name = NAME;
        this.Hits = hits;
        this.VelocityAtHit = velocity;
        this.VelocityApplied = velocityApplied;
    }
}

public class GameplayPausedEvent : LocalEventNotifier.Event
{
    public const string NAME = "GAMEPLAY_PAUSE";
    public bool Paused;

    public GameplayPausedEvent(bool paused)
    {
        this.Name = NAME;
        this.Paused = paused;
    }
}

public class SequencePausedEvent : LocalEventNotifier.Event
{
    public const string NAME = "SEQUENCE_PAUSE";
    public bool Paused;

    public SequencePausedEvent(bool paused)
    {
        this.Name = NAME;
        this.Paused = paused;
    }
}

public class FreezeFrameEvent : LocalEventNotifier.Event
{
    public const string NAME = "FREEZE_FRAME";
    public int NumFrames;

    public FreezeFrameEvent(int numFrames)
    {
        this.Name = NAME;
        this.NumFrames = numFrames;
    }
}

public class FreezeFrameEndedEvent : LocalEventNotifier.Event
{
    public const string NAME = "FREEZE_FRAME_END";

    public FreezeFrameEndedEvent()
    {
        this.Name = NAME;
    }
}

public class HitStunEvent : LocalEventNotifier.Event
{
    public const string NAME = "HIT_STUN";
    public int NumFrames;
    public float GravityMultiplier;
    public float AirFrictionMultiplier;

    public HitStunEvent(int numFrames, float gravityMultiplier, float airFrictionMultiplier)
    {
        this.Name = NAME;
        this.NumFrames = numFrames;
        this.GravityMultiplier = gravityMultiplier;
        this.AirFrictionMultiplier = airFrictionMultiplier;
    }
}

public class PlayerSpawnedEvent : LocalEventNotifier.Event
{
    public const string NAME = "PLAYER_SPAWNED";
    public GameObject PlayerObject;

    public PlayerSpawnedEvent(GameObject playerObject)
    {
        this.Name = NAME;
        this.PlayerObject = playerObject;
    }
}

public class PlayerDiedEvent : LocalEventNotifier.Event
{
    public const string NAME = "PLAYER_DIED";

    public PlayerDiedEvent()
    {
        this.Name = NAME;
    }
}

public class WorldRecenterEvent : LocalEventNotifier.Event
{
    public const string NAME = "WORLD_RECENTER";
    public IntegerVector RecenterOffset;

    public WorldRecenterEvent(IntegerVector recenterOffset)
    {
        this.Name = NAME;
        this.RecenterOffset = recenterOffset;
    }
}

public class SCSpriteAnimationLoopEvent : LocalEventNotifier.Event
{
    public const string NAME = "ANIM_LOOP";
    public int NewElapsed;

    public SCSpriteAnimationLoopEvent(int newElapsed)
    {
        this.Name = NAME;
        this.NewElapsed = newElapsed;
    }
}

public class PauseEvent : LocalEventNotifier.Event
{
    public const string NAME = "PAUSE";
    public PauseController.PauseGroup PauseGroup;
    public string Tag;

    public PauseEvent(PauseController.PauseGroup pauseGroup, string tag = null)
    {
        this.Name = NAME;
        this.PauseGroup = pauseGroup;
        this.Tag = tag;
    }
}

public class ResumeEvent : LocalEventNotifier.Event
{
    public const string NAME = "RESUME";
    public PauseController.PauseGroup PauseGroup;
    public string Tag;

    public ResumeEvent(PauseController.PauseGroup pauseGroup, string tag = null)
    {
        this.Name = NAME;
        this.PauseGroup = pauseGroup;
        this.Tag = tag;
    }
}

public class EntityConsumedEvent : LocalEventNotifier.Event
{
    public const string NAME = "ENTITY_CONSUMED";
    public string QuadName;
    public string EntityName;

    public EntityConsumedEvent(string quadName, string entityName)
    {
        this.Name = NAME;
        this.QuadName = quadName;
        this.EntityName = entityName;
    }
}

public class InteractionTargetChangeEvent : LocalEventNotifier.Event
{
    public const string NAME = "INTERACT_TARGET";
    public GameObject Target;

    public InteractionTargetChangeEvent(GameObject target)
    {
        this.Name = NAME;
        this.Target = target;
    }
}
