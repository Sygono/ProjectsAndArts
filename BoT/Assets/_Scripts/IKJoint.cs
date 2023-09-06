using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum RotationAxis {
    LocalX,
    LocalY,
    LocalZ,
}

public class IKJoint : MonoBehaviour
{
    [Header("Joint")]
    [Range(0.0f, 1.0f)]
    public float weight = 1.0f;
    [Range(-180, 0)]
    public float minAngle = -45;
    [Range(0, 180)]
    public float maxAngle = 45;
    //[Range(-180, 180)]
    //public float defaultAngle;

    public float angleToNormal;

    public RotationAxis rotationAxis;

    private Vector3 rotationAxisLocal;
    private Vector3 defaultNormalLocal;

    private float currentAngle;

    public bool showDebug = true;
    public bool useRotationLimits = true;

    public float scale = 2f;

    void Reset() {
        Setup();
    }

    void OnValidate() {
        Setup();
    }

    public void Setup() {
        switch (rotationAxis) {
            case RotationAxis.LocalX:
                rotationAxisLocal = new Vector3(1,0,0);
                break;
            case RotationAxis.LocalY:
                rotationAxisLocal = new Vector3(0,1,0);
                break;
            case RotationAxis.LocalZ:
                rotationAxisLocal = new Vector3(0,0,1);
                break;
            default:
                break;
        }
        Vector3 rotationAxisGlobal = getRotationAxis();
        Vector3 up = Vector3.ProjectOnPlane(transform.up, rotationAxisGlobal);
        angleToNormal = Vector3.SignedAngle(Vector3.up, up, rotationAxisGlobal);
        defaultNormalLocal = transform.InverseTransformDirection(transform.up);
        angleToNormal+=90;
        angleToNormal=-angleToNormal;
        //UpdateConstrain();
        //Debug.Log(angleToNormal);
    }

    void Awake() {
        Setup();
    }

    public void ApplyRotation(float angle) {
        angle = angle % 360;

        Vector3 rotationAxis = getRotationAxis();
        Vector3 rotPoint = transform.position;

        if (useRotationLimits) {
            //The angle gets clamped if its application to the current angle exceeds the limits.
            angle = Mathf.Clamp(currentAngle + angle, minAngle, maxAngle) - currentAngle;
        }

        // Apply the rotation
        transform.RotateAround(rotPoint, rotationAxis, angle);

        // Update the current angle
        currentAngle += angle;

        defaultNormalLocal = Quaternion.AngleAxis(-angle, rotationAxisLocal) * defaultNormalLocal;
        //UpdateConstrain();
    }

    public Vector3 getRotationPoint() {
        return transform.position;
    }

    public Vector3 getRotationAxis() {
        return transform.TransformDirection(rotationAxisLocal);
    }

    public float getWeight() {
        return weight;
    }

    public Vector3 getNormalGlobal() {
        return transform.TransformDirection(defaultNormalLocal);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected() {
        if (!showDebug) {
            return;
        }
        Vector3 rotPoint = transform.position;
        Vector3 rotationAxis = getRotationAxis();
        Vector3 defaultOrientation = getNormalGlobal();

        Vector3 minOrientation = Quaternion.AngleAxis(minAngle, rotationAxis) * defaultOrientation;

        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawSolidArc(rotPoint, rotationAxis, minOrientation, maxAngle - minAngle, 0.15f * scale);

        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawSolidArc(rotPoint, rotationAxis, minOrientation, currentAngle - minAngle, 0.08f * scale);;

        //UnityEditor.Handles.color = Color.cyan;
        //UnityEditor.Handles.DrawLine(rotPoint, rotPoint+Quaternion.AngleAxis(defaultAngle, rotationAxis) * defaultOrientation, 0.1f * scale);
    }
#endif
}

#if UNITY_EDITOR
 [CustomEditor(typeof(IKJoint))]
 [CanEditMultipleObjects]
 public class IKEditor : Editor{

	public override void OnInspectorGUI () {
        IKJoint joint = (IKJoint)target;
        //joint.Setup();
        SceneView.RepaintAll();
        //joint.defaultAngle = Mathf.Clamp(joint.defaultAngle, joint.minAngle, joint.maxAngle);
        /*if (GUILayout.Button("Setup")) {
            joint.Setup();
            SceneView.RepaintAll();
		}*/

		DrawDefaultInspector();
    }
}
#endif
