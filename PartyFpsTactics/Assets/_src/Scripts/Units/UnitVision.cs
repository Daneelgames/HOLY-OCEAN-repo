using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using MrPink.Health;
using UnityEngine;

namespace MrPink.Units
{
    public class UnitVision : MonoBehaviour
    {
        public float fov = 70.0f;
        public LayerMask raycastsLayerMask;
        public Transform raycastOrigin;
        
        public EnemiesSetterBehaviour setDamagerAsEnemyBehaviour = EnemiesSetterBehaviour.SetOnlyOtherTeam;
        
        // TODO кажется, не используется. Точно должно жить не здесь
        private readonly List<HealthController> _enemiesToRemember = new List<HealthController>();
        
        private readonly List<HealthController> _visibleEnemies = new List<HealthController>();
        
        private RaycastHit _hit;
        private HealthController _selfHealth;

        public List<HealthController> VisibleEnemies
            => _visibleEnemies;
    
    
        private void Start()
        {
            _selfHealth = GetComponent<HealthController>();
            StartCoroutine(CheckEnemies());
        }

        public void SetDamager(HealthController damager)
        {
            if (setDamagerAsEnemyBehaviour == EnemiesSetterBehaviour.SetOnlyOtherTeam && damager.team == _selfHealth.team)
                return;
        
            if (_enemiesToRemember.Contains(damager))
                return;
        
            _enemiesToRemember.Add(damager);
        }
    
        private IEnumerator CheckEnemies()
        {
            while (_selfHealth.health > 0)
            {
                for (int i = 0; i < _enemiesToRemember.Count; i++)
                {
                    var unit = _enemiesToRemember[i];
                    if (unit == null)
                        continue;

                    CheckUnit(unit, true);
                    yield return new WaitForSeconds(0.1f);
                }

                for (int i = 0; i < UnitsManager.Instance.unitsInGame.Count; i++)
                {
                    var unit = UnitsManager.Instance.unitsInGame[i];
                    if (unit == null || unit.team == _selfHealth.team)
                        continue;
                
                    CheckUnit(unit);
                    yield return new WaitForSeconds(0.1f);
                }

                yield return null;
            }
            VisibleEnemies.Clear();
        }

        private void CheckUnit(HealthController unit, bool ignoreTeams = false)
        {
            if (unit.health <= 0)
            {
                RemoveFromVisible(unit);
                return;
            }
                
            if (!ignoreTeams && (unit.team == _selfHealth.team || unit.team == Team.NULL || _selfHealth.team == Team.NULL))
                return;

            if (IsInLineOfSight(unit.visibilityTrigger.transform))
                AddToVisible(unit);
            else
                RemoveFromVisible(unit);
        }

        private void AddToVisible(HealthController unit)
        {
            if (!VisibleEnemies.Contains(unit))
                VisibleEnemies.Add(unit);
        }

        private void RemoveFromVisible(HealthController unit)
        {
            if (VisibleEnemies.Contains(unit))
                VisibleEnemies.Remove(unit);
        }

        private bool IsInLineOfSight(Transform target) 
        {
            if (Vector3.Angle((target.position + Vector3.one * 1.25f) - raycastOrigin.position, transform.forward) <= fov &&
                Physics.Linecast(raycastOrigin.position, target.position, out _hit, raycastsLayerMask) &&
                _hit.collider.transform == target)
            {
                return true;
            }

            return false;
        }
    }
}