using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Creates and manages objects with Health Bar for all enemies visible on screen. 
    /// </summary>
    public class EnemyDisplayManager : MonoBehaviour
    {
        /// <summary>
        /// Prototype of a health bar to be shown on any visible enemy.
        /// </summary>
        [Tooltip("Prototype of a health bar to be shown on any visible enemy.")]
        public RectTransform HealthPrototype;
        
        /// <summary>
        /// Prototype of an arrow to be shown for any active enemy that is away.
        /// </summary>
        [Tooltip("Prototype of an arrow to be shown for any active enemy that is away.")]
        public RectTransform ArrowPrototype;

        /// <summary>
        /// Player that is used to determine who is an enemy.
        /// </summary>
        [Tooltip("Player that is used to determine who is an enemy.")]
        public Actor Player;

        /// <summary>
        /// Offset of the health bar relative to the screen height.
        /// </summary>
        [Tooltip("Offset of the health bar relative to the screen height.")]
        public Vector2 Offset = new Vector2(0, 0.1f);

        private Dictionary<GameObject, GameObject> _bars = new Dictionary<GameObject, GameObject>();
        private Dictionary<GameObject, GameObject> _away = new Dictionary<GameObject, GameObject>();
        private List<GameObject> _keep = new List<GameObject>();

        private void LateUpdate()
        {
            /// Manage health bars
            {
                _keep.Clear();

                if (HealthPrototype != null)
                {
                    foreach (var character in Characters.AllAlive)
                        if (character.Actor != null && (Player == null || character.Actor.Side != Player.Side))
                        {
                            var position = character.ViewportPoint();

                            if (character.IsInSight(2, -0.01f))
                            {
                                _keep.Add(character.Object);

                                if (!_bars.ContainsKey(character.Object))
                                {
                                    var clone = GameObject.Instantiate(HealthPrototype.gameObject);
                                    clone.transform.SetParent(transform);
                                    clone.GetComponent<HealthBar>().Target = character.Object;
                                    _bars.Add(character.Object, clone);
                                }

                                var t = _bars[character.Object].GetComponent<RectTransform>();
                                t.gameObject.SetActive(true);
                                t.position = new Vector3(position.x * Screen.width + Offset.x * Screen.height, position.y * Screen.height + Offset.y * Screen.height, t.position.z);
                            }
                        }
                }

                for (int i = 0; i < _bars.Count - _keep.Count; i++)
                {
                    foreach (var key in _bars.Keys)
                        if (!_keep.Contains(key))
                        {
                            GameObject.Destroy(_bars[key]);
                            _bars.Remove(key);
                            break;
                        }
                }
            }

            /// Manage arrows
            {
                _keep.Clear();

                if (ArrowPrototype != null)
                {
                    foreach (var character in Characters.AllAlive)
                        if (character.Actor != null && (Player == null || character.Actor.Side != Player.Side) && !_bars.ContainsKey(character.Object))
                        {
                            var position = character.ViewportPoint();
                            const float delta = 0.05f;

                            if (position.z < 0)
                            {
                                position.x = 1 - position.x;
                                position.y = 1 - position.y;
                            }

                            float angle = 0;

                            var isLeft = position.x < delta;
                            var isRight = position.x > 1 - delta;
                            var isDown = position.y < delta;
                            var isUp = position.y > 1 - delta;

                            if (isUp)
                            {
                                if (isLeft) angle = 45;
                                else if (isRight) angle = -45;
                                else angle = 0;
                            }
                            else if (isDown)
                            {
                                if (isLeft) angle = 135;
                                else if (isRight) angle = -135;
                                else
                                    angle = 180;
                            }
                            else
                            {
                                if (isLeft) angle = 90;
                                else if (isRight) angle = -90;
                            }

                            if (isLeft) position.x = delta;
                            if (isDown) position.y = delta;
                            if (isRight) position.x = 1 - delta;
                            if (isUp) position.y = 1 - delta;

                            _keep.Add(character.Object);

                            if (!_away.ContainsKey(character.Object))
                            {
                                var clone = GameObject.Instantiate(ArrowPrototype.gameObject);
                                clone.transform.SetParent(transform, false);
                                _away.Add(character.Object, clone);
                            }

                            var t = _away[character.Object].GetComponent<RectTransform>();
                            t.gameObject.SetActive(true);
                            t.position = new Vector3(position.x * Screen.width, position.y * Screen.height, t.position.z);
                            t.eulerAngles = new Vector3(t.eulerAngles.x, t.eulerAngles.y, angle);
                        }
                }

                for (int i = 0; i < _away.Count - _keep.Count; i++)
                {
                    foreach (var key in _away.Keys)
                        if (!_keep.Contains(key))
                        {
                            GameObject.Destroy(_away[key]);
                            _away.Remove(key);
                            break;
                        }
                }
            }
        }
    }
}
