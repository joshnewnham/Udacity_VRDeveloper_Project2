using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VikingCrewTools.UI;
using System;
using System.Linq;

public class VPA : MonoBehaviour {

    #region Events 
    public delegate void StateChanged(VPA caller, State state);
    public StateChanged OnStateChanged = delegate { };

    #endregion 

    public enum State{
        Undefined, 
        IntroMoveTowardsPlayer, 
        IntroGreet, 
        Attentive  
    }

    /** Origin of the speech bubble */ 
    public GameObject speechBubbleOrigin;
    /** 'Comfortable' follow distance */ 
    public float followDistance = 2.0f;
    /** How fast the robot moves (units per second) */ 
    public float translationSpeed = 0.5f;
    /** How fast the robot rotates (degrees per second) */ 
    public float rotationSpeed = 2.0f; 

    /** State which drives the logic */ 
    private State _state = State.Undefined;
    /** Reference to the 'player' */
    public Player player; 

    public string[] introDialog = {
            "Welcome to the\nNight at the Museum",
            "A place to\nexplore and learn about\nSocial VR ",
            "I'm Exe and will be\nyour tour guide. ",
            "A few things to\nhelp you get started.",
            "You can move around \nby teleporting to where \nyour cursor is on the floor",
            "and interact with \nthe exhbits by selecting \nthem."
        };

    public AudioClip[] introAudio; 

    public State CurrentState{
        get{
            return _state; 
        }
        set{
            var oldState = _state; 
            _state = value;

            HandleStateChange(oldState, _state);
        }
    }

	// Use this for initialization
	void Start () {
        if(player == null){
            player = FindObjectOfType<Player>();    
        }
	}
	
	// Update is called once per frame
	void Update () {
        if(CurrentState == State.Undefined){
            CurrentState = State.IntroMoveTowardsPlayer;
        }
    }

    void HandleStateChange(State from, State to){
        if(from == to){
            return; 
        }

        Debug.LogFormat("State changed from {0} to {1}", from, to); 

        switch(to){
            case State.IntroMoveTowardsPlayer:
                StartCoroutine(IntroMoveTowardsPlayerState()); 
                break; 
            case State.IntroGreet:
                StartCoroutine(IntroGreetState());
                break; 
        }

        // broadcast 
        OnStateChanged(this, CurrentState);
    }

    IEnumerator IntroMoveTowardsPlayerState()
    {                
        while (CurrentState == State.IntroMoveTowardsPlayer){
            var direction = (player.transform.position - transform.position);
            var distance = direction.magnitude;

            // close enough? 
            if(distance <= followDistance){
                CurrentState = State.IntroGreet;
                continue; // exit 
            } 

            // rotate towards the player 

            Vector3 newDir = Vector3.RotateTowards(transform.forward, direction.normalized, rotationSpeed, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDir);

            // move towards the player 
            transform.position += transform.forward * translationSpeed * Time.deltaTime;

            yield return null;
        } 
    }

    IEnumerator IntroGreetState(){

        for(var idx= 0; idx<introDialog.Length; idx++){
            var text = introDialog[idx];
            var audio = introAudio[idx];

            yield return StartCoroutine(WaitForEyeContact());

            SpeechBubbleManager.Instance.AddSpeechBubble(
                speechBubbleOrigin.transform.position,
                text,
                SpeechBubbleManager.SpeechbubbleType.NORMAL,
                5.0f,
                Color.white
            );

            GetComponent<GvrAudioSource>().PlayOneShot(audio); 

            yield return new WaitForSeconds(6);
        }

        CurrentState = State.Attentive;
    }

    IEnumerator WaitForEyeContact()
    {        
        while(Vector3.Dot(player.forward, transform.forward) > -0.8){
            yield return null;
        }
    }

    public void onGazeEntered()
    {
        Debug.Log("Entered!");
    }

    public void onTapped(){
        Debug.Log("Tap!");
    }

    public void onGazeExited()
    {
        Debug.Log("Exit!");
    }
}
