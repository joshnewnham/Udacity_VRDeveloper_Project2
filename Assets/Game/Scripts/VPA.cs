using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VikingCrewTools.UI;
using System;
using System.Linq;
using System.Text;
using IBM.Watson.DeveloperCloud.Services.TextToSpeech.v1;
using UnityEditor;

public class VPA : MonoBehaviour
{

    #region Events 

    public delegate void StateChanged(VPA caller, State state);
    public StateChanged OnStateChanged = delegate { };

    #endregion 

    public enum State
    {
        Undefined,
        IntroMoveTowardsPlayer,
        IntroGreet,
        Attentive,
        Presenting
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
    /** Flag indiciating that we're currently pursuing the player */
    private bool followingPlayer = false;

    private Cabnet selectedCabnet = null; 

    public string[] introDialog = {
            "Welcome to the Night at the Museum",
            "A place to explore and learn about Social VR ",
            "I'm Exe and will be your tour guide. ",
            "A few things to help you get started.",
            "You can move around by teleporting to where your cursor is on the floor",
            "Each cabnet has an artifact highlighting how Social Virtual Reality is being used today",
            "Select them with your cursor to have me give you a brief summary of the use-case"
        };

    public AudioClip[] introAudio;

    public State CurrentState
    {
        get
        {
            return _state;
        }
        set
        {
            var oldState = _state;
            _state = value;

            HandleStateChange(oldState, _state);
        }
    }

    // Use this for initialization
    void Start()
    {
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }

        player.OnDidMove += Player_OnDidMove;

        SceneManager.SharedInstance.OnCabnetSelected += SharedInstance_OnCabnetSelected;
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentState == State.Undefined)
        {
            CurrentState = State.IntroMoveTowardsPlayer;
        }

        if (followingPlayer)
        {
            FollowPlayer();
        } 
        else
        {
            if(CurrentState == State.Attentive){
                LookAtPlayer(); 
            }    
        }
    }

    void HandleStateChange(State from, State to)
    {
        if (from == to)
        {
            return;
        }

        Debug.LogFormat("State changed from {0} to {1}", from, to);

        switch (to)
        {
            case State.IntroMoveTowardsPlayer:
                StartCoroutine(IntroMoveTowardsPlayerState());
                break;
            case State.IntroGreet:
                StartCoroutine(IntroGreetState());
                break;
            case State.Presenting:
                StartCoroutine(PresentingState());
                break;
        }

        // broadcast 
        OnStateChanged(this, CurrentState);
    }

    void AddSpeechBubble(string text, float displayTime = 5.0f)
    {

        string linedText = string.Empty;
        int lineCharCount = 0;
        const int lineCharMax = 20;
        foreach (var word in text.Split(' '))
        {
            var wordCount = word.Length;
            if (lineCharCount + wordCount >= lineCharMax)
            {
                linedText += "\n";
                lineCharCount = 0;
            }

            if (lineCharCount > 0)
            {
                linedText += " ";
            }

            linedText += word;
            lineCharCount += wordCount;
        }

        SpeechBubbleManager.Instance.AddSpeechBubble(
            speechBubbleOrigin.transform.position,
            linedText,
            SpeechBubbleManager.SpeechbubbleType.NORMAL,
            displayTime,
            Color.white
        );
    }

    IEnumerator IntroMoveTowardsPlayerState()
    {
        while (CurrentState == State.IntroMoveTowardsPlayer)
        {
            var direction = (player.transform.position - transform.position);
            var distance = direction.magnitude;

            // close enough? 
            if (distance <= followDistance)
            {
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

    IEnumerator IntroGreetState()
    {

        for (var idx = 0; idx < introDialog.Length; idx++)
        {
            var text = introDialog[idx];
            var audioClip = introAudio[idx];

            yield return StartCoroutine(WaitForEyeContact());

            this.AddSpeechBubble(text, 5);

            GetComponent<GvrAudioSource>().PlayOneShot(audioClip);

            yield return new WaitForSeconds(6);
        }

        CurrentState = State.Attentive;
    }

    IEnumerator WaitForEyeContact()
    {
        while (Vector3.Dot(player.forward, transform.forward) > -0.8)
        {
            yield return null;
        }
    }

    IEnumerator PresentingState(){
        yield return StartCoroutine(MoveIntoPosition());

        for (var idx = 0; idx < selectedCabnet.summaryDialog.Length; idx++){
            if(CurrentState != State.Presenting){
                break; 
            }

            var text = selectedCabnet.summaryDialog[idx];
            var audioClip = selectedCabnet.summaryAudio[idx];

            yield return StartCoroutine(WaitForEyeContact());

            // Virtual Reality is an ideal medium for socialising; some of the key characteristics are
            var wordsPerSecond = 5.0f / 10.0f;
            var words = text.Split(' ').Length;
            var delay = wordsPerSecond * words;

            this.AddSpeechBubble(text, delay);

            GetComponent<GvrAudioSource>().PlayOneShot(audioClip);

            yield return new WaitForSeconds(delay+0.5f);
        }

        CurrentState = State.Attentive;
    }

    /// <summary>
    /// Move into front of the user to present material associated with the 
    /// selected cabnet 
    /// </summary>
    /// <returns>The into position.</returns>
    IEnumerator MoveIntoPosition()
    {
        if (CurrentState == State.Presenting)
        {
            const float offsetDistance = 2.5f;

            var direction = (selectedCabnet.transform.position - player.transform.position).normalized;
            direction.y = 0; 
            var targetPosition = selectedCabnet.transform.position + direction * offsetDistance;
            targetPosition.y = this.transform.position.y; 

            {
                // Rotate to look at destination 
                while (true)
                {
                    Vector3 previousForward = transform.forward; 

                    LookAt(targetPosition);

                    var changeInAngle = Vector3.Angle(previousForward, transform.forward);

                    if (changeInAngle < 0.01f)
                    {
                        break; 
                    }
                    yield return null;
                }
            }
            {
                // Move into position 
                while ((this.transform.position - targetPosition).magnitude > 0.05f)
                {
                    var targetDirection = (targetPosition - transform.position).normalized;
                    transform.position += targetDirection * translationSpeed * Time.deltaTime;
                    yield return null;
                }
            }

            {                
                // Rotate to face the player 
                while (true)
                {
                    Vector3 previousForward = transform.forward;

                    LookAt(player.transform.position);

                    var changeInAngle = Vector3.Angle(previousForward, transform.forward);

                    if (changeInAngle < 0.01f)
                    {
                        break;
                    }
                    yield return null;
                }
            }

        }
    }

    void Player_OnDidMove(Player caller, Vector3 position)
    {
        // Exit Presenting state (if in) 
        if (CurrentState == State.Presenting)
        {
            CurrentState = State.Attentive;
        }

        FollowPlayer();
    }

    void FollowPlayer()
    {
        var direction = (player.transform.position - transform.position);
        var distance = direction.magnitude;

        const float followPadding = 1;

        // close enough? 
        if (Math.Abs(distance - followDistance) <= followPadding)
        {
            followingPlayer = false;
            return;
        }

        followingPlayer = true;

        // rotate towards the player 
        direction.y = 0; 

        Vector3 newDir = Vector3.RotateTowards(transform.forward, direction.normalized, rotationSpeed, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDir);

        // move towards the player 
        transform.position += Mathf.Sign(distance - followDistance) * transform.forward * translationSpeed * Time.deltaTime;
    }

    void LookAtPlayer(){
        LookAt(player.transform.position);
    }

    void LookAt(Vector3 target){
        var direction = (target - transform.position);
        var distance = direction.magnitude;

        direction.y = 0;

        Vector3 newDir = Vector3.RotateTowards(transform.forward, direction.normalized, rotationSpeed, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDir);
    }

    void SharedInstance_OnCabnetSelected(Cabnet caller)
    {
        this.selectedCabnet = caller;
        CurrentState = State.Presenting;
    }

    #region Gaze events 

    public void onGazeEntered()
    {
        Debug.Log("Entered!");
    }

    public void onTapped()
    {
        Debug.Log("Tap!");
    }

    public void onGazeExited()
    {
        Debug.Log("Exit!");
    }

    #endregion 
}
