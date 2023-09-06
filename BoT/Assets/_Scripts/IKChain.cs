using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TargetInfo {
    public Vector3 position;
    public Vector3 normal;
    public bool grounded;

    public TargetInfo(Vector3 m_position, Vector3 m_normal, bool m_grounded = false) {
        position = m_position;
        normal = m_normal;
        grounded = m_grounded;
    }    
}

public enum TargetMode {
    Manual,
    Calculated
}


public class IKChain : MonoBehaviour
{
    
    [Header("Solving")]
    [Range(0.1f, 10f)]
    public float tolerance;
    [Range(1f, 100f)]
    public float singularityRadius;
    [Range(1, 100)]
    public int maxIterations = 10;
    [Range(0f, 1f)]
    public float weight = 1.0f;

    [Header("Chain")]
    public List<IKJoint> joints;
    public Transform endEffector;
    public bool adjustLastJointToNormal = true;
    
    [Header("Target Mode")]
    public TargetMode targetMode;
    public Transform targetObject;

    public TargetInfo target;
    private Vector3 lastEndeffectorPos;
    public Vector3 defaultPos;
    float error;
    public bool pause;


    void Reset() {
        Transform current = transform;
        joints = new List<IKJoint>();
        while(current.childCount!=0) {
            IKJoint joint = current.gameObject.AddComponent<IKJoint>();
            joints.Add(joint);
            current = current.GetChild(0);
        }
        endEffector = current;
    }


    void Awake() {
        defaultPos = transform.InverseTransformPoint(endEffector.position);
        target.position = endEffector.position;
    }


    void LateUpdate() {
        GetTarget();
        Solve();
        lastEndeffectorPos = endEffector.position;
    }

    void GetTarget() {
        if (targetMode == TargetMode.Manual&&targetObject != null) {
            setTarget(new TargetInfo(targetObject.position,targetObject.up,false));
        }
    }

    public void setTarget(TargetInfo _target) {
        target = _target;
    }

    void Solve() {
        solveChainCCD(ref joints, endEffector, target, getTolerance(), maxIterations, weight, getSingularityRadius(), adjustLastJointToNormal);
    }

    void solveChainCCD(ref List<IKJoint> joints, Transform endEffector, TargetInfo target, float tolerance, int maxIterations, float weight, float singularityRadius=0 ,bool adjustLastJointToNormal = false) {

        int iteration = 0;
        error = Vector3.Distance(target.position, endEffector.position);
        float oldError;
        float errorDelta;

        while (iteration < maxIterations && error > tolerance) {

            for (int i = 0; i < joints.Count; i++) {
                //This line ensures that the we start with the last joint, but then chronologically, e.g. k= 4 0 1 2 3
                int k = mod((i - 1), joints.Count);
                solveJointCCD(joints[k], ref endEffector, ref target, weight, singularityRadius, adjustLastJointToNormal && k == joints.Count - 1);
            }
            iteration++;

            oldError = error;
            error = Vector3.Distance(target.position, endEffector.position);
            errorDelta = Mathf.Abs(oldError - error);
            /*
            if (errorDelta < minimumChangePerIteration) {
                Debug.Log($"Only moved {errorDelta}. Therefore i give up solving");
                break;
            }
            */
        }
        /*
        if (showDebug) {
        if (iteration == maxIterations) Debug.Log($"{endEffector.gameObject.name} could not solve with {iteration} iterations. The error is {error}");
        if (iteration != maxIterations && iteration > 0) Debug.Log($"{endEffector.gameObject.name} completed CCD with {iteration} iterations and an error of {error}");
        */
    }

    static void solveJointCCD(IKJoint joint, ref Transform endEffector, ref TargetInfo target, float weight, float singularityRadius, bool adjustToTargetNormal) {
        Vector3 rotPoint = joint.transform.position;
        Vector3 toEnd;
        Vector3 toTarget;
        Vector3 rotAxis;

        rotAxis = joint.getRotationAxis();
        toEnd = Vector3.ProjectOnPlane((endEffector.position - rotPoint), rotAxis);
        toTarget = Vector3.ProjectOnPlane(target.position - rotPoint, rotAxis);

        // If singularity, skip.
        if (toTarget == Vector3.zero || toEnd == Vector3.zero) return;
        if (toTarget.magnitude < singularityRadius) return;

        float angle;

        //This is a special case, where i want the foot, that is the last joint of the chain to adjust to the normal it hit
        if (adjustToTargetNormal) {
            angle = -joint.angleToNormal - 90.0f - Vector3.SignedAngle(Vector3.ProjectOnPlane(target.normal, rotAxis), toEnd, rotAxis);
        }
        else {
            angle = weight * joint.getWeight() * Vector3.SignedAngle(toEnd, toTarget, rotAxis);
        }
        joint.ApplyRotation(angle);
    }

    public Vector3 getEndeffectorVelocity() {
        return (endEffector.position - lastEndeffectorPos) / Time.deltaTime;
    }

    public void unpauseSolving() {
        pause = false;
    }

    public void pauseSolving() {
        pause = true;
    }

    public float getTolerance() {
        return transform.lossyScale.y * tolerance;
    }

    public float getError() {
        return transform.lossyScale.y * error;
    }

    public float getSingularityRadius() {
        return transform.lossyScale.y * singularityRadius;
    }

    private static int mod(int n, int m) {
        return ((n % m) + m) % m;
    }

    public IKJoint getRootJoint() {
        return joints[0];
    }

    public Transform getEndEffector() {
        return endEffector;
    }

    public float calculateChainLength() {
        float chainLength = 0;
        for (int i = 0; i < joints.Count; i++) {
            Vector3 p = joints[i].transform.position;
            Vector3 q = (i != joints.Count - 1) ? joints[i + 1].transform.position : endEffector.position;
            chainLength += Vector3.Distance(p, q);
        }
        return chainLength;
    }
}