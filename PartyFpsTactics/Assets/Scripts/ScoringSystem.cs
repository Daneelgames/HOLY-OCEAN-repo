using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoringSystem : MonoBehaviour
{
 public static ScoringSystem Instance;
 // каждый килл дает множитель
 // вид килла определяет количество очков
 public enum ActionType
 {
  NULL,
  KillRangedIdle, KillRangedOnMove, KillRangedOnRun, KillRangedOnJump,
  KillMeleeIdle, KillMeleeOnMove, KillMeleeOnRun, KillMeleeOnJump,
  KillLeaningRangedIdle, KillLeaningRangedOnMove, KillLeaningRangedOnRun, KillLeaningRangedOnJump,
  KillLeaningMeleeIdle, KillLeaningMeleeOnMove, KillLeaningMeleeOnRun, KillLeaningMeleeOnJump,
  KillExplosion, TileDestroyed, EnemyBumped, BarrelBumped 
 }

 public List<ActionScore> Scores;

 private void Awake()
 {
  Instance = this;
 }

 public void RegisterAction(ActionType action)
 {
  
 }
}

public class ActionScore
{
 public ScoringSystem.ActionType ActionType;
 public int score = 100;
}