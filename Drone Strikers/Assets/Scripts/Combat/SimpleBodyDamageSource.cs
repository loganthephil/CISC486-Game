using UnityEngine;

namespace DroneStrikers.Combat
{
    public class SimpleBodyDamageSource : MonoBehaviour, IDamageSource
    {
        [SerializeField] private int _contactDamage = 1;
        public int ContactDamage => _contactDamage;

        public IDestructionContextReceiver InstigatorContextReceiver => null;
    }
}