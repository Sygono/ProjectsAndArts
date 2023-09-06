using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StepMode { AlternatingTetrapodGait, QueueWait, QueueNoWait }

public class IKManager : MonoBehaviour
{
    public List<IKStepper> steppers = new List<IKStepper>();
    public List<IKStepper> left = new List<IKStepper>();
    public List<IKStepper> right = new List<IKStepper>();
    private Queue<IKStepper> stepperQueue = new Queue<IKStepper>();

    [Header("Steptime")]
    public bool dynamicStepTime = true;
    public float stepTimePerVelocity;
    [Range(0, 1.0f)]
    public float maxStepTime;

    [Header("Step Mode")]
    public StepMode stepMode;

    private IKStepper currentStepper;

    void Awake() {
        foreach (IKStepper stepper in steppers) {
            stepperQueue.Enqueue(stepper);
        }
    }

    void Update() {
        if (currentStepper == null) {
            currentStepper = stepperQueue.Dequeue();
            stepperQueue.Enqueue(currentStepper);
        } else {
            if (!currentStepper.isStepping&&currentStepper.needStepping) {
                currentStepper.step();
            } else {
                currentStepper = null;
            } 
        }
    }

}