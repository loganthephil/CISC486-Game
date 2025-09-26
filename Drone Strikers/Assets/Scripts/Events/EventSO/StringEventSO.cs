using UnityEngine;

namespace DroneStrikers.Events.EventSO
{
    [CreateAssetMenu(menuName = "Events/String Event", fileName = "NewStringEvent")]
    public class StringEventSO : SingleParameterEventSO<string>
    {
    }
}