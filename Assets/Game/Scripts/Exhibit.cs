using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Exhibit : MonoBehaviour {

    #region Events 

    public delegate void Selected(Exhibit caller);
    public Selected OnSelected= delegate { };

    #endregion 

    public enum ExhibitType{
        Cabnet, 
        Mural
    }

    public ExhibitType exhibitType = ExhibitType.Cabnet;

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

    public void SelectExhibit(){
        // Ignore the y-axis 
        var playerPosition = SceneManager.SharedInstance.player.transform.position;
        playerPosition.y = 0;
        var position = this.transform.position;
        position.y = 0;

        var distanceToPlayer = (playerPosition - position).magnitude;

        Debug.LogFormat("Distance to player {0}", distanceToPlayer);

        if(distanceToPlayer > selectDistanceThreshold){
            return; // ignore 
        }

        OnSelected(this); 
    }
}
