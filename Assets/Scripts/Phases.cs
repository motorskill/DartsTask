using System;
using System.Collections.Generic;
using UnityEngine;

public class Phases : MonoBehaviour
{
    public List<ExperimentPhase> experimentPhases;
    public int currentPhaseIndex = 0;
    public float phaseTimer = 0f;
    public bool phaseRunning = false;

    // Primary physics script
    [SerializeField] public AimShoot aimShoot;

    public void CreatePhases()
    {
        experimentPhases = new List<ExperimentPhase>
        {
            new ExperimentPhase("ITI", UnityEngine.Random.Range(4f, 8f), () =>
            {
                Debug.Log("→ ITI: show fixation, disable input");
            }),
            new ExperimentPhase("Ready", UnityEngine.Random.Range(2f, 8f), () =>
            {
                Debug.Log("→ Ready: show crosshair, no movement");
            }),
            new ExperimentPhase("Aim", UnityEngine.Random.Range(4f, 8f), () =>
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
            new ExperimentPhase("Return", 2f, () =>
            {
                Debug.Log("→ Return: move back to start");
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
