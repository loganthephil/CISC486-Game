using DroneStrikers.Core.Interfaces;
using UnityEngine;

namespace DroneStrikers.Game.AI
{
    public class SimpleValueDangerProvider : MonoBehaviour, IValueDangerProvider
    {
        private IExperienceProvider _experienceProvider;
        public float BaseValue => _experienceProvider.ExperienceOnDestroy;

        public float DangerLevel { get; }

        private void Awake()
        {
            _experienceProvider = GetComponent<IExperienceProvider>();
            if (_experienceProvider == null) Debug.LogError("No IExperienceProvider found on" + gameObject.name + ". SimpleValueProvider requires an IExperienceProvider to function.", this);
        }
    }
}