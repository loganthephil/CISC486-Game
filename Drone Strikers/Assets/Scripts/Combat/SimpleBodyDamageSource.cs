using UnityEngine;

namespace DroneStrikers.Combat
{
    public class SimpleBodyDamageSource : MonoBehaviour, IDamageSource
    {
        [SerializeField] private int _contactDamage = 1;
        public float ContactDamage => _contactDamage;

        public IDestructionContextReceiver InstigatorContextReceiver => null;
    }
}