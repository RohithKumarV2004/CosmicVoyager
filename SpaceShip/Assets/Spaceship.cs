using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

[RequireComponent (typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class Spaceship : MonoBehaviour
{
    [Header("=== Ship Movement Settings ===")]
    [SerializeField]
    private float yawTorque =500f;
    [SerializeField]
    private float pitchTorque = 1000f;
    [SerializeField]
    private float rollTorque = 1000f;
    [SerializeField]
    private float thrust = 100f;
    [SerializeField]
    private float upThrust = 50f;
    [SerializeField]
    private float strafeThrust = 50f;

    [Header("=== Boost Settings ===")]
    [SerializeField]
    private float maxBoostAmount=2f;
    [SerializeField]
    private float boostDeprecationRate=0.25f;
    [SerializeField]
    private float boostRechargeRate=0.5f;
    [SerializeField]
    private float boostMultiplier=5f;
    public bool boosting = false;
    public float currentBoostAmount;

    [SerializeField]
    private CinemachineVirtualCamera shipThirdPersonCamera;
    [SerializeField]
    private CinemachineVirtualCamera shipFirstPersonCamera;

    private AudioSource audioSource;

    [Header("=== Audio Settings ===")]
    public AudioClip thrustSound;
    public AudioClip boostSound;
    public AudioClip explosionSound;
    public AudioClip backGroundSound;



    [SerializeField, Range(0.001f, 0.999f)]
    private float thrustGlideReduction = 0.999f;
    [SerializeField, Range(0.001f, 0.999f)]
    private float upDownGlideReduction = 0.111f;
    [SerializeField, Range(0.001f, 0.999f)]
    private float leftRightGlideReduction = 0.111f;
    float glide, verticalGlide, horizontalGlide = 0f;

    Rigidbody rb;

    private float thrust1D;
    private float upDown1D;
    private float strafe1D;
    private float roll1D;
    private Vector2 pitchYaw;

    private bool isOccupied=false;

    private ZeroGMovement player;

    public delegate void OnRequestShipExit();
    public event OnRequestShipExit onRequestShipExit;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>(); 
        currentBoostAmount=maxBoostAmount;

        // player = GameObject.FindGameObjectWithTag("Player").GetComponent<ZeroGMovement>();
        // if(player != null){
        //     print("player found");
        // }

        ZeroGMovement[] players = FindObjectsOfType<ZeroGMovement>();

        foreach (ZeroGMovement playerComponent in players)
        {
            // Check if the object has the "Player" tag
            if (playerComponent.gameObject.CompareTag("Player"))
            {
                // This is the player object with the "Player" tag and the ZeroGMovement script
                player = playerComponent;
                print("player found");
                break; // Stop iterating once the player is found (if there are multiple)
            }
        }

        player.onRequestShipEntry += PlayerEnteredShip;
        
        // GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        // if (playerObject != null)
        // {
        //     Debug.Log("Player object found: " + playerObject.name);

        //     // Check if ZeroGMovement component is present
        //     player = playerObject.GetComponent<ZeroGMovement>();
        //     if (player != null)
        //     {
        //         Debug.Log("ZeroGMovement component found on player.");
        //     }
        //     else
        //     {
        //         Debug.LogError("ZeroGMovement component not found on player.");
        //     }
        // }
        // else
        // {
        //     Debug.LogError("Player object not found!");
        // }

        // Initialize the AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        PlayAudio(backGroundSound);

    }
    private void OnEnable(){
        if(shipThirdPersonCamera!=null){
            CinemachineCameraSwitcher.Register(shipThirdPersonCamera);
        }
        else{
            Debug.LogError("player camera not assigned");
        }
    }
    private void OnDisable(){
        if(shipThirdPersonCamera!=null){
            CinemachineCameraSwitcher.UnRegister(shipThirdPersonCamera);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(isOccupied){
            HandleBoosting();
            HandleMovement();
        }
    }
    void HandleBoosting(){
        if( boosting && currentBoostAmount > 0f ){
            currentBoostAmount -= boostDeprecationRate;
            if( currentBoostAmount<=0f ){
                boosting=false;
            }

            PlayAudio(boostSound);
        }
        else{
            if( currentBoostAmount<maxBoostAmount ){
                currentBoostAmount+=boostRechargeRate;
            }
        }
    }
    void HandleMovement()
    {
        // roll
        rb.AddRelativeTorque(Vector3.back * roll1D * rollTorque * Time.deltaTime);
        // pitch
        rb.AddRelativeTorque(Vector3.right * Mathf.Clamp(-pitchYaw.y, -1f, 1f) * pitchTorque * Time.deltaTime);
        // yaw
        rb.AddRelativeTorque(Vector3.up * Mathf.Clamp(pitchYaw.x, -1f, 1f) * yawTorque * Time.deltaTime);

        // thrust
        if (thrust1D > 0.1f || thrust1D < -0.1f){
            float currentThrust;

            if(boosting){
                currentThrust=thrust*boostMultiplier;
            }
            else{
                currentThrust=thrust;
            }

            rb.AddRelativeForce(Vector3.forward * thrust1D * currentThrust * Time.deltaTime);
            glide=thrust;

            PlayAudio(thrustSound);

        }
        else{
            rb.AddRelativeForce(Vector3.forward * glide * Time.deltaTime);
            glide *= thrustGlideReduction;
        }
        //updown
        if (upDown1D > 0.1f || upDown1D < -0.1f){

            rb.AddRelativeForce(Vector3.up * upDown1D * upThrust * Time.fixedDeltaTime);
            verticalGlide=upDown1D*upThrust;

        }
        else{
            rb.AddRelativeForce(Vector3.up * verticalGlide * Time.fixedDeltaTime);
            verticalGlide*=upDownGlideReduction;
        }
        //strafing
        if (strafe1D > 0.1f || strafe1D < -0.1f){

            rb.AddRelativeForce(Vector3.right * strafe1D * strafeThrust * Time.fixedDeltaTime);
            horizontalGlide=strafe1D*strafeThrust;

        }
        else{
            rb.AddRelativeForce(Vector3.right * horizontalGlide * Time.fixedDeltaTime);
            horizontalGlide *= leftRightGlideReduction;
        }
    }
    private void PlayAudio(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void PlayerEnteredShip(){
        rb.isKinematic=false;
        CinemachineCameraSwitcher.SwitchCamera(shipThirdPersonCamera);
        isOccupied=true;
    }

    void PlayerExitedShip(){
        rb.isKinematic=true;
        isOccupied=false;
        if(onRequestShipExit!=null){
            onRequestShipExit();
        }
    }

    #region Input Methods

    public void OnThrust(InputAction.CallbackContext context){
        thrust1D=context.ReadValue<float>();
    }
    public void OnStrafe(InputAction.CallbackContext context){
        strafe1D=context.ReadValue<float>();
    }
    public void OnUpDown(InputAction.CallbackContext context){
        upDown1D=context.ReadValue<float>();
    }
    public void OnRoll(InputAction.CallbackContext context){
        roll1D=context.ReadValue<float>();
    }
    public void OnPitchYaw(InputAction.CallbackContext context){
        pitchYaw=context.ReadValue<Vector2>();
    }
    public void onBoost(InputAction.CallbackContext context){
        boosting=context.performed;
    }
    public void OnInteract(InputAction.CallbackContext context){
        if(isOccupied && context.action.triggered){
            PlayerExitedShip();
        }
    }
    public void OnSwitchCamera(InputAction.CallbackContext context){
        if(isOccupied && context.action.triggered){
            if(CinemachineCameraSwitcher.IsActiveCamera(shipFirstPersonCamera)){
                CinemachineCameraSwitcher.SwitchCamera(shipThirdPersonCamera);
            }
            else if(CinemachineCameraSwitcher.IsActiveCamera(shipThirdPersonCamera)){
                CinemachineCameraSwitcher.SwitchCamera(shipFirstPersonCamera);
            }
        }
    }
    #endregion
}
