using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class QuestMark : MonoBehaviour
{
    public Transform target;
    public Text markerName;
    public Text markerNameBack;

    public Image hpBar;
    [ReadOnly]public float lastHpBarFill = 1;
    public Image hpBarOffsetFeedback;
    [ReadOnly] public HealthController hcToHpBar;
    [SerializeField] private Animator _animator;
    private static readonly int Damage = Animator.StringToHash("Damage");

    public void StartListeningForDamage()
    {
        hcToHpBar.OnDamagedEvent.AddListener(DamageFeedback);
    }
    void DamageFeedback()
    {
        var fill = hcToHpBar.GetHealthFill;
        hpBarOffsetFeedback.fillAmount = 1 - fill;
        _animator.SetTrigger(Damage);   
    }

}
