using UnityEngine;
using System;
using System.Collections.Generic;

namespace CupkekGames.Combat
{
    /// <summary>
    /// Handles collision detection for a projectile.
    /// Attach this to your projectile prefab.
    /// Requires a SphereCollider (set to Is Trigger) and a Rigidbody (can be kinematic).
    /// </summary>
    public class ProjectileCollisionHandler : MonoBehaviour
    {
        public CombatUnit Caster { get; set; }

        // Action parameters: Caster, HitTarget, HitPoint
        public Action<CombatUnit, CombatUnit> OnTargetHitAction { get; set; }

        public bool DestroyOnHit { get; set; } =
            true; // Does the projectile get destroyed/deactivated on the first valid hit?

        private HashSet<Guid>
            _hitTargetsThisFlight; // Used for piercing projectiles to avoid multiple hits on the same target during one flight.

        private SphereCollider _sphereCollider;
        private Rigidbody _rigidbody;

        private void Awake()
        {
            _sphereCollider = GetComponent<SphereCollider>();
            if (_sphereCollider == null)
            {
                Debug.LogError("ProjectileCollisionHandler requires a SphereCollider component on the GameObject.");
                return;
            }

            if (!_sphereCollider.isTrigger)
            {
                _sphereCollider.isTrigger = true;
            }

            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                Debug.LogError("ProjectileCollisionHandler requires rigidbody component.");
                return;
            }

            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;
        }

        public void Reset()
        {
            if (_hitTargetsThisFlight == null)
            {
                _hitTargetsThisFlight = new();
            }

            _hitTargetsThisFlight.Clear(); // Clear previously hit targets for this new flight.
        }

        public void Setup(CombatUnit caster, Action<CombatUnit, CombatUnit> onTargetHitAction, bool destroyOnHit,
            float collisionRadius)
        {
            Reset();

            Caster = caster;
            OnTargetHitAction = onTargetHitAction;
            DestroyOnHit = destroyOnHit;

            if (_sphereCollider != null)
            {
                _sphereCollider.radius = collisionRadius;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            // Attempt to get CombatUnit from the collided object or its parent
            CombatUnitView targetUnitGameObject = other.GetComponentInParent<CombatUnitView>();
            if (targetUnitGameObject == null)
            {
                return; // No CombatUnitGameObject found, ignore this collision.
            }

            CombatUnit targetUnit = targetUnitGameObject.CombatUnit;

            // Validate the target
            if (targetUnit == null || !CombatTargetSelection.CanSelect(Caster, targetUnit, false, false, true))
            {
                // Hit something not a CombatUnit, or hit the caster. Ignore.
                return;
            }

            // Handle piercing: if it's a piercing projectile and has already hit this target in this flight, ignore.
            if (!DestroyOnHit)
            {
                if (_hitTargetsThisFlight.Contains(targetUnit.ID))
                {
                    return; // Already hit this target on this flight.
                }

                _hitTargetsThisFlight.Add(targetUnit.ID); // Record hit for piercing.
            }

            // Invoke the primary action (apply effects, damage, etc.)
            OnTargetHitAction?.Invoke(Caster, targetUnit);

            // If the projectile should be destroyed/deactivated on hit
            if (DestroyOnHit)
            {
                gameObject.SetActive(false); // Deactivate the projectile. The spawner/pool should handle its return.
            }
        }
    }
}