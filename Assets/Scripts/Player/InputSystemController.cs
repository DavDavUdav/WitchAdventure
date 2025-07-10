using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _jumpHeight;
    [SerializeField] private Transform _groundCheckPointLeft;
    [SerializeField] private Transform _groundCheckPointRight;
    [SerializeField] private float _rayLenght;

    [SerializeField] private Animator _animator;

    private Rigidbody2D _rigidbody;
    private InputSystem_Actions _inputSystem;
    private InputAction _move;
    private InputAction _jump;

    [SerializeField] private bool _isJumping;
    [SerializeField] private bool _isFalling;
    [SerializeField] private bool _isGrounded;
    [SerializeField] private bool _wasGrounded;
    [SerializeField] private bool _takeJumpButton;
    [SerializeField] private bool _isMove;
    [SerializeField] private bool _wasIsJumping;
    [SerializeField] private bool _playedEndJump;

    [SerializeField] private float _currentVerticalSpeed;
    private void Awake()
    {
        _inputSystem = new InputSystem_Actions();
    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

        _isJumping = false;
        _isFalling = false;
        _takeJumpButton = false;
        _playedEndJump = false;
    }

    private void FixedUpdate()
    {
        _currentVerticalSpeed = _rigidbody.linearVelocityY;
        Move();
        var filter = new ContactFilter2D();
        filter.SetLayerMask(8);
        RaycastHit2D hitLeft = Physics2D.Raycast(_groundCheckPointLeft.position, Vector2.down,_rayLenght);
        RaycastHit2D hitRight = Physics2D.Raycast(_groundCheckPointRight.position, Vector2.down,_rayLenght);

        var playerVerticalVelocity = Mathf.Sign(_rigidbody.linearVelocity.y);

        var hasGroundColliderLeft = hitLeft.collider != null;
        var hasGroundColliderRight = hitRight.collider != null;
        _wasGrounded = _isGrounded;
        _isGrounded = hasGroundColliderLeft || hasGroundColliderRight;
        _wasIsJumping = _isJumping;



        if (_isGrounded && !_wasGrounded)
        {
            if((_isJumping || _isFalling) && !_playedEndJump)
            {
                _animator.SetTrigger("IsEndJump");
                _playedEndJump = true;
            }

            _isJumping = false;
            _isFalling = false;
            _takeJumpButton = false;
        }
        else
        {
            if(_takeJumpButton && playerVerticalVelocity == 1f)
            {
                _isGrounded=false;
                _isJumping=true;
                _isFalling=false;
                
            }
            else if(playerVerticalVelocity == -1f && !hasGroundColliderLeft && !hasGroundColliderRight && _currentVerticalSpeed > 2f)
            {
                _isGrounded = false;
                _isJumping=true;
                _isFalling=true;

            }
        }

        // amination
        _animator.SetBool("IsMove", _isMove ? true : false);

        if (_wasGrounded && !_isGrounded && _currentVerticalSpeed > 2f)
        {
            _animator.SetTrigger("IsStartJump");
        }
    }

    private void OnEnable()
    {
        _inputSystem.Player.Enable();
        _move = _inputSystem.Player.Move;
        _jump = _inputSystem.Player.Jump;
        _jump.performed += OnPressedjump;
    }

    private void OnDisable()
    {
        _jump.performed -= OnPressedjump;
        _inputSystem.Player.Disable();
    }

    private void Move()
    {
        var moveDirection = _move.ReadValue<Vector2>();
        _rigidbody.linearVelocityX = moveDirection.x * _moveSpeed;


        if (moveDirection.x != 0)
        {
            _isMove = true;
            transform.localScale = new Vector3(Mathf.Sign(moveDirection.x), 1, 1);
        }
        else
        {
            _isMove = false;
        }
    }

    private void OnPressedjump(InputAction.CallbackContext callbackContext)
    {
        if (_isGrounded && !_isFalling)
        {
            _takeJumpButton = true;
            Jump();
        }
    }

    private void Jump()
    {
        _playedEndJump = false;
        _rigidbody.linearVelocityY += _jumpHeight;
    }
}
