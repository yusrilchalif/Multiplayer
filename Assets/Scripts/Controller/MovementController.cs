using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon;
using Photon.Realtime;
using Hastable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;
using System.IO;

public class MovementController : MonoBehaviourPunCallbacks
{
    PlayerInput _playerInput;
    CharacterController _characterController;
    Animator _animator;

    int isWalkingHash;
    int isRunningHash;
    int isJumpingHash;

    Vector2 currentMoveInput;
    Vector3 currentMove;
    Vector3 currentRunMove;
    bool isMovePressed;
    bool isRunPressed;
    
    float rotationMoveChar = 15.0f;
    float speedRun = 10.0f;
    int zero = 0;

    //jumping
    bool isJumPressed = false;
    float initialJumpVelocity;
    float maxJumpHeight = 1.0f;
    float maxJumpTime=0.5f;
    bool isJumping = false;

    //gravity variabel
    float gravity = -9.8f;
    float groundedGravity = -0.5f;

    Rigidbody rb;
    PhotonView pv;

    PlayerManager playerManager;

    void Awake()
    {
        _playerInput = new PlayerInput();
        _animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();
        _characterController = GetComponent<CharacterController>();

        isWalkingHash = Animator.StringToHash("isWalk");
        isRunningHash = Animator.StringToHash("isRun");

        //player input callback
        _playerInput.CharacterControl.Move.started += onMovementInput;
        _playerInput.CharacterControl.Move.canceled += onMovementInput;
        _playerInput.CharacterControl.Move.performed += onMovementInput;
        _playerInput.CharacterControl.Run.started += onRun;
        _playerInput.CharacterControl.Run.canceled += onRun;
        _playerInput.CharacterControl.Jump.started += onJump;
        _playerInput.CharacterControl.Jump.canceled += onJump;

        playerManager = PhotonView.Find((int)pv.InstantiationData[0]).GetComponent<PlayerManager>();

        setupJumpVariabel();
    }

    

    void onMovementInput(InputAction.CallbackContext context)
    {
        currentMoveInput = context.ReadValue<Vector2>();
        currentMove.x = currentMoveInput.x;
        currentMove.z = currentMoveInput.y;

        //run speed
        currentRunMove.x = currentMoveInput.x * speedRun;
        currentRunMove.z = currentMoveInput.y * speedRun;

        isMovePressed = currentMoveInput.x != 0 || currentMoveInput.y != 0;
    }

    void onRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    void setupJumpVariabel()
    {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
    }

    void onJump(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    void HandleAnimation()
    {
        bool isWalking = _animator.GetBool(isWalkingHash);
        bool isRunning = _animator.GetBool(isRunningHash);
        bool isJumping = _animator.GetBool(isJumpingHash);

        if (isMovePressed && !isWalking)
            _animator.SetBool(isWalkingHash, true);
        else if (!isMovePressed && isWalking)
            _animator.SetBool(isWalkingHash, false);

        if ((isMovePressed && isRunPressed) && !isRunning)
        {
            _animator.SetBool(isRunningHash, true);
        }
        else if ((!isMovePressed || !isRunPressed) && isRunning)
        {
            _animator.SetBool(isRunningHash, false);
        }

        //if (isMovePressed && !isJumping)
        //    _animator.SetBool(isJumpingHash, true);
        //else if (!isMovePressed && isJumping)
        //    _animator.SetBool(isJumpingHash, false);
    }

    void HandleRotation()
    {
        Vector3 positionLookAt;

        // change position
        positionLookAt.x = currentMove.x;
        positionLookAt.y = 0.0f;
        positionLookAt.z = currentMove.z;

        // change rotation
        Quaternion currentRotation = transform.rotation;

        //create new rotation
        if (isMovePressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationMoveChar * Time.deltaTime);
        }
    }

    void HandleJump()
    {
        if(!isJumping && _characterController.isGrounded && isJumPressed)
        {
            isJumping = true;
            currentMove.y = initialJumpVelocity;
            currentRunMove.y = initialJumpVelocity;
        }
        else if (!isJumPressed && isJumping && _characterController.isGrounded)
        {
            isJumping = false;
        }
    }

    void HandleGravity()
    {
        if(_characterController.isGrounded)
        {
            float groundedGravity = -0.5f;
            currentMove.y = groundedGravity;
            currentRunMove.y = groundedGravity;
        }else
        {
            float gravity = -9.8f;
            currentMove.y += gravity;
            currentRunMove.y += gravity;
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation();
        HandleAnimation();

        if (isRunPressed)
        {
            _characterController.Move(currentRunMove * Time.deltaTime);
        }
        else
        {
            _characterController.Move(currentMove * Time.deltaTime);
        }
        HandleGravity();
        HandleJump();
    }

    private void OnEnable()
    {
        _playerInput.CharacterControl.Enable();
    }

    private void OnDisable()
    {
        _playerInput.CharacterControl.Disable();
    }

}
