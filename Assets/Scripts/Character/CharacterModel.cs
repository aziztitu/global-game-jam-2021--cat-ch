using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public Transform hand;
    public AudioSource footstepAudio;
    public float footstepMaxVolume = 1f;
    public AudioSource characterAudio;

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
    public Image qteErrorTint;
    public List<InputKeyUI> qteIcons;
    public RangeFloat qteDurationRange = new RangeFloat(1.0f, 1.7f);
    public float qteBaseTimeScale = 0.15f;
    public float qteCatMaxRange = 10f;
    public float qteCatMaxAngle = 30f;

    public RangeFloat qteXRange;
    public RangeFloat qteYRange;

    private int selectedQte = 0;
    private CatController selectedQteCat = null;
    private bool qtePending = false;
    private Coroutine qteCoroutine = null;

    [Header("Death")] public float deathAnimationDuration = 3f;
    public float playerHitCamShakeDuration = 1f;
    public float playerHitCamShakeMinInterval = 1f;
    public float playerHitCamShakeAmplitude = 3f;
    public int playerHitCamShakeFrequency = 10;

    private bool isShakingCam = false;

    public event Action onInitialized;

    [ReadOnly] public bool qtePassed = false;

    public int catsFound = 0;

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
        if (Time.timeScale < 0.1f)
        {
            return;
        }

        if (characterAnimEventHandler.isInDivingState && !characterAnimEventHandler.qteSequenceFailed && qtePending)
        {
            qtePassed = selectedQte == buttonPressed;
            EndQte();
        }
    }

    public void SetQteCat(CatController catController)
    {
        selectedQteCat = catController;
    }

    public void Qte(AnimationEvent animEvent)
    {
        if (qteIcons.Count < 1)
        {
            return;
        }

        if (!characterAnimEventHandler.qteSequenceFailed)
        {
            if (characterAnimEventHandler.qteIndex == 0 && selectedQteCat == null)
            {
                var cat = FindCatInFront();
                if (cat)
                {
                    selectedQteCat = cat;

                    // Pause Cat Movement
                    selectedQteCat.Stop();
                }
                else
                {
                    characterAnimEventHandler.qteSequenceFailed = true;
                    return;
                }
            }

            selectedQte = Random.Range(1, qteIcons.Count + 1);
            ShowSelectedQteIcon();

            var qteX = qteXRange.GetRandom() * (Random.Range(0, 10) >= 5 ? 1 : -1);
            var qteY = qteXRange.GetRandom() * (Random.Range(0, 10) >= 5 ? 1 : -1);
            var qtePos = new Vector2(qteX, qteY);

            qteButton.localPosition = qtePos;

            qteButton.gameObject.SetActive(true);
            qteErrorTint.DOFade(0, 0).Play();

            qteFiller.DOKill();
            qteFiller.fillAmount = 1;

            qteDurationRange.SelectRandom();

            var fillerTween = qteFiller.DOFillAmount(0, qteDurationRange.selected).SetUpdate(true).Play();

            Time.timeScale = qteBaseTimeScale;
            qtePassed = false;
            qtePending = true;

            if (qteCoroutine != null)
            {
                StopCoroutine(qteCoroutine);
            }

            qteCoroutine = this.WaitAndExecute(() => { EndQte(); }, qteDurationRange.selected, true);
        }
    }

    public void EndQte()
    {
        Time.timeScale = 1;

        if (!characterAnimEventHandler.qteSequenceFailed && !qtePassed)
        {
            characterAnimEventHandler.qteSequenceFailed = true;
            if (characterAnimEventHandler.qteIndex == 0)
            {
                animator.SetTrigger("Fall");

                PlaySFX("Sighing", 0.3f);
            }

            if (characterAnimEventHandler.qteIndex == 1)
            {
                animator.SetTrigger("FallHalf");

                PlaySFX("Sighing", 0.1f);
            }

            if (selectedQteCat?.catState == CatController.CatState.Running)
            {
                // Resume Cat Movement
                selectedQteCat?.Resume();
            }
            else
            {
                selectedQteCat?.ChangeState(CatController.CatState.Running);
            }

            var nearbyCats = FindCatsInRadius(8).Where(cat => cat != selectedQteCat);
            foreach (var cat in nearbyCats)
            {
                if (cat?.catState == CatController.CatState.Running)
                {
                    // Resume Cat Movement
                    cat?.Resume();
                }
                else
                {
                    cat?.ChangeState(CatController.CatState.Running);
                }
            }

            selectedQteCat = null;


            qteErrorTint.DOFade(0.9f, 0.3f).Play();
            this.WaitAndExecute(() => { qteButton.gameObject.SetActive(false); }, 2f);
        }
        else
        {
            qteButton.gameObject.SetActive(false);
        }

        if (qteCoroutine != null)
        {
            StopCoroutine(qteCoroutine);
        }

        qtePending = false;
    }

    public void ShowSelectedQteIcon()
    {
        for (int i = 0; i < qteIcons.Count; i++)
        {
            qteIcons[i].gameObject.SetActive(selectedQte == i + 1);
        }
    }

    public CatController FindCatInFront()
    {
        return FindCatInDirection(avatar.forward);
    }

    public CatController FindCatInDirection(Vector3 forward, float tooCloseThreshold = 0)
    {
        var colliders = Physics.OverlapSphere(transform.position, qteCatMaxRange);
        foreach (var col in colliders)
        {
            var cat = col.GetComponentInParent<CatController>();
            if (cat)
            {
                var toCat = cat.transform.position - transform.position;
                toCat.y = 0;

                if (toCat.magnitude <= tooCloseThreshold)
                {
                    return cat;
                }

                var yLessFwd = forward;
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

    public List<CatController> FindCatsInRadius(float radius)
    {
        var colliders = Physics.OverlapSphere(transform.position, qteCatMaxRange);
        return colliders.Select(col => col.GetComponentInParent<CatController>()).Where(cat => cat != null).ToList();
    }

    public void AttemptGrabCat()
    {
        if (characterAnimEventHandler.qteSequenceFailed || !selectedQteCat)
        {
            return;
        }


        selectedQteCat.transform.SetParent(hand);
        selectedQteCat.transform.DOLocalMove(Vector3.zero, 0.3f).Play();

        selectedQteCat.UnwrapAI();

        // Destroy(selectedQteCat.gameObject, 1f);


        var nearbyCats = FindCatsInRadius(8).Where(cat => cat != selectedQteCat);
        foreach (var cat in nearbyCats)
        {
            if (cat?.catState == CatController.CatState.Running)
            {
                // Resume Cat Movement
                cat?.Resume();
            }
            else
            {
                cat?.ChangeState(CatController.CatState.Running);
            }
        }

        this.WaitAndExecute(() =>
        {
            selectedQteCat.PutInCage();
            selectedQteCat = null;
        }, 1f);

        catsFound++;
    }

    public void PlaySFX(string key, float volume = 1)
    {
        PlaySFX(SoundEffectsManager.Instance.GetClip(key), volume);
    }

    public void PlaySFX(AudioClip audioClip, float volume = 1)
    {
        if (audioClip == null)
        {
            return;
        }

        characterAudio.PlayOneShot(audioClip, volume);
    }
}