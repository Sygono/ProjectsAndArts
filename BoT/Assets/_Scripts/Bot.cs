using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IKManager))]
[RequireComponent(typeof(CapsuleCollider))]
[DefaultExecutionOrder(0)]
public class Bot : MonoBehaviour
{

    [Header("IK Legs")]
    public Transform body;
    public List<IKChain> legs = new List<IKChain>();

    [Header("Grounding")]
    public CapsuleCollider capsuleCollider;

    [Header("Balancing")]
    [Range(0,1)]
    public float legBalanceWeight;
    public float legAdjustSpeed = 10f;
    public bool legCentroidAdjustment;
    public bool legNormalAdjustment;
    public float heightWeight;

    Vector3 localCentroid;
    Vector3 defaultOffset;
    
    Vector3 lastPosition;
    float defaultSpan;
    public Vector3 velocity;

    private enum RayType { None, ForwardRay, DownRay };
    private struct groundInfo {
        public bool isGrounded;
        public Vector3 groundNormal;
        public float distanceToGround;
        public RayType rayType;

        public groundInfo(bool isGrd, Vector3 normal, float dist, RayType m_rayType) {
            isGrounded = isGrd;
            groundNormal = normal;
            distanceToGround = dist;
            rayType = m_rayType;
        }
    }

    [Header("Ray Adjustments")]
    public float groundHeight;
    [Range(0.0f, 1.0f)]
    public float forwardRayLength;
    [Range(0.0f, 1.0f)]
    public float downRayLength;
    [Range(0.1f, 1.0f)]
    public float forwardRaySize = 0.66f;
    [Range(0.1f, 1.0f)]
    public float downRaySize = 0.9f;
    private float downRayRadius;

    [Header("Ground Check")]
    public LayerMask walkableLayer;
    public bool groundCheckOn;
    [Range(1, 10)]
    public float groundNormalAdjustSpeed;
    [Range(1, 10)]
    public float forwardNormalAdjustSpeed;
    private groundInfo grdInfo;
    private SphereCast downRay, forwardRay;
    private RaycastHit hitInfo;
    private Vector3 lastNormal;

    public Vector3 heightOffset;

    void Awake() {
        Vector3 legCentroid = getLegsCentroid();
        defaultSpan = getLegsSpan();
        // Calculate the normal and tangential translation needed
        defaultOffset = transform.InverseTransformDirection(body.position - legCentroid);
        lastPosition = transform.position;

        downRayRadius = downRaySize * getColliderRadius();
        float forwardRayRadius = forwardRaySize * getColliderRadius();
        downRay = new SphereCast(transform.position,transform.position-transform.up*downRayLength * getColliderLength(), downRayRadius, transform, transform);
        forwardRay = new SphereCast(transform.position+transform.up*groundHeight, transform.position+transform.forward*forwardRayLength * getColliderLength()+transform.up*groundHeight, forwardRayRadius, transform, transform);
    }

    public void Reset() {
        body = this.transform;
        for (int i=0; i<2; i++) {
            body = body.GetChild(0);
        }
        capsuleCollider = GetComponent<CapsuleCollider>();
    }
    
    void FixedUpdate() {
        //** Ground Check **//
        grdInfo = GroundCheck();

        //** Rotation to normal **// 
        float normalAdjustSpeed = (grdInfo.rayType == RayType.ForwardRay) ? forwardNormalAdjustSpeed : groundNormalAdjustSpeed;

        Vector3 slerpNormal = Vector3.Slerp(transform.up, grdInfo.groundNormal, 0.02f * normalAdjustSpeed);
        Quaternion goalrotation = getLookRotation(Vector3.ProjectOnPlane(transform.right, slerpNormal), slerpNormal);

        // Save last Normal for access
        lastNormal = transform.up;

        //Apply the rotation to the spider
        if (Quaternion.Angle(transform.rotation,goalrotation)>Mathf.Epsilon) transform.rotation = goalrotation;
    }

    private groundInfo GroundCheck() {
        if (groundCheckOn) {
            if (forwardRay.castRay(out hitInfo, walkableLayer)) {
                return new groundInfo(true, hitInfo.normal.normalized, Vector3.Distance(transform.TransformPoint(capsuleCollider.center), hitInfo.point) - getColliderRadius(), RayType.ForwardRay);
            }

            if (downRay.castRay(out hitInfo, walkableLayer)) {
                return new groundInfo(true, hitInfo.normal.normalized, Vector3.Distance(transform.TransformPoint(capsuleCollider.center), hitInfo.point) - getColliderRadius(), RayType.DownRay);
            }
        }
        return new groundInfo(false, Vector3.up, float.PositiveInfinity, RayType.None);
    }

    private Quaternion getLookRotation(Vector3 right, Vector3 up) {
        if (up == Vector3.zero || right == Vector3.zero) return Quaternion.identity;
        // If vectors are parallel return identity
        float angle = Vector3.Angle(right, up);
        if (angle == 0 || angle == 180) return Quaternion.identity;
        Vector3 forward = Vector3.Cross(right, up);
        return Quaternion.LookRotation(forward, up);
    }


    void Update() {
        Balance();
        UpdateVelocity();
    }

    private void UpdateVelocity() {
        lastPosition = transform.position;
    }

    private void Balance() {

        //Debug.Log(transform.position - body.position);
        Vector3 currentCentroid = body.position;
        Vector3 newCentroid = getLegsCentroid() + transform.TransformDirection(defaultOffset);
        newCentroid = Vector3.Lerp(currentCentroid, newCentroid, legBalanceWeight);
        DebugShapes.DrawPoint(newCentroid, 0.5f, Color.cyan);

        if (legCentroidAdjustment) currentCentroid = Vector3.Lerp(currentCentroid, newCentroid, Time.deltaTime * legAdjustSpeed);
        transform.position = currentCentroid;//Vector3.ProjectOnPlane(currentCentroid-transform.position, transform.forward);


        if (heightWeight>0) {
            float spanOffset = defaultSpan - getLegsSpan();
            Debug.Log(spanOffset);
            Vector3 heightCentroid = getLegsCentroid() + transform.TransformDirection(defaultOffset) + transform.up*spanOffset*heightWeight;
            DebugShapes.DrawPoint(heightCentroid, 1f, Color.red);
            heightOffset = heightCentroid - transform.position;
            Debug.Log(heightOffset);
            transform.position += heightOffset;
        }
        
        //Debug.Log(defaultSpan);
        //Debug.Log(spanOffset);
        
        //transform.position+=transform.up*spanOffset*heightWeight*0.01f;

        if (legNormalAdjustment) {
            Vector3 Y = transform.up;
            Vector3 newNormal = getLegsPlaneNormal();

            //Use Global X for  pitch
            Vector3 X = transform.right;
            float angleX = Vector3.SignedAngle(Vector3.ProjectOnPlane(Y, X), Vector3.ProjectOnPlane(newNormal, X), X);
            angleX = Mathf.LerpAngle(0, angleX, Time.deltaTime * legAdjustSpeed);
            transform.rotation = Quaternion.AngleAxis(angleX, X) * transform.rotation;

            //Use Local Z for roll. With the above global X for pitch, this avoids any kind of yaw happening.
            Vector3 Z = transform.forward;
            float angleZ = Vector3.SignedAngle(Vector3.ProjectOnPlane(Y, Z), Vector3.ProjectOnPlane(newNormal, Z), Z);
            angleZ = Mathf.LerpAngle(0, angleZ, Time.deltaTime * legAdjustSpeed);
            transform.rotation = Quaternion.AngleAxis(angleZ, Z) * transform.rotation;
        }
    }

    private Vector3 getLegsPlaneNormal() {

        if (legs == null) {
            Debug.LogError("Cant calculate normal, legs not assigned.");
            return transform.up;
        }

        if (legBalanceWeight <= 0f) return transform.up;

        Vector3 newNormal = transform.up;
        Vector3 toEnd;
        Vector3 currentTangent;

        for (int i = 0; i < legs.Count; i++) {
            if (!legs[i].target.grounded) continue;
            toEnd = legs[i].endEffector.position - transform.position;
            currentTangent = Vector3.ProjectOnPlane(toEnd, transform.up);

            if (currentTangent == Vector3.zero) continue; // Actually here we would have a 90degree rotation but there is no choice of a tangent.

            newNormal = Quaternion.Lerp(Quaternion.identity, Quaternion.FromToRotation(currentTangent, toEnd), legBalanceWeight) * newNormal;
        }
        return newNormal.normalized;
    }

    private Vector3 getLegsCentroid() {

        Vector3 newCentroid = Vector3.zero;
        float k = 0;
        for (int i = 0; i < legs.Count; i++) {
            //if (!legs[i].target.grounded) continue;
            newCentroid += legs[i].endEffector.position;
            k++;
        }
        newCentroid = newCentroid / k;

       
        return newCentroid;
    }

    private float getLegsSpan() {
        float span = 0;
        float k = 0;
        for (int i = 0; i < legs.Count; i++) {
            span += Vector3.ProjectOnPlane(legs[i].endEffector.position - transform.position, transform.up).magnitude;
            k++;
        }
        span = span / k;

       
        return span;
    }


    public Vector3 getColliderBottomPoint() {
        return transform.TransformPoint(capsuleCollider.center - capsuleCollider.radius * new Vector3(0, 1, 0));
    }

    public float getColliderRadius() {
        return capsuleCollider.radius;
    }

    public float getColliderLength() {
        return capsuleCollider.height;
    }

    public float getScale() {
        return transform.lossyScale.y;
    }

    public Vector3 getDirection() {
        return transform.forward.normalized;
    }


    private void drawDebug() {
        //Draw the two Sphere Rays
        downRay.draw(Color.green);
        forwardRay.draw(Color.blue);

        //Draw the Gravity off distance
        Vector3 borderpoint = getColliderBottomPoint();
        //Debug.DrawLine(borderpoint, borderpoint + getGravityOffDistance() * -transform.up, Color.magenta);

        //Draw the current transform.up and the bodys current Y orientation
        Debug.DrawLine(transform.position, transform.position + 2f * getColliderRadius() * transform.up, new Color(1, 0.5f, 0, 1));
        //Debug.DrawLine(transform.position, transform.position + 2f * getColliderRadius() * body.TransformDirection(bodyY), Color.blue);

        //Draw the Centroids 
        //DebugShapes.DrawPoint(getDefaultCentroid(), Color.magenta, 0.1f);
        //DebugShapes.DrawPoint(getLegsCentroid(), Color.red, 0.1f);
        //DebugShapes.DrawPoint(getColliderBottomPoint(), Color.cyan, 0.1f);
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {
        Awake();
        drawDebug();
    }
#endif
}
