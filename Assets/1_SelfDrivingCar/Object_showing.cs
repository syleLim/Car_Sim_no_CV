using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_showing : MonoBehaviour {



	// Use this for initialization
	void Start () {
        		
	}
	
	// Update is called once per frame
	void Update () {
	    	
	}

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("col");
        if(other.tag == "Tar_Col")
        {
            Debug.Log("col-In");
            other.transform.GetChild(0).gameObject.SetActive(true);
        }
    }
}
