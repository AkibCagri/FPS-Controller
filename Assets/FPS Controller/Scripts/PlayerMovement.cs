
// This code was written by Akýb Çaðrý Kürklü.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header ("Dragged Variables")]
    [SerializeField] CharacterController controller;
    [SerializeField] Transform myCamera;

    [Header ("Look Parameters")]
    [SerializeField] float mouseSensitivity = 750;
    [SerializeField] float upLookLimit = -90;
    [SerializeField] float downLookLimit = 90;
    float rotationX;

    [Header ("Movement Parameters")]
    [SerializeField] float walkSpeed = 4;
    [SerializeField] float sprintSpeed = 6;
    [SerializeField] float crouchSpeed = 1;
    bool isSprint;

    [Header ("Jumping Parameters")]
    [SerializeField] float gravity = 10;
    [SerializeField] float jumpForce = 5;
    Vector3 velocity;

    [Header("Landing Parameters")]
    bool isLand = true;

    [Header ("Crouching Parameters")]
    [SerializeField] float crouchTime = .25f;
    [SerializeField] float standingHeight = 1.8f;
    [SerializeField] float crouchHeight = .9f;
    [SerializeField] float standingCenter = .9f;
    [SerializeField] float crouchingCenter = .45f;
    [SerializeField] float standingCameraHeight = 1.5f;
    [SerializeField] float crouchingCameraHeight = .75f;
    bool isCrouching;

    [Header ("Head Bob Parameters")]
    [SerializeField] float baseBobSpeed = 4f;
    [SerializeField] float walkBobAmount = .05f;
    [SerializeField] float sprintBobAmount = .1f;
    [SerializeField] float crouchBobAmount = .05f;
    float headBobTimer;
    float positionOffset;
    bool canBob = true;


    [Header("Footstep Parameters")]
    [SerializeField] float baseStepSpeed = .5f;
    [SerializeField] float crouchStepMultipler = 1.5f;
    [SerializeField] float sprintStepMultipler = .6f;
    [SerializeField] AudioSource footstepAudioSource;
    bool canStep = true;

    [Header("Footstep Sounds")]
    [SerializeField] AudioClip[] woodStepClips;
    [SerializeField] AudioClip[] metalStepClips;
    [SerializeField] AudioClip[] grassStepClips;
    [SerializeField] AudioClip[] dirtStepClips;
    [SerializeField] AudioClip[] tileStepClips;
    [SerializeField] AudioClip[] mudStepClips;
    [SerializeField] AudioClip[] snowStepClips;
    [SerializeField] AudioClip[] waterStepClips;

    [Header("Jump Sounds")]
    [SerializeField] AudioClip[] woodJumpClips;
    [SerializeField] AudioClip[] metalJumpClips;
    [SerializeField] AudioClip[] grassJumpClips;
    [SerializeField] AudioClip[] dirtJumpClips;
    [SerializeField] AudioClip[] tileJumpClips;
    [SerializeField] AudioClip[] mudJumpClips;
    [SerializeField] AudioClip[] snowJumpClips;
    [SerializeField] AudioClip[] waterJumpClips;

    [Header("Land Sounds")]
    [SerializeField] AudioClip[] woodLandClips;
    [SerializeField] AudioClip[] metalLandClips;
    [SerializeField] AudioClip[] grassLandClips;
    [SerializeField] AudioClip[] dirtLandClips;
    [SerializeField] AudioClip[] tileLandClips;
    [SerializeField] AudioClip[] mudLandClips;
    [SerializeField] AudioClip[] snowLandClips;
    [SerializeField] AudioClip[] waterLandClips;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        LookAround();
        Sprint();
        Movement();
        Gravity();
        Jump();
        HandleCrouch();
        HeadBob();
        FinalMove();
    }

    void Footstep()
    {
            RaycastHit hit;
            if (Physics.Raycast(myCamera.transform.position, Vector3.down, out hit, standingHeight))
            {
                switch (hit.collider.tag)
                {
                    case "Ground/Grass":
                        footstepAudioSource.PlayOneShot(grassStepClips[Random.Range(0, grassStepClips.Length)]);
                        break;
                    case "Ground/Wood":
                        footstepAudioSource.PlayOneShot(woodStepClips[Random.Range(0, woodStepClips.Length)]);
                        break;
                    case "Ground/Metal":
                        footstepAudioSource.PlayOneShot(metalStepClips[Random.Range(0, metalStepClips.Length)]);
                        break;
                    case "Ground/Dirt":
                        footstepAudioSource.PlayOneShot(dirtStepClips[Random.Range(0, dirtStepClips.Length)]);
                        break;
                    case "Ground/Tile":
                        footstepAudioSource.PlayOneShot(tileStepClips[Random.Range(0, tileStepClips.Length)]);
                        break;
                    case "Ground/Mud":
                        footstepAudioSource.PlayOneShot(mudStepClips[Random.Range(0, mudStepClips.Length)]);
                        break;
                    case "Ground/Snow":
                        footstepAudioSource.PlayOneShot(snowStepClips[Random.Range(0, snowStepClips.Length)]);
                        break;
                    case "Ground/Water":
                        footstepAudioSource.PlayOneShot(waterStepClips[Random.Range(0, waterStepClips.Length)]);
                        break;
                    default:
                        footstepAudioSource.PlayOneShot(tileStepClips[Random.Range(0, tileStepClips.Length)]);
                        break;
            }
        }
    }

    void HeadBob()
    {
        if (controller.isGrounded == false)
            return;

        if (!canBob)
            return;

        float bobAmount = 0;
        float cameraHeight = standingCameraHeight;

        if (isCrouching)
        {
            bobAmount = crouchBobAmount;
            cameraHeight = crouchingCameraHeight;
        }
        else if (isSprint)
        {
            bobAmount = sprintBobAmount;
        }
        else
        {
            bobAmount = walkBobAmount;
        }

        if (positionOffset / Time.deltaTime > .5f)
        {
            headBobTimer += baseBobSpeed * positionOffset;
            if (Mathf.Sin(headBobTimer) < -.5f && canStep)
            {
                Footstep();
                canStep = false;
            }
            else if (Mathf.Sin(headBobTimer) > .5f)
            {
                canStep = true;
            }
                myCamera.transform.localPosition = new Vector3(myCamera.transform.localPosition.x, cameraHeight + Mathf.Sin(headBobTimer) * bobAmount, myCamera.transform.localPosition.z);
            while (headBobTimer > 2 * Mathf.PI)
            {
                headBobTimer -= 2 * Mathf.PI;
            }
        }
    }

    void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, upLookLimit, downLookLimit);

        myCamera.localRotation = Quaternion.Euler(rotationX, 0, 0);

        transform.Rotate(Vector3.up * mouseX);
    }

    void Movement()
    {
        float movementX = Input.GetAxis("Horizontal");
        float movementZ = Input.GetAxis("Vertical");

        float speed = 0;
        if (isCrouching)
            speed = crouchSpeed;
        else if (isSprint)
            speed = sprintSpeed;
        else
            speed = walkSpeed;

        Vector3 move = transform.right * movementX + transform.forward * movementZ;
        move *= speed;

        float velecityY = velocity.y;
        velocity = move;
        velocity.y = velecityY;
    }

    void Gravity()
    {
        if (controller.isGrounded == false)
        {
            velocity.y += gravity * Time.deltaTime;
            isLand = false;
        }
        else
        {
            if (isLand == false && velocity.y < -2)
            {
                RaycastHit hit;
                if (Physics.Raycast(myCamera.transform.position, Vector3.down, out hit, standingHeight))
                {
                    switch (hit.collider.tag)
                    {
                        case "Ground/Grass":
                            footstepAudioSource.PlayOneShot(grassLandClips[Random.Range(0, grassLandClips.Length)]);
                            break;
                        case "Ground/Wood":
                            footstepAudioSource.PlayOneShot(woodLandClips[Random.Range(0, woodLandClips.Length)]);
                            break;
                        case "Ground/Metal":
                            footstepAudioSource.PlayOneShot(metalLandClips[Random.Range(0, metalLandClips.Length)]);
                            break;
                        case "Ground/Dirt":
                            footstepAudioSource.PlayOneShot(dirtLandClips[Random.Range(0, dirtLandClips.Length)]);
                            break;
                        case "Ground/Tile":
                            footstepAudioSource.PlayOneShot(tileLandClips[Random.Range(0, tileLandClips.Length)]);
                            break;
                        case "Ground/Mud":
                            footstepAudioSource.PlayOneShot(mudLandClips[Random.Range(0, mudLandClips.Length)]);
                            break;
                        case "Ground/Snow":
                            footstepAudioSource.PlayOneShot(snowLandClips[Random.Range(0, snowLandClips.Length)]);
                            break;
                        case "Ground/Water":
                            footstepAudioSource.PlayOneShot(waterLandClips[Random.Range(0, waterLandClips.Length)]);
                            break;
                        default:
                            footstepAudioSource.PlayOneShot(tileLandClips[Random.Range(0, tileLandClips.Length)]);
                            break;
                    }
                }
            }

            velocity.y = -1f;
            isLand = true;
        }
    }

    void FinalMove()
    {
        Vector3 oldPosition = transform.position;
        controller.Move(velocity * Time.deltaTime);
        positionOffset = Vector3.Distance(transform.position, oldPosition);
    }

    void Jump()
    {
        if (controller.isGrounded == false)
            return;

        if (Input.GetButtonDown("Jump"))
        {
            if (!isCrouching)
            {
                velocity.y = jumpForce;

                RaycastHit hit;
                if (Physics.Raycast(myCamera.transform.position, Vector3.down, out hit, standingHeight))
                {
                    switch (hit.collider.tag)
                    {
                        case "Ground/Grass":
                            footstepAudioSource.PlayOneShot(grassJumpClips[Random.Range(0, grassJumpClips.Length)]);
                            break;
                        case "Ground/Wood":
                            footstepAudioSource.PlayOneShot(woodJumpClips[Random.Range(0, woodJumpClips.Length)]);
                            break;
                        case "Ground/Metal":
                            footstepAudioSource.PlayOneShot(metalJumpClips[Random.Range(0, metalJumpClips.Length)]);
                            break;
                        case "Ground/Dirt":
                            footstepAudioSource.PlayOneShot(dirtJumpClips[Random.Range(0, dirtJumpClips.Length)]);
                            break;
                        case "Ground/Tile":
                            footstepAudioSource.PlayOneShot(tileJumpClips[Random.Range(0, tileJumpClips.Length)]);
                            break;
                        case "Ground/Mud":
                            footstepAudioSource.PlayOneShot(mudJumpClips[Random.Range(0, mudJumpClips.Length)]);
                            break;
                        case "Ground/Snow":
                            footstepAudioSource.PlayOneShot(snowJumpClips[Random.Range(0, snowJumpClips.Length)]);
                            break;
                        case "Ground/Water":
                            footstepAudioSource.PlayOneShot(waterJumpClips[Random.Range(0, waterJumpClips.Length)]);
                            break;
                        default:
                            footstepAudioSource.PlayOneShot(tileJumpClips[Random.Range(0, tileJumpClips.Length)]);
                            break;
                    }
                }
            }

            if (isCrouching)
            {
                StartCoroutine(CrouchStand());
            }
        }
    }

    void Sprint()
    {
        if (Input.GetButton("Sprint"))
        {
            isSprint = true;
        }
        else
        {
            isSprint = false;
        }
    }

    void HandleCrouch()
    {
        if (Input.GetButtonDown("Crouch"))
        {
            StartCoroutine(CrouchStand());
        }
    }

    IEnumerator CrouchStand()
    {
        if (isCrouching)
        {
            Vector3 rayStartPos = transform.position + Vector3.up * controller.radius;
            float rayDistance = standingHeight - controller.radius * 2;
            RaycastHit hit;
            if (Physics.SphereCast(rayStartPos, controller.radius, Vector3.up, out hit, rayDistance))
                yield break;
        }
        float height = 0;
        float center = 0;
        float cameraHeight = 0;
        float timeElepsed = 0;
        if (!isCrouching)
        {
            height = crouchHeight;
            center = crouchingCenter;
            cameraHeight = crouchingCameraHeight;
        }
        else
        {
            height = standingHeight;
            center = standingCenter;
            cameraHeight = standingCameraHeight;
        }

        float currentHeight = controller.height;
        Vector3 currentCenter = controller.center;
        Vector3 currentCameraHeight = myCamera.transform.localPosition;

        canBob = false;

        while (timeElepsed < crouchTime)
        {
            controller.height = Mathf.Lerp(currentHeight, height, timeElepsed / crouchTime);
            controller.center = Vector3.Lerp(currentCenter, new Vector3(currentCenter.x, center, currentCenter.z), timeElepsed / crouchTime);
            myCamera.localPosition = Vector3.Lerp(currentCameraHeight, new Vector3(myCamera.localPosition.x, cameraHeight, myCamera.localPosition.z), timeElepsed / crouchTime);
            timeElepsed += Time.deltaTime;
            yield return null;
        }

        canBob = true;

        controller.height = height;
        controller.center = new Vector3(controller.center.x, center, controller.center.z);

        isCrouching = !isCrouching;
    }
}