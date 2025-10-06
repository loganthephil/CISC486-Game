using System.Collections.Generic;
using System.Linq;
using DroneStrikers.Combat;
using DroneStrikers.Drone;
using UnityEngine;

namespace DroneStrikers.AI
{
    public class ObjectDetector : MonoBehaviour
    {
        /// <summary>
        ///     Returns true if there is at least one enemy drone in range.
        /// </summary>
        public bool IsDroneInRange => _detectedDrones.Count > 0;
        
        // TODO: Consider that a drones level may change while being detected
        
        // TODO: Honestly just completely overhaul this entire file. It's so jank.

        /// <summary>
        ///     The drone info of the highest level detected enemy drone, or null if none are detected.
        /// </summary>
        public DroneInfo DroneWithHighestLevel
        {
            get
            {
                if (_highestLevelInfo != null) return _highestLevelInfo;
                RemoveNullDrones();
                _highestLevelInfo = IsDroneInRange ? _detectedDrones.Values.OrderByDescending(info => info.Level).FirstOrDefault() : null;
                return _highestLevelInfo;
            }
        }

        /// <summary>
        ///     The drone info of the lowest level detected enemy drone, or null if none are detected.
        /// </summary>
        /// <returns></returns>
        public DroneInfo DroneWithLowestLevel
        {
            get
            {
                if (_lowestLevelInfo != null) return _lowestLevelInfo;
                RemoveNullDrones();
                _lowestLevelInfo = IsDroneInRange ? _detectedDrones.Values.OrderBy(info => info.Level).FirstOrDefault() : null;
                return _lowestLevelInfo;
            }
        }

        private readonly HashSet<GameObject> _detected = new();
        private readonly Dictionary<GameObject, DroneInfo> _detectedDrones = new();

        private DroneInfo _selfInfo; // Info of the drone this detector belongs to
        private DroneInfo _highestLevelInfo;
        private DroneInfo _lowestLevelInfo;

        /// <summary>
        ///     Returns the closest detected object, or null if none are detected.
        ///     Be careful not to run this too often, as it does a linear search through all detected objects.
        /// </summary>
        /// <returns> The closest detected GameObject, or null if none are detected. </returns>
        public GameObject GetClosestDetectedObject()
        {
            if (_detected.Count == 0) return null;

            GameObject closest = null;
            Vector3 selfPos = transform.position;
            float minSqrDist = float.MaxValue;

            // TODO: Remove null references in a more appropriate way
            List<GameObject> toRemove = new();

            foreach (GameObject obj in _detected)
            {
                if (obj == null)
                {
                    toRemove.Add(obj);
                    continue;
                }

                float sqr = (obj.transform.position - selfPos).sqrMagnitude;
                if (sqr < minSqrDist)
                {
                    minSqrDist = sqr;
                    closest = obj;
                }
            }

            // Clean up any null references
            foreach (GameObject obj in toRemove)
            {
                _detected.Remove(obj);
                _detectedDrones.Remove(obj);
            }

            return closest;
        }

        /// <summary>
        ///     Returns the most important detected object based on the following priority:
        ///     1. Enemy drone with the highest level
        ///     2. Closest non-drone object
        /// </summary>
        /// <returns> The most important detected GameObject, or null if none are detected. </returns>
        public GameObject GetMostImportantDetectedObject()
        {
            if (_detected.Count == 0) return null;
            return IsDroneInRange ? DroneWithHighestLevel?.gameObject : GetClosestDetectedObject();
        }

        private void Awake()
        {
            _selfInfo = GetComponentInParent<DroneInfo>();
        }

        private void OnTriggerEnter(Collider other)
        {
            TeamMember otherTeamMember = other.GetComponent<TeamMember>();
            if (otherTeamMember != null && otherTeamMember.Team == _selfInfo.Team) return; // Ignore same team members

            _detected.Add(other.gameObject);

            if (!other.CompareTag("Drone")) return;

            DroneInfo info = other.GetComponent<DroneInfo>();
            if (info == null) return;
            // if (info.Team == _selfInfo.Team) return; // Ignore same team drones

            _detectedDrones[other.gameObject] = info;

            if (_highestLevelInfo == null || info.Level > _highestLevelInfo.Level) _highestLevelInfo = info;
            if (_lowestLevelInfo == null || info.Level < _lowestLevelInfo.Level) _lowestLevelInfo = info;
        }

        private void OnTriggerExit(Collider other)
        {
            _detected.Remove(other.gameObject);

            if (!other.CompareTag("Drone")) return;

            if (_detectedDrones.Remove(other.gameObject, out DroneInfo info))
            {
                if (info == _highestLevelInfo) _highestLevelInfo = null;
                if (info == _lowestLevelInfo) _lowestLevelInfo = null;
            }
        }

        private void RemoveNullDrones()
        {
            List<GameObject> toRemove = (from pair in _detectedDrones where pair.Key == null || pair.Value == null select pair.Key).ToList();

            foreach (GameObject obj in toRemove)
            {
                _detectedDrones.Remove(obj);
                _detected.Remove(obj);
            }

            // TODO: Come back to this?
            // If the object was destroyed, set explicitly to null
            if (_highestLevelInfo == null) _highestLevelInfo = null;
            if (_lowestLevelInfo == null) _lowestLevelInfo = null;
        }
    }
}