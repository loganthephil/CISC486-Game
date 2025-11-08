using DroneStrikers.Core.Types;
using UnityEngine;

namespace DroneStrikers.Events.EventSO
{
    [CreateAssetMenu(menuName = "Events/DamageContext Event", fileName = "NewDamageContextEvent")]
    public class DamageContextEventSO : SingleParameterEventSO<DamageContext> { }
}