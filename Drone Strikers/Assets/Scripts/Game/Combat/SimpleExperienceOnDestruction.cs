using DroneStrikers.Core.Interfaces;
using UnityEngine;

namespace DroneStrikers.Game.Combat
{
    public class SimpleExperienceOnDestruction : MonoBehaviour, IExperienceProvider
    {
        [SerializeField] private float _experienceOnDestroy = 10f;
        public float ExperienceOnDestroy => _experienceOnDestroy;
    }
}