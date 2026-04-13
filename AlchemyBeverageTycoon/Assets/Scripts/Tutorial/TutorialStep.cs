using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class TutorialStep
{
    [TextArea]
    public string instructionText;
    public GameObject button;
    public Sprite buttonIcon;

    [Header("Signal Trigger")]
    public bool waitForSignal = false;
    public List<string> requiredSignals = new List<string>();

    [Header("Auto Complete")]
    public bool autoComplete = false;
    public bool hidePanel = false;
    public float autoDelaySeconds = 1f;


    [Header("Next Button")]
    public bool useNextButton = true; 

    [Header("Events")]
    public UnityEvent onStart;
    public UnityEvent onComplete;
}