using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Recorder;
using UnityEngine.XR;

public class openMocapActor : MonoBehaviour
{

    Animator rig;
    Vector2 smoothDeltaPosition = Vector2.zero;
    Vector2 velocity = Vector2.zero;
    Vector3 LastPosition;

    public float hipOffset = -0.615f; // how far away the hips are from the head - depends per actor
    public Transform hipOffsetNode;
    public Transform headMountedDisplayRoot;
    public Transform skeletonHeadRoot;
    public Transform actorCamera;
    public Transform Hips;
    public Transform Head;
    public Transform leftFoot;
    public Transform rightFoot;

    public Transform leftFootRestingLocation;
    public Transform rightFootRestingLocation;
    public Transform headTarget;
    public Transform leftHand;
    public Transform leftHandOffset;
    public Transform rightHand;
    public Transform rightHandOffset;

    public bool shouldUnlockFeet;
    float triggerPress;
    bool lastIsRecording;
    bool isRecording = false;

    bool canRecord = false;

    RecorderController actorRecorder;
    RecorderControllerSettings actorRecorderSettings;

    AnimationRecorderSettings animationRecorderSettings;


    public float smoothFeetIK;
    Vector3 leftFootPosition;
    Vector3 rightFootPosition;




    void Start()
    {
        XRDevice.SetTrackingSpaceType(TrackingSpaceType.RoomScale);
        rig = GetComponent<Animator>();

    }
    void SetupNewRecorder()
    {
        // creating animation recorder and assigning the camera to it
        animationRecorderSettings = ScriptableObject.CreateInstance("AnimationRecorderSettings") as AnimationRecorderSettings;
        animationRecorderSettings.animationInputSettings.recursive = true;
        animationRecorderSettings.animationInputSettings.gameObject = this.gameObject;
        animationRecorderSettings.animationInputSettings.AddComponentToRecord(typeof(Transform));

        // creating recorder settings and assigning our animation recorder settings to it
        actorRecorderSettings = ScriptableObject.CreateInstance("RecorderControllerSettings") as RecorderControllerSettings;
        actorRecorderSettings.AddRecorderSettings(animationRecorderSettings);
        actorRecorderSettings.SetRecordModeToManual();

        // creating a new recorder instance with the correct settings
        actorRecorder = new RecorderController(actorRecorderSettings);

    }

    void OnAnimatorIK(int layerIndex)
    {

        rig.bodyPosition = Hips.position;
        rig.bodyRotation = Hips.rotation;

        rig.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootRestingLocation.position);
        rig.SetIKPosition(AvatarIKGoal.RightFoot, rightFootRestingLocation.position);

        rig.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
        rig.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);

        rig.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
        rig.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);

        rig.SetIKPosition(AvatarIKGoal.LeftHand, leftHandOffset.position);
        rig.SetIKRotation(AvatarIKGoal.LeftHand, leftHandOffset.rotation);

        rig.SetIKPosition(AvatarIKGoal.RightHand, rightHandOffset.position);
        rig.SetIKRotation(AvatarIKGoal.RightHand, rightHandOffset.rotation);

        rig.SetLookAtWeight(1f, 0.05f, 1f, 0f, 0.6f);
        rig.SetLookAtPosition(headTarget.position);

        rig.SetIKPositionWeight(AvatarIKGoal.LeftFoot, smoothFeetIK);
        rig.SetIKPositionWeight(AvatarIKGoal.RightFoot, smoothFeetIK);

        /*
                if (shouldUnlockFeet == true)
                {
                    rig.SetTrigger("ikOut");
                    rig.SetIKPositionWeight(AvatarIKGoal.LeftFoot, smoothFeetIK);


                    rig.SetIKPositionWeight(AvatarIKGoal.RightFoot, smoothFeetIK);

                }
                else
                {
                    rig.SetTrigger("ikIn");

                    rig.SetIKPositionWeight(AvatarIKGoal.LeftFoot, smoothFeetIK);


                    rig.SetIKPositionWeight(AvatarIKGoal.RightFoot, smoothFeetIK);



                }
                 */


    }

    void Update()
    {
        lastIsRecording = isRecording;

        Head.position = actorCamera.position;
        Head.rotation = actorCamera.rotation;

        Vector3 newActorPosition = new Vector3(actorCamera.position.x, 0f, actorCamera.position.z);
        headMountedDisplayRoot.localPosition = newActorPosition;

        Vector3 newActorRotation = new Vector3(0f, actorCamera.localEulerAngles.y, 0f);
        headMountedDisplayRoot.localEulerAngles = newActorRotation;

        Vector3 hipOffsetNodePosition = new Vector3(Head.position.x, Head.position.y + hipOffset, Head.position.z);
        hipOffsetNode.position = hipOffsetNodePosition;


        Vector3 newHipPosition = new Vector3(Hips.position.x, hipOffsetNode.position.y, Hips.position.z);
        Hips.position = newHipPosition;


        List<XRNodeState> xrNodes = new List<XRNodeState>();
        InputTracking.GetNodeStates(xrNodes);

        for (int i = 0; i < xrNodes.Count; i++)
        {
            // left hand position
            if (xrNodes[i].nodeType == XRNode.LeftHand)
            {
                Vector3 newHandPosition;
                xrNodes[i].TryGetPosition(out newHandPosition);
                leftHand.position = newHandPosition;

                Quaternion newHandRotation;
                xrNodes[i].TryGetRotation(out newHandRotation);
                leftHand.localRotation = newHandRotation;
            }

            // left hand position
            if (xrNodes[i].nodeType == XRNode.LeftHand)
            {
                Vector3 newHandPosition;
                xrNodes[i].TryGetPosition(out newHandPosition);
                leftHand.position = newHandPosition;

                Quaternion newHandRotation;
                xrNodes[i].TryGetRotation(out newHandRotation);
                leftHand.localRotation = newHandRotation;
            }

            // right hand position
            if (xrNodes[i].nodeType == XRNode.RightHand)
            {
                Vector3 newHandPosition;
                xrNodes[i].TryGetPosition(out newHandPosition);
                rightHand.position = newHandPosition;

                Quaternion newHandRotation;
                xrNodes[i].TryGetRotation(out newHandRotation);
                rightHand.localRotation = newHandRotation;
            }

            // recording backend
            if (xrNodes[i].nodeType == XRNode.LeftHand || xrNodes[i].nodeType == XRNode.RightHand)
            {
                InputDevices.GetDeviceAtXRNode(xrNodes[i].nodeType).TryGetFeatureValue(CommonUsages.grip, out smoothFeetIK);
                InputDevices.GetDeviceAtXRNode(xrNodes[i].nodeType).TryGetFeatureValue(CommonUsages.trigger, out triggerPress);

                if (triggerPress > 0.6f)
                {
                    isRecording = true;
                }
                else
                {
                    isRecording = false;
                }
            }

        }

        Vector3 worldDeltaPosition = Hips.position - LastPosition;

        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);

        float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
        smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

        if (Time.deltaTime > 1e-5f)
            velocity = smoothDeltaPosition / Time.deltaTime;

        bool shouldMove = velocity.magnitude > 0.15f;

        //  rig.SetBool("move", shouldMove);

        rig.SetFloat("velx", velocity.x);
        rig.SetFloat("vely", velocity.y);


        //   shouldUnlockFeet = shouldMove;
        LastPosition = Hips.transform.position;



        // recording backend 
        if (canRecord)
        {
            if (isRecording == true && lastIsRecording == false)
            {
                startRecording();
            }
            if (lastIsRecording == true && isRecording == false)
            {
                stopRecording();
            }
        }

    }

    void startRecording()
    {
        SetupNewRecorder();

        if (actorRecorder.IsRecording() == false)
        {
            actorRecorder.StartRecording();
        }

    }

    void stopRecording()
    {
        if (actorRecorder.IsRecording() == true)
        {
            actorRecorder.StopRecording();
        }
    }
}