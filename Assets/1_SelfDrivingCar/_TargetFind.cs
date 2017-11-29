using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _TargetFind : MonoBehaviour {

    UnityStandardAssets.Vehicles.Car.CarAIControl car;

    // Use this for initialization
	void Start () {
        car = GameObject.FindGameObjectWithTag("Player").GetComponent<UnityStandardAssets.Vehicles.Car.CarAIControl>();
        Debug.Log("tar_f-Start");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnWillRenderObject()
    {
        Debug.Log("On_will-Start");
        if (Camera.current.tag == "center_camera")
        {
            if (this.transform.position.y < 0.2)
            {
                car.SetTarget(this.transform);
            }
            
        }
    }
}
