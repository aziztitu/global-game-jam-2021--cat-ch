using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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

            this.WaitAndExecute(() => { owner.PlaySFX($"Grunt{Random.Range(1, 4)}", 0.4f); }, 0.3f, true);
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

        var dashSpeed = animEvent.floatParameter;
        var dashTarget = owner.transform.position + dashDir;
        dashTarget.y = transform.position.y;

        var cat = owner.FindCatInDirection(dashDir, 2);
        if (cat)
        {
            dashTarget = cat.transform.position;

            var toCat = cat.transform.position - transform.position;
            toCat.y = 0;

            dashSpeed = HelperUtilities.Remap(toCat.magnitude, 0, owner.qteCatMaxRange, 0, dashSpeed);

            // Pause Cat Movement
            cat.Stop();

            if (qteIndex <= 0)
            {
                owner.SetQteCat(cat);
            }
        }

        owner.characterMovementController.DashTowards(dashTarget, dashSpeed, -1);
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