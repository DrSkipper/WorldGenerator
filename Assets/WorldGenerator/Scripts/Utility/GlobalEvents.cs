using UnityEngine;

[RequireComponent(typeof(LocalEventNotifier))]
public class GlobalEvents : MonoBehaviour
{
    public static LocalEventNotifier Notifier { get { return _notifier; } }

    void Awake()
    {
        _notifier = this.GetComponent<LocalEventNotifier>();
    }

    void OnDestroy()
    {
        _notifier = null;
    }

    /**
     * Private
     */
    private static LocalEventNotifier _notifier;
}
