using UnityEngine;

namespace DroneStrikers.Combat
{
    public class SimpleExperienceOnDestruction : MonoBehaviour, IExperienceProvider
    {
        [SerializeField] private float _experienceOnDestroy = 10f;
        public float ExperienceOnDestroy => _experienceOnDestroy;
    }
}