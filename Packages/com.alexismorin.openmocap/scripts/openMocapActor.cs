using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class openMocapActor : MonoBehaviour {

    Animator rig;
    Vector2 smoothDeltaPosition = Vector2.zero;
    Vector2 velocity = Vector2.zero;
    Vector3 LastPosition;
    public Transform headMountedDisplayRoot;
    public Transform Hips;
    public Transform Head;
    public Transform headTarget;
    public Transform leftHand;
    public Transform rightHand;

    void Start () {
        rig = GetComponent<Animator> ();

    }

    void OnAnimatorIK (int layerIndex) {
        rig.bodyPosition = Hips.position;
        rig.bodyRotation = Hips.rotation;

        //    rig.SetIKPositionWeight (AvatarIKGoal.LeftFoot, 1f);
        //     rig.SetIKRotationWeight (AvatarIKGoal.LeftFoot, 1f);

        //     rig.SetIKPositionWeight (AvatarIKGoal.RightFoot, 1f);
        //    rig.SetIKRotationWeight (AvatarIKGoal.RightFoot, 1f);

        rig.SetIKPositionWeight (AvatarIKGoal.LeftHand, 1f);
        rig.SetIKRotationWeight (AvatarIKGoal.LeftHand, 1f);

        rig.SetIKPositionWeight (AvatarIKGoal.RightHand, 1f);
        rig.SetIKRotationWeight (AvatarIKGoal.RightHand, 1f);

        rig.SetIKPosition (AvatarIKGoal.LeftHand, leftHand.position);
        rig.SetIKRotation (AvatarIKGoal.LeftHand, leftHand.rotation);

        rig.SetIKPosition (AvatarIKGoal.RightHand, rightHand.position);
        rig.SetIKRotation (AvatarIKGoal.RightHand, rightHand.rotation);

        rig.SetLookAtWeight (1f, 0.0125f, 1f, 0f, 0.6f);
        rig.SetLookAtPosition (headTarget.position);
    }

    void Update () {
        Vector3 worldDeltaPosition = Hips.position - LastPosition;

        float dx = Vector3.Dot (transform.right, worldDeltaPosition);
        float dy = Vector3.Dot (transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2 (dx, dy);

        float smooth = Mathf.Min (1.0f, Time.deltaTime / 0.15f);
        smoothDeltaPosition = Vector2.Lerp (smoothDeltaPosition, deltaPosition, smooth);

        if (Time.deltaTime > 1e-5f)
            velocity = smoothDeltaPosition / Time.deltaTime;

        bool shouldMove = velocity.magnitude > 0.5f;

        rig.SetBool ("move", shouldMove);
        rig.SetFloat ("velx", velocity.x);
        rig.SetFloat ("vely", velocity.y);

        LastPosition = Hips.transform.position;
    }
}