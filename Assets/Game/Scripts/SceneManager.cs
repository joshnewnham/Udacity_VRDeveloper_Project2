using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour {

    public enum State{
        Introduction, 
        Exploring 
    }

    private static SceneManager _sharedInstance; 

    public static SceneManager SharedInstance{
        get
        {
            if(_sharedInstance == null){
                _sharedInstance = FindObjectOfType<SceneManager>(); 

                if(_sharedInstance == null){
                    var go = new GameObject("SceneManager");
                    _sharedInstance = go.AddComponent<SceneManager>(); 
                }
            }
            
            return _sharedInstance; 
        }
    }

    /** Reference to the virtual personal assistant */ 
    public VPA virtualPersonalAssistant;
    /** Reference to the player */ 
    public Player player;

    /** Current state of our scene; influences control flow */ 
    private State _currentState = State.Introduction; 

    public State CurrentState{
        get{
            return _currentState; 
        }
        private set{
            _currentState = value; 
        }
    }

	void Start () {
        virtualPersonalAssistant.OnStateChanged += VirtualPersonalAssistant_OnStateChanged;
	}
		
	void Update () {
		
	}

    void VirtualPersonalAssistant_OnStateChanged(VPA caller, VPA.State state)
    {
        if(caller.CurrentState == VPA.State.Attentive){            
            CurrentState = State.Exploring;
        }
    }
}
