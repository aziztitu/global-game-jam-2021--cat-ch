using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CharacterModel : SingletonMonoBehaviour<CharacterModel>
{
    [Serializable]
    public class CharacterInput
    {
        public Vector3 Move;
        public bool LightAttack;
        public bool HeavyAttack;
        public bool IsBlocking;
        public bool AttemptParry;

        public bool Dodge;

        public bool Taunt;
        // public bool Sprint;
    }

    [ReadOnly] public CharacterInput characterInput = new CharacterInput();

    public CharacterAnimEventHandler characterAnimEventHandler { get; private set; }
    public CharacterMovementController characterMovementController { get; private set; }
    public PlayerInputController playerInputController { get; private set; }
    public Animator animator { get; private set; }
    public Health health { get; private set; }
    public bool isAlive => health.currentHealth > 0;
    public bool isDead => !isAlive;

    [ReadOnly] public int characterIndex = 0;

    [HideInInspector] public Transform lockedOnTarget;
    public Vector3 lockedOnTargetPos => lockedOnTarget?.position ?? Vector3.zero;

    // public float delayBeforeHealthRegeneration = 3f;
    // public float healthRegenerationSpeed = 1f;
    public Transform playerTarget;
    public float playerTargetRotationSpeed = 10f;

    public Transform avatar;

    public Transform avatarModel
    {
        get
        {
            for (int i = 0; i < avatar.childCount; i++)
            {
                var child = avatar.GetChild(i);
                if (child.gameObject.activeInHierarchy)
                {
                    return child;
                }
            }

            return null;
        }
    }

    [Header("QTE")] public GameObject qteUi;
    public RectTransform qteButton;
    public Image qteFiller;
    public List<InputKeyUI> qteIcons;
    public float qteBaseDuration = 1.5f;
    public float qteBaseTimeScale = 0.2f;
    public float qteCatMaxRange = 10f;
    public float qteCatMaxAngle = 30f;

    private int selectedQte = 0;
    private CatController selectedQteCat = null;


    [Header("Death")] public float deathAnimationDuration = 3f;
    public float playerHitCamShakeDuration = 1f;
    public float playerHitCamShakeMinInterval = 1f;
    public float playerHitCamShakeAmplitude = 3f;
    public int playerHitCamShakeFrequency = 10;

    private bool isShakingCam = false;

    public event Action onInitialized;

    [ReadOnly] public bool qtePassed = false;

    new void Awake()
    {
        base.Awake();

        characterMovementController = GetComponent<CharacterMovementController>();
        playerInputController = GetComponent<PlayerInputController>();
        animator = GetComponentInChildren<Animator>(false);
        characterAnimEventHandler = animator.GetComponent<CharacterAnimEventHandler>();
        health = GetComponent<Health>();
        health.OnDamageTaken.AddListener(() =>
        {
            if (health.currentHealth > 0)
            {
                animator.SetTrigger("Hit");
                SoundEffectsManager.Instance.Play($"HurtGrunt{Random.Range(1, 5)}");

                if (!isShakingCam)
                {
                    CinemachineCameraManager.Instance.CurrentStatefulCinemachineCamera.CamNoise(
                        playerHitCamShakeAmplitude, playerHitCamShakeFrequency);
                    isShakingCam = true;
                    this.WaitAndExecute(() =>
                    {
                        CinemachineCameraManager.Instance.CurrentStatefulCinemachineCamera.CamNoise(0, 0);
                        this.WaitAndExecute(() => { isShakingCam = false; }, playerHitCamShakeMinInterval);
                    }, playerHitCamShakeDuration);
                }
            }
        });
        health.OnHealthDepleted.AddListener(() =>
        {
            animator.applyRootMotion = true;
            var deathMode = (Random.Range(0, 10) >= 5) ? 1 : 2;
            animator.SetTrigger($"Death{deathMode}");

            var deathSoundKey = deathMode == 1 ? "BackwardsDeath" : "ForwardDeath";
            SoundEffectsManager.Instance.Play(deathSoundKey);
        });

        qteButton.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    void OnDrawGizmos()
    {
        var catMinRangeTarget = transform.position + avatar.forward * qteCatMaxRange;
        DebugExtension.DebugCone(transform.position, catMinRangeTarget - transform.position, Color.red, qteCatMaxAngle);
    }

    // Update is called once per frame
    void Update()
    {
        /*if (isAlive && health.currentHealth < health.maxHealth &&
            health.timeSinceLastDamage > delayBeforeHealthRegeneration)
        {
            health.UpdateHealth(healthRegenerationSpeed * Time.deltaTime);
        }
        */

        // playerTarget.localRotation = Quaternion.Slerp(playerTarget.localRotation, Quaternion.LookRotation(toTarget, Vector3.up), Time.deltaTime);

        /*var targetLookAt = lockedOnTargetPos;
        targetLookAt.y = 0;

        var originalRot = playerTarget.localRotation;
        playerTarget.LookAt(targetLookAt);

        var newAngles = playerTarget.localRotation.eulerAngles;
        newAngles.x = originalRot.eulerAngles.x;

        var targetRotation = Quaternion.Euler(newAngles);

        playerTarget.localRotation = Quaternion.Slerp(originalRot, targetRotation, playerTargetRotationSpeed * Time.deltaTime);
        */

        animator.SetBool("IsAlive", isAlive);
    }

    public void OnInitialized()
    {
        this.onInitialized?.Invoke();
    }

    public void PlayVictoryTaunt()
    {
    }

    public void OnDive()
    {
        if (characterMovementController.canMove)
        {
            animator.SetTrigger("Dive");
        }
    }

    public void OnQte1()
    {
        OnQtePressed(1);
    }

    public void OnQte2()
    {
        OnQtePressed(2);
    }

    public void OnQte3()
    {
        OnQtePressed(3);
    }

    public void OnQte4()
    {
        OnQtePressed(4);
    }

    public void OnQtePressed(int buttonPressed)
    {
        qtePassed = selectedQte == buttonPressed;
        EndQte();
    }

    public void Qte(AnimationEvent animEvent)
    {
        if (qteIcons.Count < 1)
        {
            return;
        }

        if (!characterAnimEventHandler.qteSequenceFailed)
        {
            if (characterAnimEventHandler.qteIndex == 0)
            {
                var cat = FindCatInFront();
                if (cat)
                {
                    selectedQteCat = cat;
                }
                else
                {
                    characterAnimEventHandler.qteSequenceFailed = true;
                    return;
                }
            }

            selectedQte = Random.Range(1, qteIcons.Count + 1);
            ShowSelectedQteIcon();
            qteButton.gameObject.SetActive(true);

            Time.timeScale = qteBaseTimeScale;
            qtePassed = false;

            this.WaitAndExecute(() =>
            {
                EndQte();

                if (!qtePassed)
                {
                    characterAnimEventHandler.qteSequenceFailed = true;
                    if (characterAnimEventHandler.qteIndex == 0)
                    {
                        animator.SetTrigger("Fall");
                    }
                }
            }, qteBaseDuration, true);
        }
    }

    public void EndQte()
    {
        qteButton.gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    public void ShowSelectedQteIcon()
    {
        for (int i = 0; i < qteIcons.Count; i++)
        {
            qteIcons[i].gameObject.SetActive(selectedQte == i);
        }
    }

    public CatController FindCatInFront()
    {
        var colliders = Physics.OverlapSphere(transform.position, qteCatMaxRange);
        foreach (var col in colliders)
        {
            var cat = col.GetComponentInParent<CatController>();
            if (cat)
            {
                var toCat = cat.transform.position - transform.position;
                toCat.y = 0;

                var yLessFwd = avatar.forward;
                yLessFwd.y = 0;

                var angle = Vector3.Angle(toCat, yLessFwd);
                if (angle < qteCatMaxAngle)
                {
                    return cat;
                }
            }
        }

        return null;
    }

    public void AttemptGrabCat()
    {
        if (characterAnimEventHandler.qteSequenceFailed || !selectedQteCat)
        {
            return;
        }
    }
}