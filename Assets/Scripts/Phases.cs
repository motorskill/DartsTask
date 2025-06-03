using System;
using System.Collections.Generic;
using UnityEngine;

public class Phases : MonoBehaviour
{
    public List<ExperimentPhase> experimentPhases;
    public int currentPhaseIndex = 0;
    public int conditionNum = 0;
    public string conditionName = "";
    public float phaseTimer = 0f;
    public float ITI_time;
    public float Ready_time;
    public float Aim_time;
    public bool phaseRunning = false;

    // Primary physics script
    [SerializeField] public AimShoot aimShoot;

    public void CreatePhases()
    {
        experimentPhases = new List<ExperimentPhase>
        {
            new ExperimentPhase("ITI", ITI_time, () =>
            {
                Debug.Log("→ ITI: show fixation, disable input");
            }),
            new ExperimentPhase("Ready", Ready_time, () =>
            {
                Debug.Log("→ Ready: show crosshair, no movement");
            }),
            new ExperimentPhase("Aim", Aim_time, () =>
            {
                Debug.Log("→ Aim: allow aiming");
            }),
            // new ExperimentPhase("Go Cue", 0.1f, () =>
            // {
            //     Debug.Log("→ Go Cue: prepare to shoot (visual cue here)");
            // }),
            new ExperimentPhase("Shoot", 0.9f, () =>
            {
                Debug.Log("→ Shoot: detect finger lift or input");
            }),
            // new ExperimentPhase("Feedback", 1f, () =>
            // {
            //     Debug.Log("→ Feedback: show feedback cue");
            // }),
            new ExperimentPhase("Return", 30f, () =>
            {
                Debug.Log("→ Return: move back to start");
            }),
            new ExperimentPhase("End", 2f, () =>
            {
                Debug.Log("→ End: end trial");
            })
        };

        StartNextPhase();
    }

    public void StartNextPhase()
    {
        if (currentPhaseIndex >= experimentPhases.Count)
        {
            Debug.Log("All phases completed.");
            return;
        }

        ExperimentPhase phase = experimentPhases[currentPhaseIndex];
        Debug.Log($"Starting Phase: {phase.name}");

        phaseTimer = phase.duration;
        phase.onEnter?.Invoke();
        phaseRunning = true;
    }
}
