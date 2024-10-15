using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PullInteraction : XRBaseInteractable
{
    public static event Action<float> PullActionReleased;

    public Transform start, end;
    public GameObject notch;

    //Amount pulled back on the BowString
    public float pullAmount { get; private set; } = 0.0f;

    //Line Renderer for the BowString
    private LineRenderer _lineRenderer;
    //Determines when the BowString is being pulled (Determines which hand)
    private IXRSelectInteractor _pullingInteractor = null;

    private ArrowSpawner _arrowSpawner;
    public Arrow arrowScript;

    private GameObject _arrow;

    //Audio
    private AudioSource _audioSource;

    //Change Arrow Instantiate to Bomb Arrow
    private bool _bombArrowIsActive;

    private bool _arrowIsSpawned;

    protected override void Awake()
    {

        base.Awake();
        _lineRenderer = GetComponent<LineRenderer>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _arrowSpawner = GameObject.Find("Bow").GetComponent<ArrowSpawner>();
    }

    //Is called on in OnSelectEntered
    public void SetPullInteractor(SelectEnterEventArgs args)
    {
        _pullingInteractor = args.interactorObject;
    }

    //Release of the BowString
    //Is called on in OnSelectExited
    //Signals to other scripts to tell the arrow to launch
    public void Release()
    {
        if (PullActionReleased != null)
        {
            PullActionReleased?.Invoke(pullAmount);

            _pullingInteractor = null;
            pullAmount = 0f; //Set to 0 because the BowString is no longer being pulled on

            notch.transform.localPosition = new Vector3(notch.transform.localPosition.x, notch.transform.localPosition.y, 0f);
            UpdateString();

            PlayReleaseAudio();

            //Null out the arrow variable
            //_arrow = null;
            _arrowIsSpawned = false;
        }
    }

    //Is called from the XR Interaction manager
    //Is called when the interacable is being interacted with
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);
        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic) //If it occurs during the Dynamic Phase
        {
            
            if (isSelected ) //Double checks if the interaction is selected
            {
                if (_bombArrowIsActive == true && _arrowIsSpawned == false)
                {
                    _arrowSpawner.InstantiateBombArrow();
                    Debug.Log("Switched to Bomb Arrows");
                }
                else if (_bombArrowIsActive == false && _arrowIsSpawned == false)
                {
                    _arrowSpawner.InstantiateArrow();
                    Debug.Log("Arrow has Spawned");
                }
                _arrowIsSpawned = true;
                Vector3 pullPosition = _pullingInteractor.transform.position; //Gets the pull position based on the Pull Interactor
                pullAmount = CalculatePull(pullPosition); // Calculates the Pull Amount
                                                          //Debug.Log("Pull Position: " + pullPosition + "Pull Amount: " + pullAmount);

                UpdateString();

            }

        }
    }

    //Math Stuff to calculate the values of pulling the BowString
    private float CalculatePull(Vector3 pullPosition) //May be where I determine the Force Amount ****
    {
        Vector3 pullDirection = pullPosition - start.position;
        Vector3 targetDirection = end.position - start.position;
        float maxLength = targetDirection.magnitude;

        targetDirection.Normalize();
        float pullValue = Vector3.Dot(pullDirection, targetDirection) / maxLength;
        return Mathf.Clamp(pullValue, 0, 1);
    }

    private void UpdateString()
    {
        //Finds the position based on the start and end points based on the Calculated Pull Amount on the Z axis
        Vector3 linePosition = Vector3.forward * Mathf.Lerp(start.transform.localPosition.z, end.transform.localPosition.z, pullAmount);

        //Notch value to match the pull on the BowString
        notch.transform.localPosition = new Vector3(notch.transform.localPosition.x, notch.transform.localPosition.y, linePosition.z + 0.2f);
        _lineRenderer.SetPosition(1, linePosition);
    }

    private void HapticFeedback()
    {
        if (_pullingInteractor != null)
        {
            ActionBasedController currentController = _pullingInteractor.transform.gameObject.GetComponent<ActionBasedController>();
            currentController.SendHapticImpulse(pullAmount, 0.1f);
        }
    }
    private void PlayReleaseAudio()
    {
        _audioSource.Play();
    }

    private IEnumerator BombDurationRoutine()
    {
        yield return new WaitForSeconds(5f);
        _bombArrowIsActive = false;
        Debug.Log("Bomb Arrow has been deactivated");
    }
    public void ReceiveBombInfoTrue()
    {
        _bombArrowIsActive = true;
        StartCoroutine(BombDurationRoutine());
        Debug.Log("Bomb arrows set to true on Pull Interaction Script");
    }

    public void ReceiveBombInfoFalse()
    {
        _bombArrowIsActive = false;
    }
}
