using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    public float moveSmoothing = 2f;

    private CharacterModel _characterModel;

    private Vector3 targetMove = Vector3.zero;

    private void Awake()
    {
        _characterModel = GetComponent<CharacterModel>();
    }

    public void Update()
    {
        UpdatePlayerInput();
    }

    public void UpdatePlayerInput()
    {
        if (Time.timeScale < 0.1f)
        {
            _characterModel.characterInput = new CharacterModel.CharacterInput();
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        //_characterModel.characterInput.Move = new Vector3(horizontal, 0f, vertical);
        //_characterModel.characterInput.AttemptParry = Input.GetButtonDown("Block");
        //_characterModel.characterInput.IsBlocking = Input.GetButton("Block");
        //_characterModel.characterInput.Dodge = Input.GetButtonDown("Dodge");
        //_characterModel.characterInput.Sprint = Input.GetButton("Sprint");

        //_characterModel.characterInput.HeavyAttack = (Input.GetKey(KeyCode.LeftShift) && Input.GetButtonDown("Fire1")) || Input.GetButtonDown("Fire2");
        // _characterModel.characterInput.LightAttack =
        //     !_characterModel.characterInput.HeavyAttack && Input.GetButtonDown("Fire1");


        _characterModel.characterInput.Move = Vector3.Lerp(_characterModel.characterInput.Move,
            targetMove, Time.deltaTime * moveSmoothing);

        if (_characterModel.characterInput.Move.magnitude < 0.1)
        {
            _characterModel.characterInput.Move = Vector3.zero;
        }
        else if (_characterModel.characterInput.Move.magnitude > 0.9)
        {
            _characterModel.characterInput.Move = targetMove;
        }
    }

    public void OnMove(InputValue value)
    {
        var moveVector = value.Get<Vector2>();
        targetMove = new Vector3(moveVector.x, 0f, moveVector.y).normalized;
    }
}