using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Simulator Event", menuName = "Simulator Event")]
public class SimulatorEvent : ScriptableObject
{
    private readonly List<SimulatorEventListener> eventListeners = new();

    [ContextMenu("Raise Event")]
    public void Raise()
    {
        for (int i = eventListeners.Count - 1; i >= 0; i--)
        {
            eventListeners[i].OnEventRaised(this);
        }
    }

    public void RegisterListener(SimulatorEventListener listener)
    {
        if (!eventListeners.Contains(listener))
        {
            eventListeners.Add(listener);
        }
    }

    public void UnregisterListener(SimulatorEventListener listener)
    {
        if (eventListeners.Contains(listener))
        {
            eventListeners.Remove(listener);
        }
    }
}
