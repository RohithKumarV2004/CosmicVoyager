using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;


[RequireComponent (typeof(Rigidbody))]
public class ZeroGMovement : MonoBehaviour
{
    [Header("=== Player Movement Settings ===")]
    [SerializeField]
    private float rollTorque = 1000f;
    [SerializeField]
    private float thrust = 100f;
    [SerializeField]
    private float upThrust = 50f;
    [SerializeField]
    private float strafeThrust = 50f;
    [SerializeField] 
    private CinemachineVirtualCamera playerCamera;

    private Camera mainCam;

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

    public Spaceship ShipToEnter;

    public delegate void OnRequestShipEntry();
    public event OnRequestShipEntry onRequestShipEntry;




    // Start is called before the first frame update
    void Start()
    {
        mainCam=Camera.main;
        rb = GetComponent<Rigidbody>(); 
        rb.useGravity=false;
        currentBoostAmount=maxBoostAmount;
        ShipToEnter=null;
        CinemachineCameraSwitcher.SwitchCamera(playerCamera);
        if(playerCamera!=null){
            CinemachineCameraSwitcher.Register(playerCamera);
        }
        else{
            Debug.LogError("player camera not assigned");
        }

    }
    private void OnEnable(){
        if(playerCamera!=null){
            CinemachineCameraSwitcher.Register(playerCamera);
        }
        else{
            Debug.LogError("player camera not assigned");
        }
    }
    private void OnDisable(){
        if(playerCamera!=null){
            CinemachineCameraSwitcher.UnRegister(playerCamera);
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        HandleBoosting();
        HandleMovement();
    }
    void EnterShip(){
        transform.parent=ShipToEnter.transform;
        this.gameObject.SetActive(false);
        if(onRequestShipEntry!=null){
            onRequestShipEntry();
        }
    }
    void ExitShip(){
        transform.parent=null;
        this.gameObject.SetActive(true);
        CinemachineCameraSwitcher.SwitchCamera(playerCamera); //
    }

    public void AssignShip(Spaceship ship){
        ShipToEnter=ship;
        if(ShipToEnter!=null){
            ShipToEnter.onRequestShipExit+=ExitShip; //
        }
    }
    public void RemoveShip(){
        ShipToEnter.onRequestShipExit-=ExitShip; //
        ShipToEnter=null;
    }
    void HandleBoosting(){
        if( boosting && currentBoostAmount > 0f ){
            currentBoostAmount -= boostDeprecationRate;
            if( currentBoostAmount<=0f ){
                boosting=false;
            }
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
        rb.AddTorque(-mainCam.transform.forward * roll1D * rollTorque * Time.deltaTime);
        // // pitch
        // rb.AddRelativeTorque(Vector3.right * Mathf.Clamp(-pitchYaw.y, -1f, 1f) * pitchTorque * Time.deltaTime);
        // // yaw
        // rb.AddRelativeTorque(Vector3.up * Mathf.Clamp(pitchYaw.x, -1f, 1f) * yawTorque * Time.deltaTime);

        // thrust
        if (thrust1D > 0.1f || thrust1D < -0.1f){
            float currentThrust;

            if(boosting){
                currentThrust=thrust*boostMultiplier;
            }
            else{
                currentThrust=thrust;
            }

            rb.AddForce(mainCam.transform.forward * thrust1D * currentThrust * Time.deltaTime);
            glide=thrust;


        }
        else{
            rb.AddForce(mainCam.transform.forward * glide * Time.deltaTime);
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

            rb.AddForce(mainCam.transform.right * strafe1D * strafeThrust * Time.fixedDeltaTime);
            horizontalGlide=strafe1D*strafeThrust;

        }
        else{
            rb.AddForce(mainCam.transform.right * horizontalGlide * Time.fixedDeltaTime);
            horizontalGlide *= leftRightGlideReduction;
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
    public void OnInteract(InputAction.CallbackContext context){
        if(ShipToEnter!=null && context.action.triggered){
            EnterShip();
        }
    }
    // public void OnPitchYaw(InputAction.CallbackContext context){
    //     pitchYaw=context.ReadValue<Vector2>();
    // }
    public void onBoost(InputAction.CallbackContext context){
        boosting=context.performed;
    }
    #endregion
}
