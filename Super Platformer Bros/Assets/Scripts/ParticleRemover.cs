using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleRemover : MonoBehaviour {
    new ParticleSystem particleSystem;

	void Start () {
        particleSystem = GetComponent<ParticleSystem>();
	}
	
	void Update () {
        if (particleSystem.isPlaying)
            return;

        Destroy(gameObject);
	}
}
