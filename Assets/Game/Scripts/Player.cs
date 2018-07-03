using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.NetworkInformation;

public class Player : MonoBehaviour
{
    #region Events 

    public delegate void DidMove(Player caller, Vector3 position);
    public DidMove OnDidMove= delegate { };

    #endregion 

    public Transform cameraTransform;

    public Transform floorCursor;

    public GvrReticlePointer gvrReticlePointer;

    public Vector3 forward
    {
        get
        {
            return cameraTransform.forward;
        }
    }

    public Vector3 position
    {
        get
        {
            return cameraTransform.position;
        }
    }

    private TeleportScreenTransitionImageEffect _teleportEffect;

    public TeleportScreenTransitionImageEffect TeleportEffect{
        get{
            if(_teleportEffect == null){
                _teleportEffect = transform.GetComponentInChildren<TeleportScreenTransitionImageEffect>();
            }
            return _teleportEffect; 
        }
    }

    public float teleportFadeSpeed = 0.5f; 

    private RaycastHit? cursorHit;

    public bool CanTeleport{
        get{
            return SceneManager.SharedInstance.CurrentState == SceneManager.State.Exploring;
        }
    }

    // Use this for initialization
    void Start()
    {
        cameraTransform = transform.GetComponentInChildren<Camera>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTeleportationCursor();

        if (Input.GetButtonDown("Fire1"))
        {
            Teleport(); 
        }
    }

    void Teleport(){
        if(!cursorHit.HasValue){
            return; 
        }

        StartCoroutine(Teleporting());
    }

    IEnumerator Teleporting(){
        while(TeleportEffect.maskValue < 0.98f){
            // Update mask value 
            TeleportEffect.maskValue += teleportFadeSpeed * Time.deltaTime;

            // update fade alpha 
            const float minAlpha = 0.3f;
            const float maxAlpha = 1.0f;
            float alpha = minAlpha + (maxAlpha - minAlpha) * (TeleportEffect.maskValue / 1.0f);
            TeleportEffect.maskColorAlpha = alpha;

            yield return null; 
        }
        TeleportEffect.maskValue = 1.0f;
        TeleportEffect.maskColorAlpha = 1.0f; 

        transform.position = new Vector3(
            cursorHit.Value.point.x,
            transform.position.y,
            cursorHit.Value.point.z);

        while (TeleportEffect.maskValue > 0.02f)
        {
            // Update mask value 
            TeleportEffect.maskValue -= teleportFadeSpeed * Time.deltaTime;

            // update fade alpha 
            const float minAlpha = 0.3f;
            const float maxAlpha = 1.0f;
            float alpha = minAlpha + (maxAlpha - minAlpha) * (TeleportEffect.maskValue / 1.0f);
            TeleportEffect.maskColorAlpha = alpha;

            yield return null;
        }
        TeleportEffect.maskValue = 0.0f; 
        TeleportEffect.maskColorAlpha = 0.0f;

        OnDidMove(this, transform.position);
    }

    /** Update teleport target */ 
    void UpdateTeleportationCursor(){
        RaycastHit hit;
        if (CanTeleport && Physics.Raycast(position, forward, out hit, gvrReticlePointer.maxReticleDistance))
        {
            if (hit.collider.tag == "TeleportSurface")
            {
                gvrReticlePointer.gameObject.SetActive(false);
                floorCursor.gameObject.SetActive(true);

                floorCursor.transform.position = new Vector3(
                    hit.point.x,
                    floorCursor.transform.position.y,
                    hit.point.z);

                cursorHit = hit; 

                return; 
            }
        }

        cursorHit = null; 
        gvrReticlePointer.gameObject.SetActive(true);
        floorCursor.gameObject.SetActive(false);
    }
}
