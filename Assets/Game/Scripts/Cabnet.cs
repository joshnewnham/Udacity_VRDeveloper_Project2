using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Cabnet : MonoBehaviour {

    #region Events 

    public delegate void Selected(Cabnet caller);
    public Selected OnSelected= delegate { };

    #endregion 

    [Tooltip("Min distance the user needs to be for this to be selected")]
    public float selectDistanceThreshold = 2.5f; 

    public string[] summaryDialog;

    public AudioClip[] summaryAudio; 


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SelectCabnet(){
        var distanceToPlayer = (SceneManager.SharedInstance.player.transform.position - this.transform.position).magnitude;

        Debug.LogFormat("Distance to player {0}", distanceToPlayer);

        if(distanceToPlayer > selectDistanceThreshold){
            return; // ignore 
        }

        OnSelected(this); 
    }
}
