using System.Collections;
using System.Collections.Generic;
using MrPink;
using UnityEngine;

public class FragGrenadeTool : MonoBehaviour
{
    public ExplosionController fragExplosion;
    public ScoringActionType scoringAction;

    public void Explode()
    {
        var explosion = Instantiate(fragExplosion, transform.position, transform.rotation);
        explosion.Init(scoringAction);
    }
}
