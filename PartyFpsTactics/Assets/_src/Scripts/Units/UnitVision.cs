using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using MrPink.Health;
using Sirenix.OdinInspector;
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
        // используется, ё маё. с помощью этой штуки  тиммейты могут начать считать тебя врагом при соответствующем поведении
        private readonly List<HealthController> _enemiesToRemember = new List<HealthController>();

        private RaycastHit _hit;
        private HealthController _selfHealth;

        [ReadOnly]
        public List<HealthController> visibleEnemies = new List<HealthController>();


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
        
        public async UniTask<HealthController> GetClosestVisibleEnemy()
        {
            float distance = 1000;
            HealthController closestVisibleEnemy = null;
            for (int i = visibleEnemies.Count - 1; i >= 0; i--)
            {
                await UniTask.DelayFrame(1);
                    
                if (i >= visibleEnemies.Count)
                    continue;

                if (visibleEnemies[i] == null)
                    continue;

                float newDistance = Vector3.Distance(transform.position, visibleEnemies[i].transform.position);
                if (newDistance < distance)
                {
                    distance = newDistance;
                    closestVisibleEnemy = visibleEnemies[i];
                }
            }

            return closestVisibleEnemy;
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
            visibleEnemies.Clear();
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
            if (!visibleEnemies.Contains(unit))
                visibleEnemies.Add(unit);
        }

        private void RemoveFromVisible(HealthController unit)
        {
            if (visibleEnemies.Contains(unit))
                visibleEnemies.Remove(unit);
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