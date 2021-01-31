using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimEventHandler : KeyedStateController
{
    private Animator animator;
    private CharacterModel owner;

    public bool checkingComboContinue = false;

    public bool isInDivingState = false;
    public bool isInFallingState = false;
    public bool isInGettingUpState = false;

    public int qteIndex = -1;
    public bool qteSequenceFailed = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        owner = GetComponentInParent<CharacterModel>();
    }

    void Start()
    {
        this.AddStateEnterListener("dive", (s, animator1, arg3, arg4) =>
        {
            isInDivingState = true;
            owner.qtePassed = false;
            qteSequenceFailed = false;
            qteIndex = -1;
        });
        this.AddStateExitListener("dive", (s, animator1, arg3, arg4) =>
        {
            isInDivingState = false;
            StopDashing(null);
            owner.qtePassed = false;
        });

        this.AddStateEnterListener("fall", (s, animator1, arg3, arg4) =>
        {
            isInFallingState = true;
            qteSequenceFailed = true;
        });
        this.AddStateExitListener("fall", (s, animator1, arg3, arg4) =>
        {
            isInFallingState = false;
        });


        this.AddStateEnterListener("gettingUp", (s, animator1, arg3, arg4) =>
        {
            isInGettingUpState = true;
        });
        this.AddStateExitListener("gettingUp", (s, animator1, arg3, arg4) =>
        {
            isInGettingUpState = false;
        });
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void StartDashing(AnimationEvent animEvent)
    {
        var dashDir = ThirdPersonCamera.Instance.virtualCamera.transform.TransformVector(owner.characterInput.Move);
        if (owner.characterInput.Move.magnitude <= 0.1f)
        {
            dashDir = owner.avatar.forward;
        }

        var dashTarget = owner.transform.position + dashDir;
        dashTarget.y = transform.position.y;
        owner.characterMovementController.DashTowards(dashTarget, animEvent.floatParameter, -1);
    }


    public void StopDashing(AnimationEvent animEvent)
    {
        owner.characterMovementController.StopDashing();
    }

    public void Qte(AnimationEvent animEvent)
    {
        qteIndex++;
        owner.Qte(animEvent);
    }

    public void GrabCat(AnimationEvent animEvent)
    {
        owner.AttemptGrabCat();
    }
}