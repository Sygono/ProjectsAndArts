using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum CastMode {
        RayCast,
        SphereCast
    }

    public abstract class Cast {
        public Vector3 start;
        public Vector3 end;
        public Transform parentStart;
        public Transform parentEnd;

        public Cast(Vector3 _start, Vector3 _end, Transform _parentStart = null, Transform _parentEnd = null) {
            start = _start;
            end = _end;
            parentStart = _parentStart;
            parentEnd = _parentEnd;
        }

        public Vector3 getDirection() {
            return end - start;
        }

        public abstract bool castRay(out RaycastHit hitInfo, LayerMask layerMask, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore);

        public abstract void draw(Color col, float duration=0);
    }

    public class RayCast : Cast {

        public RayCast(Vector3 _start, Vector3 _end, Transform _parentStart = null, Transform _parentEnd = null) : base(_start,_end,_parentStart,_parentEnd) {
        }

        public override void draw(Color col, float duration=0) {
            Debug.DrawLine(start, end, col,duration);
        }

        public override bool castRay(out RaycastHit hitInfo, LayerMask layerMask, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore) {
            Vector3 v = getDirection();
            return Physics.Raycast(start, v.normalized, out hitInfo, v.magnitude, layerMask, q);
        }
    }

    public class SphereCast : Cast {
        public float radius;

        public SphereCast(Vector3 _start, Vector3 _end, float _radius, Transform _parentStart = null, Transform _parentEnd = null) : base(_start,_end,_parentStart,_parentEnd) {
            radius = _radius;
        }

        public override void draw(Color col,float duration=0) {
            DebugShapes.DrawSphereRay(start, end, radius, 5, col,duration);
        }

        public override bool castRay(out RaycastHit hitInfo, LayerMask layerMask, QueryTriggerInteraction q = QueryTriggerInteraction.Ignore) {
            Vector3 v = getDirection();
            return Physics.SphereCast(start, radius, v.normalized, out hitInfo, v.magnitude, layerMask, q);
        }
    }


[RequireComponent(typeof(IKChain))]
public class IKStepper : MonoBehaviour
{
    public Bot bot;
    public IKChain chain;

    [Header("Target Mode")]
    public TargetMode targetMode;
    public Transform targetObject;

    [Header("Stepping Information")]
    [Range(0f, 10f)]
    public float overShoot;
    public float tolerance = 1;
    public IKStepper[] asyncChain;
    public bool isStepping;
    public bool needStepping;

    [Header("Step Layer")]
    public LayerMask stepLayer;

    [Header("Step Timing")]
    [Range(0.0f, 2.0f)]
    public float stepTime = 0.5f;
    [Range(0.0f, 5.0f)]
    public float stepCooldown = 0.0f;
    [Range(0.0f, 2.0f)]
    public float stopSteppingAfterSecondsStill;

    [Header("Step Transition")]
    [Range(0.0f, 10.0f)]
    public float stepHeight;
    public AnimationCurve stepAnimation;

    [Header("SphereCast Config")]
    [Range(0f, 10f)]
    public float radius;

    [Header("Default Position")]
    [Range(-1.0f, 1.0f)]
    public float defaultOffsetLength;
    [Range(-1.0f, 1.0f)]
    public float defaultOffsetHeight;
    [Range(-1.0f, 1.0f)]
    public float defaultOffsetStride;

    [Header("Downwards Ray")]
    [Range(0f, 6f)]
    public float downRayHeight;
    [Range(0f, 6f)]
    public float downRayDepth;

    [Header("Frontal Ray")]
    public Vector3 rayFrontFocalPoint;
    [Range(0f, 2f)]
    public float rayFrontalHeight;
    [Range(0f, 2f)]
    public float rayFrontalEndOffset;
    [Range(0f, 2f)]
    public float rayFrontalOriginOffset;
    private Vector3 frontalStartPositionLocal;

    [Header("Outward Ray")]
    public Vector3 rayTopFocalPoint;
    [Range(0f, 1f)]
    public float rayOutwardsOriginOffset;
    [Range(0f, 1f)]
    public float rayOutwardsEndOffset;

    [Header("Inwards Ray")]
    [Range(1, 10)]
    public int Count = 3;
    public Vector3 rayBottomFocalPoint;
    [Range(0f, 1f)]
    public float rayInwardsOriginOffset;
    [Range(0f, 3f)]
    public float rayInwardsEndOffset;
    [Range(0f, 1f)]
    public float rayEndAngleOffset;
    [Range(0f, 1f)]
    public float rayStartAngleOffset;
    public AnimationCurve lengthCurve;

    [Header("Solving")]
    private Dictionary<string, Cast> casts;
    private Dictionary<string, Cast> predictionCasts;
    RaycastHit hitInfo;
    private IKJoint rootJoint;
    private float chainLength;
    private float minDistance;

    Vector3 defaultPositionLocal;
    Vector3 prediction;
    float lastResortHeight;
    string lastHitRay;

    public TargetInfo defaultTarget;
    public TargetInfo desiredTarget;

    [Header("Debug Values")]
    bool setup;
    public float  scale;
    public Vector3 minOrientLocal;
    public Vector3 maxOrientLocal;


    public void Reset() {
        Transform current = transform;
        chain = current.GetComponent<IKChain>();
        while(current != null) {
            Bot bot = current.GetComponent<Bot>();
            if (bot != null) {
                if (bot.legs == null) {
                    bot.legs = new List<IKChain>();
                }
                bot.legs.Add(chain);
                this.bot = bot;
                IKManager manager = bot.GetComponent<IKManager>();
                manager.steppers.Add(this);
                if (transform.localPosition.x>0) {
                    manager.right.Add(this);
                } else {
                    manager.left.Add(this);
                }
                break;
            }
            current = current.parent;
        }
    }

    void OnValidate() {
        Setup();
    }

    public void Setup() {
        rootJoint = chain.getRootJoint();

        //Let the chainlength be calculated and set it for future access
        chainLength = chain.calculateChainLength();

        //Set the distance which the root joint and the endeffector are allowed to have. If below this distance, stepping is forced.
        minDistance = 0.2f * chainLength;

        //Set Default Position
        defaultPositionLocal = calculateDefault();
        frontalStartPositionLocal = new Vector3(0, rayFrontalHeight * bot.getColliderRadius() * bot.getScale() *0.01f, 0);

        prediction = getDefault();
        defaultTarget = getDefaultTarget();

        // Initialize Casts as either RayCast or SphereCast 
        casts = new Dictionary<string, Cast>();
        predictionCasts = new Dictionary<string, Cast>();
        updateCasts();
        UpdatePrediction();

        //Set Start Target for chain
        chain.setTarget(getDefaultTarget());
    }

    private void Awake() {
        Setup();
    }

    private Vector3 calculateDefault() {
        float diameter = chainLength - minDistance;
        Vector3 rootRotAxis = rootJoint.getRotationAxis();

        //Be careful with the use of transform.up and rootjoint.getRotationAxis(). In my case they are equivalent with the exception of the right side being inverted.
        //However they can be different and this has to be noticed here. The code below is probably wrong for the general case.
        Vector3 normal = bot.body.up;

        Vector3 toEnd = chain.getEndEffector().position - rootJoint.transform.position;
        toEnd = Vector3.ProjectOnPlane(toEnd, normal).normalized;

        Vector3 pivot = bot.getColliderBottomPoint() + Vector3.ProjectOnPlane(rootJoint.transform.position - bot.body.position, normal);

        //Set the following debug variables for the DOF Arc
        minOrientLocal = bot.body.InverseTransformDirection(Quaternion.AngleAxis(rootJoint.minAngle, rootRotAxis) * toEnd);
        maxOrientLocal = bot.body.InverseTransformDirection(Quaternion.AngleAxis(rootJoint.maxAngle, rootRotAxis) * toEnd);

        // Now set the default position using the given stride, length and height
        Vector3 defOrientation = Quaternion.AngleAxis((defaultOffsetStride + 1) * 0.5f *(rootJoint.maxAngle-rootJoint.minAngle), rootRotAxis) * minOrientLocal;
        pivot += (minDistance + 0.5f * (1f + defaultOffsetLength) * diameter) * defOrientation;
        pivot += defaultOffsetHeight * bot.getColliderRadius() * bot.getScale() * rootRotAxis;
        return bot.body.InverseTransformPoint(pivot);
    }

    void Update() {
        if (targetMode == TargetMode.Calculated) {
            desiredTarget = FindTarget();
        } else if (targetMode == TargetMode.Manual&&targetObject!=null){
            desiredTarget.position = targetObject.transform.position;
            desiredTarget.normal = targetObject.transform.up;
            desiredTarget.grounded = true;
        }
        DebugShapes.DrawPoint(desiredTarget.position, scale, Color.red);
        DebugShapes.DrawSphere(desiredTarget.position, getTolerance(), Color.white);
        needStepping = stepCheck();
        //Debug.Log(needStepping);

        if (targetMode == TargetMode.Manual&&targetObject!=null&&needStepping) {
            step();
        }

        //if (targetMode == TargetMode.Calculated&&needStepping) {
            //step(stepTime);
        //}
        //chain.target = desiredTarget;
    }

    private void updateCasts() {
        Vector3 defaultPos = getDefault();
        Vector3 up = bot.body.up;

        //Frontal Parameters
        Vector3 frontalPos = getFrontalStartPosition();
        Vector3 toFront = Vector3.ProjectOnPlane(defaultPos - frontalPos, bot.body.up).normalized;
        Vector3 frontalDefaultEnd = frontalPos + toFront * rayFrontalEndOffset * chainLength;
        Vector3 frontalDefaultOrigin = frontalPos + toFront * rayFrontalOriginOffset * chainLength;

        
        //Outwards Parameters
        Vector3 topPos = getTopFocalPoint();
        Vector3 toEnd = (defaultPos - topPos).normalized;
        Vector3 outwardsDefaultEnd = Vector3.Lerp(defaultPos, defaultPos+toEnd*chainLength, rayOutwardsEndOffset);
        Vector3 outwardsDefaultOrigin = Vector3.Lerp(topPos, defaultPos, rayOutwardsOriginOffset);


        //Downwards Parameters
        float height = downRayHeight * bot.getColliderRadius();
        float depth = downRayDepth * bot.getColliderRadius();
        Vector3 downwardsDefaultOrigin = defaultPos + up * height;
        Vector3 downwardsDefaultEnd = defaultPos - up * depth;

        //Inwards Parameters
        Vector3 bottomPos = getBottomFocalPoint();
        Vector3 toBottom = (bottomPos - defaultPos).normalized;
        Vector3 axis = Vector3.Cross(-up, toBottom);
        float totalAngle = Vector3.SignedAngle(-up, toBottom, axis);
        Vector3 inwardsDefaultOrigin = Vector3.Lerp(bottomPos, defaultPos, rayInwardsOriginOffset);
        Vector3[] inwardsDefaultEnd = new Vector3[Count];
        Vector3 toAngle = -up;
        toAngle = Quaternion.AngleAxis(totalAngle*rayStartAngleOffset, axis) * toAngle;
        totalAngle=totalAngle*(rayEndAngleOffset-rayStartAngleOffset);
        for (int i=0; i<Count; i++) {
            inwardsDefaultEnd[i] = defaultPos+toAngle*chainLength*rayInwardsEndOffset;
            toAngle = Quaternion.AngleAxis(totalAngle/Count, axis) * toAngle;
        } 

        float r = bot.getScale() * radius;

        // Note that Prediction Out will never hit a targetpoint on a flat surface or hole since it stop at the prediction point which is on
        // default height, that is the height where the collider stops.

        casts.Clear();
        if (casts.Count == 0) {
            casts.Add("Default Frontal", getCast(frontalDefaultOrigin, frontalDefaultEnd, r));
            casts.Add("Default Outwards", getCast(outwardsDefaultOrigin, outwardsDefaultEnd, r));
            casts.Add("Default Downwards", getCast(downwardsDefaultOrigin, downwardsDefaultEnd, r));
            for (int j=0; j<Count; j++) {
                casts.Add($"Default Inwards{j}", getCast(inwardsDefaultOrigin, inwardsDefaultEnd[j], r));
            } 
        }
    }

    private void UpdatePrediction() {
        Vector3 endEffectorVelocity = chain.getEndeffectorVelocity()*0.1f*overShoot;
        //Debug.Log(endEffectorVelocity);
        prediction = defaultTarget.position + endEffectorVelocity;
        Vector3 up = bot.body.up;

        //Frontal Parameters
        Vector3 frontalPos = getFrontalStartPosition();    
        Vector3 toFrontPrediction = Vector3.ProjectOnPlane(prediction - frontalPos, bot.body.up).normalized;
        Vector3 frontalPredictionEnd = frontalPos + toFrontPrediction * rayFrontalEndOffset * chainLength;
        Vector3 frontalPredictionOrigin = frontalPos + toFrontPrediction * rayFrontalOriginOffset * chainLength;
        
        //Outwards Parameters
        Vector3 topPos = getTopFocalPoint();
        Vector3 toEndPrediction = (prediction - topPos).normalized;

        Vector3 outwardsPredictionOrigin = Vector3.Lerp(prediction, prediction+toEndPrediction*chainLength, rayOutwardsEndOffset);
        Vector3 outwardsPredictionEnd = Vector3.Lerp(topPos, prediction, rayOutwardsOriginOffset);

        //Downwards Parameters
        float height = downRayHeight * bot.getColliderRadius();
        float depth = downRayDepth * bot.getColliderRadius();

        Vector3 downwardsPredictionOrigin = prediction + up * height;
        Vector3 downwardsPredictionEnd = prediction - up * depth;

        //Inwards Parameters
        Vector3 bottomPos = getBottomFocalPoint();
        Vector3 toBottomPrediction = (bottomPos - prediction).normalized;
        Vector3 axisPrediction = Vector3.Cross(-up, toBottomPrediction);
        float totalAnglePrediction = Vector3.SignedAngle(-up, toBottomPrediction, axisPrediction);
        Vector3 inwardsPredictionOrigin = Vector3.Lerp(bottomPos, prediction, rayInwardsOriginOffset);
        Vector3[] inwardsPredictionEnd = new Vector3[Count];
        Vector3 toAnglePrediction = -up;
        toAnglePrediction = Quaternion.AngleAxis(totalAnglePrediction*rayStartAngleOffset, axisPrediction) * toAnglePrediction;
        totalAnglePrediction=totalAnglePrediction*(rayEndAngleOffset-rayStartAngleOffset);
        for (int i=0; i<Count; i++) {
            inwardsPredictionEnd[i] = prediction+toAnglePrediction*chainLength*rayInwardsEndOffset;
            toAnglePrediction = Quaternion.AngleAxis(totalAnglePrediction/Count, axisPrediction) * toAnglePrediction;
        } 

        float r = bot.getScale() * radius;
        predictionCasts.Clear();
        if (predictionCasts.Count == 0) {
            predictionCasts.Add("Prediction Frontal", getCast(frontalPredictionOrigin, frontalPredictionEnd, r));
            predictionCasts.Add("Prediction Outwards", getCast(outwardsPredictionOrigin, outwardsPredictionEnd, r));
            predictionCasts.Add("Prediction Downwards", getCast(downwardsPredictionOrigin, downwardsPredictionEnd, r));
            for (int j=0; j<Count; j++) {
                predictionCasts.Add($"Prediction Inwards{j}", getCast(inwardsPredictionOrigin, inwardsPredictionEnd[j], r));
            } 
        }
    }

    private TargetInfo FindTarget() {
        //Debug.Log(casts["Default Frontal"].start);
        updateCasts();
        bool found = false;
        foreach (var cast in casts) {
            //If the spider cant see the ray origin there is no need to cast: Consider using a different layer here, since there can be objects in the way that are not walkable surfaces.
            if (new RayCast(bot.body.position, cast.Value.start).castRay(out hitInfo, stepLayer)) continue;

            if (cast.Value.castRay(out hitInfo, stepLayer)) {
                //For the frontal ray we only allow not too steep slopes, that is +-65°
                if (cast.Key == "Default Frontal" && Vector3.Angle(cast.Value.getDirection(), hitInfo.normal) < 180f - 65f) continue;

                //Debug.Log($"Got a target point from the cast {cast.Key}");
                lastHitRay = cast.Key;
                defaultTarget =  new TargetInfo(hitInfo.point, hitInfo.normal, true);
                found = true;
                break;
            }
        }

        if (!found) return getDefaultTarget();

        UpdatePrediction();
        foreach (var cast in predictionCasts) {
            //If the spider cant see the ray origin there is no need to cast: Consider using a different layer here, since there can be objects in the way that are not walkable surfaces.
            if (new RayCast(bot.body.position, cast.Value.start).castRay(out hitInfo, stepLayer)) continue;

            if (cast.Value.castRay(out hitInfo, stepLayer)) {
                //For the frontal ray we only allow not too steep slopes, that is +-65°
                if (cast.Key == "Prediction Frontal" && Vector3.Angle(cast.Value.getDirection(), hitInfo.normal) < 180f - 65f) continue;

                //Debug.Log($"Got a target point from the cast {cast.Key}");
                lastHitRay = cast.Key;
                return new TargetInfo(hitInfo.point, hitInfo.normal, true);
            }
        }
        return defaultTarget;
    }

    private bool stepCheck() {
        if (isStepping) return false;
        if (!desiredTarget.grounded) return false;
        //If current target not grounded step
        if (!chain.target.grounded) return true;

        //If the error of the IK solver gets too big, that is if it cant solve for the current target appropriately anymore, step.
        // This is the main way this class determines if it needs to step.

        else if (getError() > getTolerance()) return true;
        // Alternativaly step if too close to root joint
        else if (Vector3.Distance(rootJoint.transform.position, chain.target.position) < minDistance) return true;

        return false;
    }

    public void step() {
        StopAllCoroutines(); // Make sure it is ok to just stop the coroutine without finishing it
        StartCoroutine(Step(stepTime));
    }

    private IEnumerator Step(float stepTime) {
        isStepping = true;
        float timer = 0.0f;
        float animTime;
        
        TargetInfo startTarget = new TargetInfo(chain.target.position, chain.target.normal, true);
        //TargetInfo endTarget = new TargetInfo(desiredTarget.position, desiredTarget.normal, desiredTarget.grounded); 
        TargetInfo stepTarget = new TargetInfo();

        TargetInfo endTarget = desiredTarget;
        DebugShapes.DrawPoint(endTarget.position, scale*0.5f, Color.green, stepTime);

        while (timer < stepTime)
        {
            animTime = timer/stepTime;

            stepTarget.position = Vector3.Lerp(startTarget.position, endTarget.position, animTime);
            //Debug.Log(startTarget.position);

            Vector3 stepUpDir = bot.body.up;
            stepTarget.position += stepUpDir * stepAnimation.Evaluate(animTime) * stepHeight * transform.lossyScale.y * 0.01f; // Upward direction of tip vector

            stepTarget.normal = Vector3.Lerp(startTarget.normal, endTarget.normal, animTime);

            chain.setTarget(stepTarget);

            timer += Time.deltaTime;

            yield return null;
        }
        chain.setTarget(endTarget);
        chain.target.grounded = true;
        isStepping = false;
    }



    private float getError() {
        return Vector3.Distance(chain.target.position, desiredTarget.position);
    }

    private float getTolerance() {
        return transform.lossyScale.y*tolerance*0.01f;
    }

    private Cast getCast(Vector3 start, Vector3 end, float radius, Transform parentStart = null, Transform parentEnd = null) {
        return new RayCast(start, end, parentStart, parentEnd);
    }
    private Vector3 getDefault() {
        return bot.body.TransformPoint(defaultPositionLocal)-bot.heightOffset;
    }
    private TargetInfo getDefaultTarget() {
        return new TargetInfo(getDefault(), bot.body.up, false);
    }
    private Vector3 getFrontalStartPosition() {
        return bot.body.TransformPoint(frontalStartPositionLocal+rayFrontFocalPoint);
    }
    private Vector3 getTopFocalPoint() {
        return bot.body.TransformPoint(rayTopFocalPoint);
    }
    private Vector3 getBottomFocalPoint() {
        return bot.body.TransformPoint(rayBottomFocalPoint);
    }

    private void drawDebug(bool points = true, bool steppingProcess = true, bool rayCasts = true, bool DOFArc = true) {

        if (points) {
            // Default Position
            DebugShapes.DrawPoint(getDefault(), scale, Color.magenta);
            //Target Point
            DebugShapes.DrawPoint(chain.target.position, scale*0.3f, Color.cyan, 0.2f);
        }

        if (rayCasts) {
            if (casts != null) {
            Color col = Color.magenta;
            foreach (var cast in casts) {
                cast.Value.draw(col);
                DebugShapes.DrawPoint(cast.Value.start, scale*0.5f, Color.yellow);
            }
            }

            if (predictionCasts != null) {
            Color col = Color.yellow;
            foreach (var cast in predictionCasts) {
                cast.Value.draw(col);
                DebugShapes.DrawPoint(cast.Value.start, scale*0.5f, Color.yellow);
            }
            }
        }

        if (DOFArc) {
            Vector3 v = bot.body.TransformDirection(minOrientLocal);
            Vector3 w = bot.body.TransformDirection(maxOrientLocal);
            Vector3 p = bot.body.InverseTransformPoint(rootJoint.transform.position);
            p.y = defaultPositionLocal.y;
            p = bot.body.TransformPoint(p);
            DebugShapes.DrawCircleSection(p, v, w, rootJoint.getRotationAxis(), minDistance, chainLength, Color.red);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected() {
        drawDebug();
    }
#endif
}

#if UNITY_EDITOR
 [CustomEditor(typeof(IKStepper))]
 [CanEditMultipleObjects]
 public class StepEditor : Editor{

	public override void OnInspectorGUI () {
        IKStepper stepper = (IKStepper)target;
        stepper.Setup();
        SceneView.RepaintAll();
        //joint.defaultAngle = Mathf.Clamp(joint.defaultAngle, joint.minAngle, joint.maxAngle);
        /*if (GUILayout.Button("Setup")) {
            stepper.Setup();
            SceneView.RepaintAll();
		}*/

		DrawDefaultInspector();
    }
}
#endif

#if UNITY_EDITOR

public class StepperWindow : EditorWindow{

    [MenuItem("Window/Stepper")]
    public static void ShowWindow() {
        GetWindow<StepperWindow>("Stepper");
    }

    void OnGUI() {
        GameObject firstObj = Selection.activeGameObject;
        if (firstObj == null) return;
        IKStepper first = firstObj.GetComponent<IKStepper>();
        if (first == null) return;
        
        if (GUILayout.Button("Copy Stepper Data")) {
            foreach (GameObject obj in Selection.gameObjects) {
                IKStepper stepper = obj.GetComponent<IKStepper>();
                if (stepper == null) continue;
                stepper.bot = first.bot;
                stepper.tolerance = first.tolerance;
                stepper.asyncChain = first.asyncChain;
                stepper.stepLayer = first.stepLayer;
                stepper.stepCooldown = first.stepCooldown;
                stepper.stopSteppingAfterSecondsStill = first.stopSteppingAfterSecondsStill;
                stepper.stepHeight = first.stepHeight;

                stepper.stepAnimation = first.stepAnimation;
                stepper.radius = first.radius;
                stepper.defaultOffsetLength = first.defaultOffsetLength;
                stepper.defaultOffsetHeight = first.defaultOffsetHeight;
                stepper.defaultOffsetStride = first.defaultOffsetStride;

                stepper.downRayHeight = first.downRayHeight;
                stepper.downRayDepth = first.downRayDepth;
                stepper.rayFrontFocalPoint = first.rayFrontFocalPoint;
                stepper.rayFrontalHeight = first.rayFrontalHeight;
                stepper.rayFrontalEndOffset = first.rayFrontalEndOffset;
                stepper.rayFrontalOriginOffset = first.rayFrontalOriginOffset;
                stepper.rayTopFocalPoint = first.rayTopFocalPoint;
                stepper.rayOutwardsOriginOffset = first.rayOutwardsOriginOffset;
                stepper.rayOutwardsEndOffset = first.rayOutwardsEndOffset;

                stepper.Count = first.Count;
                stepper.rayBottomFocalPoint = first.rayBottomFocalPoint;
                stepper.rayInwardsOriginOffset = first.rayInwardsOriginOffset;
                stepper.rayInwardsEndOffset = first.rayInwardsEndOffset;
                stepper.rayStartAngleOffset = first.rayStartAngleOffset;
                stepper.rayEndAngleOffset = first.rayEndAngleOffset;
                stepper.rayFrontalOriginOffset = first.rayFrontalOriginOffset;
                stepper.scale = first.scale;
                stepper.lengthCurve = first.lengthCurve;

                stepper.Setup();
                SceneView.RepaintAll();
            }
        }
    }
}
#endif