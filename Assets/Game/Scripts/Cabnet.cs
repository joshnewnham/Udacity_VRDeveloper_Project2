using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cabnet : MonoBehaviour {

    #region Events 

    public delegate void Selected(Cabnet caller);
    public Selected OnSelected= delegate { };

    #endregion 

    public string[] summaryDialog;

    public AudioClip[] summaryAudio; 


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SelectCabnet(){
        OnSelected(this); 
    }
}
