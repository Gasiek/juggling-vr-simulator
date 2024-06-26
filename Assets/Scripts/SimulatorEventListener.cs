using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SimulatorEventListener : MonoBehaviour
{
    [System.Serializable]
    public struct EventResponse
    {
        public SimulatorEvent simulatorEvent;
        public UnityEvent response;
    }
    public List<EventResponse> eventResponses = new List<EventResponse>();

    private void OnEnable()
    {
        foreach (var eventResponse in eventResponses)
        {
            eventResponse.simulatorEvent.RegisterListener(this);
        }
    }

    private void OnDisable()
    {
        foreach (var eventResponse in eventResponses)
        {
            eventResponse.simulatorEvent.UnregisterListener(this);
        }
    }

    public void OnEventRaised(SimulatorEvent simulatorEvent)
    {
        foreach (var eventResponse in eventResponses)
        {
            if (eventResponse.simulatorEvent == simulatorEvent)
            {
                eventResponse.response.Invoke();
                break;
            }
        }
    }
}
