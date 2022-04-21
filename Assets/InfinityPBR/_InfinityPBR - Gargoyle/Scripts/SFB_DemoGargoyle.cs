using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SFB_DemoGargoyle : MonoBehaviour {

	private Animator animator;
	public Transform ballPoint;
	public Transform beamPoint;
	public GameObject ballPrefab;
	public GameObject beamPrefab;
	public GameObject ballSpell;
	public GameObject beamSpell;
	public GameObject flightSpell;
	public Vector3 flightSpellOffset;

	public Button superRandomButton;
	
	void Start(){
		animator = GetComponent<Animator> ();
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			superRandomButton.onClick.Invoke();
		}
	}

	public void Locomotion(float newValue){
		animator.SetFloat("locomotion", newValue);
	}

	public void PrecastStart()
	{
		if (ballSpell == null) return;
		ballSpell = Instantiate(ballPrefab, ballPoint.position, ballPoint.rotation);
		ballSpell.transform.parent = ballPoint;
		Destroy(ballSpell, 3.0f);
	}

	public void CastStart()
	{
		if (beamSpell == null) return;
		beamSpell						= Instantiate(beamPrefab, beamPoint.position, beamPoint.rotation);
		beamSpell.transform.parent		= beamPoint;
	}

	public void CastEnd()
	{
		if (beamSpell == null) return;
		if (beamSpell)
		{
			ParticleSystem ps = beamSpell.GetComponent<ParticleSystem> ();
			var em = ps.emission;
			em.enabled = false;
			foreach(Transform child in beamSpell.transform)
			{
				if (child.gameObject.GetComponent<ParticleSystem>()){
					ParticleSystem cps = beamSpell.GetComponent<ParticleSystem> ();
					var cem = cps.emission;
					cem.enabled = false;
					//break;
				}
				if (child.gameObject.GetComponent<Light>())
					child.gameObject.GetComponent<Light>().enabled = false;

				Destroy(child.gameObject);
			}
			Destroy(beamSpell, 3.0f);
		}
	}

	public void FlightCast()
	{
		if (flightSpell == null) return;
		GameObject newSpell = Instantiate(flightSpell, transform.position, Quaternion.identity);
		newSpell.transform.eulerAngles = new Vector3(-90,0,0);
		newSpell.transform.position = newSpell.transform.position + flightSpellOffset;
		Destroy (newSpell, 9.0f);
	}
	
	public void PlayAudio()
	{
		
	}

	public void StartLoop()
	{
		
	}

	public void StopLoop()
	{
		
	}
}
