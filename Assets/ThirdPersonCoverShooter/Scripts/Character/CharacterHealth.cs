using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Manages health and sets Is Alive in Character Motor to false when it reaches 0. Registers damage done by bullets.
    /// Multiple hitboxes can be setup inside the character. On setup Character Health will stop registering hits and instead will expect child Body Part Health components to pass taken damage to it.
    /// </summary>
    public class CharacterHealth : MonoBehaviour, ICharacterHealthListener
    {
        /// <summary>
        /// Current health of the character.
        /// </summary>
        [Tooltip("Current health of the character.")]
        public float Health = 100f;

        /// <summary>
        /// Max health to regenerate to.
        /// </summary>
        [Tooltip("Max health to regenerate to.")]
        public float MaxHealth = 100f;
        public float stamina, maxStamina;
        public float balance, maxBalance;
        public float mind, maxMind;
        /// <summary>
        /// Amount of health regenerated per second.
        /// </summary>
        [Tooltip("Amount of health regenerated per second.")]
        public float Regeneration = 0f;

        /// <summary>
        /// How much the incoming damage is multiplied by.
        /// </summary>
        [Tooltip("How much the incoming damage is multiplied by.")]
        public float DamageMultiplier = 1;

        /// <summary>
        /// Does the component reduce damage on hits. Usually used for debugging purposes to make immortal characters.
        /// </summary>
        [Tooltip("Does the component reduce damage on hits. Usually used for debugging purposes to make immortal characters.")]
        public bool IsTakingDamage = true;

        /// <summary>
        /// Are bullet hits done to the main collider registered as damage.
        /// </summary>
        [Tooltip("Are bullet hits done to the main collider registered as damage.")]
        public bool IsRegisteringHits = true;

        /// <summary>
        /// Executed on death.
        /// </summary>
        public Action Died;

        /// <summary>
        /// Executed after being hurt.
        /// </summary>
        public Action<float> Hurt;

        /// <summary>
        /// Executed after being healed by any amount.
        /// </summary>
        public Action<float> Healed;

        /// <summary>
        /// Executed after any health change.
        /// </summary>
        public Action<float> Changed;

        private CharacterMotor _motor;

        private bool _isDead;
        private float _previousHealth;

        private static Dictionary<GameObject, CharacterHealth> _map = new Dictionary<GameObject, CharacterHealth>();
        MissionCharacter myMissionCharacter;
        public static CharacterHealth Get(GameObject gameObject)
        {
            if (_map.ContainsKey(gameObject))
                return _map[gameObject];
            else
                return null;
        }

        public static bool Contains(GameObject gameObject)
        {
            return _map.ContainsKey(gameObject);
        }

        private void Awake()
        {
            _previousHealth = Health;
            _motor = GetComponent<CharacterMotor>();
        }

        private void OnEnable()
        {
            _map[gameObject] = this;
            myMissionCharacter = GetComponent<MissionCharacter>();
        }

        private void OnDisable()
        {
            _map.Remove(gameObject);
        }

        private void OnValidate()
        {
            Health = Mathf.Max(0, Health);
            MaxHealth = Mathf.Max(0, MaxHealth);
        }

        private void LateUpdate()
        {
            if (!_isDead)
            {
                Health = Mathf.Clamp(Health + Regeneration * Time.deltaTime, 0, MaxHealth);
                check();
            }
        }

        /// <summary>
        /// Catch the resurrection event and remember that the character is alive.
        /// </summary>
        public void OnResurrect()
        {
            _isDead = false;

            if (Health <= float.Epsilon)
                Health = 0.001f;
        }

        /// <summary>
        /// Catch the death event, set health to zero and remember that the character is dead now.
        /// </summary>
        public void OnDead()
        {
            var wasOff = _isDead;
            _isDead = true;
            Health = 0;

            if (!wasOff && Died != null)
                Died();
        }

        /// <summary>
        /// Reduce health on bullet hit.
        /// </summary>
        public void OnTakenHit(Hit hit)
        {
            print(transform.name + " Taken Hit");
            Deal(hit);
        }

        /// <summary>
        /// Heals by some amount.
        /// </summary>
        public void Heal(float amount)
        {
            Health = Mathf.Clamp(Health + amount, 0, MaxHealth);

            if (Health > float.Epsilon && _motor != null && !_motor.IsAlive)
                _motor.Resurrect();

            if (!_isDead)
                check();
        }

        /// <summary>
        /// Deals a specific amount of damage.
        /// </summary>
        public void Deal(float amount)
        {
            if (Health <= 0 || !IsTakingDamage)
                return;

            amount *= DamageMultiplier;

            Health = Mathf.Clamp(Health - amount, 0, MaxHealth);
            check();

            if (Health <= 0 && _motor != null)
                _motor.Die();
        }

        public void Deal(Hit hit) {
            print(transform.name + " Deal");
            if (Health <= 0 || !IsTakingDamage)
                return;
            print("Deal: "+hit.move.name);
            hit.move.HitEffect(this, hit.Attacker.GetComponent<MissionCharacter>().character);
            
            check();
            myMissionCharacter.healthBar.SetMeters(this);
            if (Health <= 0 && _motor != null)
                _motor.Die();
        }

        public void Deal(StableDamage damage) {
            if (Health <= 0 || !IsTakingDamage)
                return;
            if (stamina <= 0 || balance <= 0 || mind <= 0) {
                Health -= damage.health;
                Debug.Log("TODO: more health damage adjustments needed");
            }
            stamina -= damage.stamina;
            balance -= damage.balance;
            mind -= damage.mind;
            
            check();
            myMissionCharacter.healthBar.SetMeters(this);
            if (Health <= 0 && _motor != null)
                _motor.Die();
        }

        private void check()
        {
            if (_previousHealth != Health)
            {
                _previousHealth = Health;
                if (Changed != null) Changed(Health);
                if (_previousHealth < Health && Healed != null) Healed(Health);
                if (_previousHealth > Health && Hurt != null) Hurt(Health);
            }
        }
    }
}