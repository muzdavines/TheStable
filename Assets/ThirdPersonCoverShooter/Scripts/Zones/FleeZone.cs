using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Denotes a target zone for the fleeing AI to run to. Either disables or destroys AI that reaches it.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class FleeZone : Zone<FleeZone>
    {
        /// <summary>
        /// Time in seconds after an actor that reached the flee zone is removed from the game.
        /// </summary>
        [Tooltip("Time in seconds after an actor that reached the flee zone is removed from the game.")]
        public float RemoveDelay = 3;

        /// <summary>
        /// Are the actors removed from the game by destroying them. If false, they are disabled.
        /// </summary>
        [Tooltip("Are the actors removed from the game by destroying them. If false, they are disabled.")]
        public bool IsRemovingByDestroying = true;

        private List<GameObject> _actors = new List<GameObject>();
        private Dictionary<GameObject, float> _times = new Dictionary<GameObject, float>();

        /// <summary>
        /// Registers an actor to be removed by the zone.
        /// </summary>
        public void Register(GameObject actor)
        {
            if (!_actors.Contains(actor))
            {
                _actors.Add(actor);
                _times[actor] = 0;
            }
        }

        /// <summary>
        /// Removes the actor from the removal list.
        /// </summary>
        public void Unregister(GameObject actor)
        {
            if (_actors.Contains(actor))
                _actors.Remove(actor);

            if (_times.ContainsKey(actor))
                _times.Remove(actor);
        }

        private void Update()
        {
            for (int i = _actors.Count - 1; i >= 0; i--)
            {
                var actor = _actors[i];
                _times[actor] += Time.deltaTime;

                if (_times[actor] >= RemoveDelay)
                {
                    Unregister(actor);

                    if (IsRemovingByDestroying)
                        Destroy(actor.gameObject);
                    else
                        actor.gameObject.SetActive(false);
                }
            }
        }
    }
}
