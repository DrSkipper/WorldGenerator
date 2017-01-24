using UnityEngine;
using System.Collections.Generic;

public class TimedCallbacks : VoBehavior, IPausable
{
    public delegate void Callback();

    public void AddCallback(Object owner, Callback callback, float afterTime)
    {
        if (_callbackEntries == null)
            _callbackEntries = new Dictionary<Object, List<CallbackEntry>>();

        if (!_callbackEntries.ContainsKey(owner))
        {
            List<CallbackEntry> entries = new List<CallbackEntry>();
            _callbackEntries.Add(owner, entries);
        }

        _callbackEntries[owner].Add(new CallbackEntry(callback, afterTime));
    }

    public void RemoveCallbacksForOwner(Object owner)
    {
        if (_callbackEntries != null)
            _callbackEntries.Remove(owner);
    }

    void Update()
    {
        if (_callbackEntries != null)
        {
            foreach (Object owner in _callbackEntries.Keys)
            {
                List<CallbackEntry> entries = _callbackEntries[owner];
                for (int i = 0; i < entries.Count;)
                {
                    CallbackEntry entry = entries[i];
                    entry.timeRemaining -= Time.deltaTime;

                    if (entry.timeRemaining <= 0.0f)
                    {
                        entries.RemoveAt(i);
                        entry.callback();
                    }
                    else
                    {
                        ++i;
                    }
                }
            }
        }
    }

    /**
     * Private
     */
    private class CallbackEntry
    {
        public Callback callback;
        public float timeRemaining;

        public CallbackEntry(Callback cb, float t)
        {
            this.callback = cb;
            this.timeRemaining = t;
        }
    }

    private Dictionary<Object, List<CallbackEntry>> _callbackEntries;
}
