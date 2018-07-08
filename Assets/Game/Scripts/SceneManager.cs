using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{

    #region Events 

    public delegate void ExhibitSelected(Exhibit caller);
    public ExhibitSelected OnExhibitSelected = delegate { };

    #endregion 

    public enum State
    {
        Introduction,
        Exploring
    }

    private static SceneManager _sharedInstance;

    public static SceneManager SharedInstance
    {
        get
        {
            if (_sharedInstance == null)
            {
                _sharedInstance = FindObjectOfType<SceneManager>();

                if (_sharedInstance == null)
                {
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

    public State CurrentState
    {
        get
        {
            return _currentState;
        }
        private set
        {
            _currentState = value;
        }
    }

    void Start()
    {
        virtualPersonalAssistant.OnStateChanged += VirtualPersonalAssistant_OnStateChanged;

        var exhibits = FindObjectsOfType<Exhibit>();
        foreach (var exhibit in exhibits)
        {
            exhibit.OnSelected += Exhibit_OnSelected;
        }
    }

    void Update()
    {

    }

    void VirtualPersonalAssistant_OnStateChanged(VPA caller, VPA.State state)
    {
        if (caller.CurrentState == VPA.State.Attentive)
        {
            CurrentState = State.Exploring;
        }
    }

    #region Cabnet handler

    void Exhibit_OnSelected(Exhibit caller)
    {
        OnExhibitSelected(caller); 
    }

    #endregion 

}
