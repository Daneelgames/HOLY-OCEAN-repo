using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using UnityEngine;

public class RestrictedZone : MonoBehaviour
{
    public Team ownerTeam = Team.Red;
    public List<Team> allowedTeams = new List<Team>();

    private List<HealthController> hcInside = new List<HealthController>();
    private List<GameObject> unitsInsideGO = new List<GameObject>();
    private bool intruderInside = false;
    private bool sirenPlaying = false;
    public AudioSource entruderInsideAu;
    private void Start()
    {
        StartCoroutine(CheckUnitsInside());
    }

    IEnumerator CheckUnitsInside()
    {
        while (true)
        {
            bool enemyInside = false;
            for (int i = 0; i < hcInside.Count; i++)
            {
                var hc = hcInside[i];
                if (allowedTeams.Contains(hc.team) == false)
                {
                    if (hc.crimeLevel)
                    {
                        enemyInside = true;
                        hc.crimeLevel.CrimeCommitedAgainstTeam(ownerTeam, false, false);
                    }
                }
            }

            intruderInside = enemyInside;
            yield return new WaitForSeconds(0.1f);
            if (!sirenPlaying && intruderInside)
            {
                sirenPlaying = true;
                entruderInsideAu.Play();
                if (changeVolumeCoroutine != null)
                    StopCoroutine(changeVolumeCoroutine);
                changeVolumeCoroutine = StartCoroutine(ChangeVolumeTo(1, true, false));
            }
            else if (sirenPlaying && !intruderInside)
            {
                sirenPlaying = false;
                if (changeVolumeCoroutine != null)
                    StopCoroutine(changeVolumeCoroutine);
                changeVolumeCoroutine = StartCoroutine(ChangeVolumeTo(0, false, true));
            }
        }
    }

    private Coroutine changeVolumeCoroutine;
    IEnumerator ChangeVolumeTo(float newVolume, bool startBeforeChangingVolume = false, bool stopAfterChangingVolume = false)
    {
        float t = 0;
        float tt = 1;
        float initVolume = entruderInsideAu.volume;

        if (startBeforeChangingVolume)
            entruderInsideAu.Play();
        
        while (t < tt)
        {
            t += Time.deltaTime;
            entruderInsideAu.volume = Mathf.Lerp(initVolume, newVolume, t / tt);
            yield return null;
        }
        
        if (stopAfterChangingVolume)
            entruderInsideAu.Stop();
    }
    [SerializeField] private bool setFollowIntruder = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7) // units
        {
            if (!unitsInsideGO.Contains(other.gameObject))
            {
                unitsInsideGO.Add(other.gameObject);
                var hc = other.gameObject.GetComponent<HealthController>();
                if (hc && !hcInside.Contains(hc))
                {
                    hcInside.Add(hc);
                }

                if (hc && allowedTeams.Contains(hc.team) == false)
                {
                    if (hc.crimeLevel)
                        hc.crimeLevel.CrimeCommitedAgainstTeam(ownerTeam, false, false, setFollowIntruder);
                }
            }   
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 7) // units
        {
            if (unitsInsideGO.Contains(other.gameObject))
            {
                var hc = other.gameObject.GetComponent<HealthController>();
                if (hc && hcInside.Contains(hc))
                {
                    hcInside.Remove(hc);
                }

                unitsInsideGO.Remove(other.gameObject);
            }   
        }
    }
    

}
