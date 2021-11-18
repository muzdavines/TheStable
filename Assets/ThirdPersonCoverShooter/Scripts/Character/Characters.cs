using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Describes a human character.
    /// </summary>
    public struct Character
    {
        /// <summary>
        /// Cached value of Motor not being null.
        /// </summary>
        public bool IsValid;

        /// <summary>
        /// Link to the game object of the character.
        /// </summary>
        public GameObject Object;

        /// <summary>
        /// Link to the character motor of the character.
        /// </summary>
        public CharacterMotor Motor;

        /// <summary>
        /// Link to the actor of the character.
        /// </summary>
        public Actor Actor;

        /// <summary>
        /// Returns true if a point at certain height inside the screen's area.
        /// </summary>
        public bool IsInSight(float height, float delta)
        {
            var wdelta = delta * (float)Screen.height / (float)Screen.width;

            var position = ViewportPoint(height);
            return position.x >= -wdelta && position.y >= -delta && position.x <= 1 + wdelta && position.y <= 1 + delta && position.z > 0;
        }

        /// <summary>
        /// Returns true if the character's head or feet inside the screen's area.
        /// </summary>
        public bool IsAnyInSight(float delta)
        {
            var wdelta = delta * (float)Screen.height / (float)Screen.width;

            var position = ViewportPoint();
            if (position.x >= -wdelta && position.y >= -delta && position.x <= 1 + wdelta && position.y <= 1 + delta && position.z > 0)
                return true;

            position = ViewportPoint(2);
            if (position.x >= -wdelta && position.y >= -delta && position.x <= 1 + wdelta && position.y <= 1 + delta && position.z > 0)
                return true;

            return false;
        }

        /// <summary>
        /// Returns position at certain height in viewport space.
        /// </summary>
        public Vector3 ViewportPoint(float height = 0)
        {
            if (Object == null || CameraManager.Main == null)
                return Vector2.zero;

            return CameraManager.Main.WorldToViewportPoint(Object.transform.position);
        }
    }

    /// <summary>
    /// Manages a list of all characters inside the level.
    /// </summary>
    public static class Characters
    {
        /// <summary>
        /// All alive characters during the last update.
        /// </summary>
        public static IEnumerable<Character> AllAlive
        {
            get
            {
                foreach (var character in _list)
                    if (character.IsValid && character.Motor.IsAlive)
                        yield return character;
            }
        }

        /// <summary>
        /// The number of characters inside the level.
        /// </summary>
        public static int Count
        {
            get { return _list.Count; }
        }

        public static Character MainPlayer;

        private static List<Character> _list = new List<Character>();
        private static Dictionary<GameObject, Character> _dictionary = new Dictionary<GameObject, Character>();

        public static Character Get(int index)
        {
            return _list[index];
        }

        public static void Register(CharacterMotor motor)
        {
            if (motor == null)
                return;

            var build = Build(motor);
            _dictionary[motor.gameObject] = build;

            if (MainPlayer.Object == null)
                if (motor.GetComponent<ThirdPersonController>() || motor.GetComponent<MobileController>())
                    MainPlayer = build;

            var isContained = false;

            for (int i = 0; i < _list.Count; i++)
                if (_list[i].Motor == motor)
                {
                    _list[i] = Build(motor);
                    isContained = true;
                }

            if (!isContained)
                _list.Add(build);
        }

        public static void Unregister(CharacterMotor motor)
        {
            if (motor != null && _dictionary.ContainsKey(motor.gameObject))
                _dictionary.Remove(motor.gameObject);

            for (int i = 0; i < _list.Count; i++)
                if (_list[i].Motor == motor)
                {
                    _list.RemoveAt(i);
                    break;
                }
        }

        /// <summary>
        /// Returns cached character description for the given object.
        /// </summary>
        public static Character Get(GameObject gameObject)
        {
            if (!_dictionary.ContainsKey(gameObject))
                _dictionary[gameObject] = Build(gameObject.GetComponent<CharacterMotor>());

            return _dictionary[gameObject];
        }

        /// <summary>
        /// Creates and returns character description for the given object.
        /// </summary>
        public static Character Build(CharacterMotor motor)
        {
            Character character;

            if (motor != null)
            {
                character.IsValid = true;
                character.Object = motor.gameObject;
                character.Motor = motor;
                character.Actor = motor.GetComponent<Actor>();
            }
            else
            {
                character.IsValid = false;
                character.Object = null;
                character.Motor = null;
                character.Actor = null;
            }

            return character;
        }
    }
}
