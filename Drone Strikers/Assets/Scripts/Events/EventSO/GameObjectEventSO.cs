using UnityEngine;

namespace DroneStrikers.Events.EventSO
{
    [CreateAssetMenu(menuName = "Events/GameObject Event", fileName = "NewGameObjectEvent")]
    public class GameObjectEventSO : SingleParameterEventSO<GameObject> { }
}