using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using MrPink.Health;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MrPink.Units
{
    public class UnitVision : MonoBehaviour
    {
        public float fov = 70.0f;
        public float visionDistance = 250;
        public LayerMask raycastsLayerMask;
        public Transform raycastOrigin;
        public float timeToForgive = 5;
        
        public EnemiesSetterBehaviour setDamagerAsEnemyBehaviour = EnemiesSetterBehaviour.SetOnlyOtherTeam;
        
        // TODO кажется, не используется. Точно должно жить не здесь
        // используется, ё маё. с помощью этой штуки  тиммейты могут начать считать тебя врагом при соответствующем поведении
        [ReadOnly]
        public List<HealthController> _enemiesToRemember = new List<HealthController>();

        private RaycastHit _hit;
        private HealthController _selfHealth;

        [ReadOnly]
        public List<HealthController> visibleEnemies = new List<HealthController>();
        [ReadOnly]
        public List<HealthController> visibleUnits = new List<HealthController>();


        private void OnEnable()
        {
            _selfHealth = GetComponent<HealthController>();
            StartCoroutine(CheckingUnits());
            StartCoroutine(CheckingPlayers());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        public void SetDamager(HealthController damager, bool stack = false, bool tellToFriends = false)
        {
            if (setDamagerAsEnemyBehaviour == EnemiesSetterBehaviour.SetOnlyOtherTeam && damager.team == _selfHealth.team)
                return;
            /*
            // VVV WE DONT USE STACK FOR NOW VVV
            if (!stack && _enemiesToRemember.Contains(damager))
                return;
            */
            if (_enemiesToRemember.Contains(damager))
                return;

            // IF DAMAGED BY A MACHINE - SET DAMAGER DRIVER
            if (damager.controlledMachine && damager.controlledMachine.controllingHc)
                damager = damager.controlledMachine.controllingHc;
            
            
            Debug.Log("SetDamager " + damager);
            _enemiesToRemember.Add(damager);

            StartCoroutine(ForgiveUnitOverTime(damager));
            
            if (_selfHealth.AiMovement)
                _selfHealth.AiMovement.SetDamager(damager);
            
            if (!tellToFriends)
                return;
            
            for (int i = 0; i < visibleUnits.Count; i++) // tell all his friends
            {
                var unit = visibleUnits[i];
                if (unit == null || unit == _selfHealth || unit.health <= 0 || !unit.gameObject.activeInHierarchy)
                    continue;
                
                if (unit.UnitVision && unit.team == _selfHealth.team)
                {
                    if (unit.UnitVision._enemiesToRemember.Contains(damager) == false)
                        unit.UnitVision.SetDamager(damager);
                }
            }
        }

        IEnumerator ForgiveUnitOverTime(HealthController unit)
        {
            yield return new WaitForSeconds(timeToForgive);
            ForgiveUnit(unit, unit.team == _selfHealth.team || unit.team == Team.PlayerParty);
        }

        public void ForgiveUnit(HealthController unit, bool removeFromEnemies)
        {
            if (unit == null)
                return;
            
            if (TeamsManager.Instance.IsUnitEnemyToMe(_selfHealth.team, unit.team))
                return;
            
            for (int i = 0; i < _enemiesToRemember.Count; i++)
            {
                if (_enemiesToRemember[i] == unit)
                    _enemiesToRemember.RemoveAt(i);
            }
            if (removeFromEnemies && visibleEnemies.Contains(unit))
                visibleEnemies.Remove(unit);
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

                if (visibleEnemies[i] == null || visibleEnemies[i].transform == null)
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

        IEnumerator CheckingPlayers()
        {
            while (Game._instance == false || Game.LocalPlayer == null)
            {
                yield return null;
            }
            while (true)
            {
                for (int i = 0; i < Game._instance.PlayersInGame.Count; i++)
                {
                    if (i > Game._instance.PlayersInGame.Count - 1 || Game._instance.PlayersInGame[i] == null)
                        continue;
                    
                    CheckUnit(Game._instance.PlayersInGame[i].Health);
                    yield return new WaitForSeconds(0.1f);
                }
                //CheckUnit(Game.LocalPlayer.Health);
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        private IEnumerator CheckingUnits()
        {
            while (true)
            {
                yield return null;
                
                if (_selfHealth.health <= 0)
                    continue;
                
                for (int i = 0; i < _enemiesToRemember.Count; i++)
                {
                    var unit = _enemiesToRemember[i];
                    if (unit == null || unit.gameObject.activeInHierarchy == false)
                        continue;

                    CheckUnit(unit, true);
                    yield return new WaitForSeconds(0.1f);
                }

                for (int i = 0; i < UnitsManager.Instance.HcInGame.Count; i++)
                {
                    var unit = UnitsManager.Instance.HcInGame[i];
                    if (unit == null || unit == _selfHealth || unit.gameObject.activeInHierarchy == false)
                        continue;
                
                    CheckUnit(unit);
                    yield return new WaitForSeconds(0.1f);
                }

                yield return null;
            }
            visibleEnemies.Clear();
        }

        private void CheckUnit(HealthController unit, bool addVisibleAsEnemy = false)
        {
            if (unit.health <= 0)
            {
                RemoveFromVisible(unit);
                return;
            }

            if (IsInLineOfSight(unit.visibilityTrigger.transform))
            {
                if (unit == Game.LocalPlayer.Health)
                {
                    //Debug.Log("UnitVision UNIT " + gameObject.name + " SEES PLAYER");
                }
                
                if (TeamsManager.Instance.IsUnitEnemyToMe(_selfHealth.team, unit.team) || addVisibleAsEnemy) // OR IF TEMPORAL ENEMY 
                {
                    if (unit == Game.LocalPlayer.Health)
                    {
                        //Debug.Log("UnitVision UNIT " + gameObject.name + " ADDS PLAYER TO VISIBLE ENEMIES");

                        if (seePlayerFeedbackCoroutine == null)
                        {
                            seePlayerFeedbackCoroutine = StartCoroutine(SeePlayerFeedback());

                        }
                    }
                    AddVisibleEnemy(unit);
                    return;
                }
                
                if (unit == Game.LocalPlayer.Health)
                {
                    //Debug.Log("UnitVision UNIT " + gameObject.name + " ADDS PLAYER TO VISIBLE UNITS");
                }
                AddVisibleUnit(unit);
                return;
            }
            
            if (unit == Game.LocalPlayer.Health)
            {
                //Debug.Log("UnitVision UNIT " + gameObject.name + " CANT SEE PLAYER");
            }

            if (unit.IsPlayer) // if any player
            {
                if (visibleEnemies.Contains(unit)) // if this ai sees player in this frame
                {
                    //give order to follow current players position
                    // should be called once when player exits ai vision
                    if (Random.value > 0.5f) // just for interest
                    {
                        _selfHealth.AiMovement.MoveToPositionOrder(unit.transform.position);
                    }
                }
            }
            RemoveFromVisible(unit);
        }
        
        Coroutine seePlayerFeedbackCoroutine;
        IEnumerator SeePlayerFeedback()
        {
            QuestMarkers.Instance.AddMarker(raycastOrigin, Color.red, "!");
            yield return new WaitForSeconds(1);
            QuestMarkers.Instance.RemoveMarker(raycastOrigin);
            seePlayerFeedbackCoroutine = null;
        }

        private void OnDestroy()
        {
            QuestMarkers.Instance.RemoveMarker(raycastOrigin);
            if (seePlayerFeedbackCoroutine != null)
                StopCoroutine(seePlayerFeedbackCoroutine);
        }

        private void AddVisibleEnemy(HealthController unit)
        {
            if (!visibleEnemies.Contains(unit))
                visibleEnemies.Add(unit);
        }
        
        private void AddVisibleUnit(HealthController unit)
        {
            if (!visibleUnits.Contains(unit))
                visibleUnits.Add(unit);
            
            unit.AddToVisibleByUnits(_selfHealth);
        }

        private void RemoveFromVisible(HealthController unit)
        {
            if (visibleEnemies.Contains(unit))
                visibleEnemies.Remove(unit);

            if (visibleUnits.Contains(unit))
                visibleUnits.Remove(unit);
            
            unit.RemoveFromVisibleByUnits(_selfHealth);
        }

        private bool IsInLineOfSight(Transform target)
        {
            if (Vector3.Distance(target.position, raycastOrigin.position) > visionDistance)
                return false;
            if (Vector3.Angle((target.position + Vector3.one * 1.25f) - raycastOrigin.position, transform.forward) <= fov)
            {
                if (Physics.Linecast(target.position,  raycastOrigin.position, out _hit, GameManager.Instance.AllSolidsMask, QueryTriggerInteraction.Ignore))
                {
                    lastRaycastedPoint = _hit.point;
                    
                    // collided with solid, can't see shit

                    return false;
                }

                return true;
            }
            
            return false;
        }

        private Vector3 lastRaycastedPoint = Vector3.zero;

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(lastRaycastedPoint, 1);
            Gizmos.DrawSphere(lastRaycastedPoint, 0.2f);
        }
    }
}