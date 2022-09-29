using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerBasicCode : MonoBehaviour
{
	public Transform playerCamera;                        
	public float turnSmoothing = 0.06f;                   
	public float sprintFOV = 100f;                             
	private float h;                                     
	private float v;                                      
	private int currentBehaviour;                        
	private int defaultBehaviour;                        
	private int behaviourLocked;                          
	private Vector3 lastDirection;                        
	private Animator anim;                                
	private ThirdPerson camScript;          
	private bool sprint;                                 
	private bool changedFOV;                             
	private int hFloat;                                   
	private int vFloat;                                  
	private List<GenericBehaviour> behaviours;            
	private List<GenericBehaviour> overridingBehaviours;  
	private Rigidbody rBody;                             
	private int groundedBool;                             
	private Vector3 colExtents;                           


	public float GetH { get { return h; } }
	public float GetV { get { return v; } }


	public ThirdPerson GetCamScript { get { return camScript; } }


	public Rigidbody GetRigidBody { get { return rBody; } }


	public Animator GetAnim { get { return anim; } }


	public int GetDefaultBehaviour {  get { return defaultBehaviour; } }

	void Awake ()
	{
		behaviours = new List<GenericBehaviour> ();
		overridingBehaviours = new List<GenericBehaviour>();
		anim = GetComponent<Animator> ();
		hFloat = Animator.StringToHash("H");
		vFloat = Animator.StringToHash("V");
		camScript = playerCamera.GetComponent<ThirdPerson> ();
		rBody = GetComponent<Rigidbody> ();

		groundedBool = Animator.StringToHash("Grounded");
		colExtents = GetComponent<Collider>().bounds.extents;
	}

	void Update()
	{
		h = Input.GetAxis("Horizontal");
		v = Input.GetAxis("Vertical");


		anim.SetFloat(hFloat, h, 0.1f, Time.deltaTime);
		anim.SetFloat(vFloat, v, 0.1f, Time.deltaTime);


		sprint = Input.GetKey(KeyCode.LeftShift);


		if (IsSprinting())
		{
			changedFOV = true;
			camScript.SetFOV(sprintFOV);
		}
		else if(changedFOV)
		{
			camScript.ResetFOV();
			changedFOV = false;
		}
		anim.SetBool(groundedBool, IsGrounded());
	}


	void FixedUpdate()
	{
		bool isAnyBehaviourActive = false;
		if (behaviourLocked > 0 || overridingBehaviours.Count == 0)
		{
			foreach (GenericBehaviour behaviour in behaviours)
			{
				if (behaviour.isActiveAndEnabled && currentBehaviour == behaviour.GetBehaviourCode())
				{
					isAnyBehaviourActive = true;
					behaviour.LocalFixedUpdate();
				}
			}
		}

		else
		{
			foreach (GenericBehaviour behaviour in overridingBehaviours)
			{
				behaviour.LocalFixedUpdate();
			}
		}


		if (!isAnyBehaviourActive && overridingBehaviours.Count == 0)
		{
			rBody.useGravity = true;
			Repositioning ();
		}
	}


	private void LateUpdate()
	{
		if (behaviourLocked > 0 || overridingBehaviours.Count == 0)
		{
			foreach (GenericBehaviour behaviour in behaviours)
			{
				if (behaviour.isActiveAndEnabled && currentBehaviour == behaviour.GetBehaviourCode())
				{
					behaviour.LocalLateUpdate();
				}
			}
		}

		else
		{
			foreach (GenericBehaviour behaviour in overridingBehaviours)
			{
				behaviour.LocalLateUpdate();
			}
		}

	}


	public void SubscribeBehaviour(GenericBehaviour behaviour)
	{
		behaviours.Add (behaviour);
	}


	public void RegisterDefaultBehaviour(int behaviourCode)
	{
		defaultBehaviour = behaviourCode;
		currentBehaviour = behaviourCode;
	}


	public void RegisterBehaviour(int behaviourCode)
	{
		if (currentBehaviour == defaultBehaviour)
		{
			currentBehaviour = behaviourCode;
		}
	}


	public void UnregisterBehaviour(int behaviourCode)
	{
		if (currentBehaviour == behaviourCode)
		{
			currentBehaviour = defaultBehaviour;
		}
	}


	public bool OverrideWithBehaviour(GenericBehaviour behaviour)
	{
		if (!overridingBehaviours.Contains(behaviour))
		{
			if (overridingBehaviours.Count == 0)
			{
				foreach (GenericBehaviour overriddenBehaviour in behaviours)
				{
					if (overriddenBehaviour.isActiveAndEnabled && currentBehaviour == overriddenBehaviour.GetBehaviourCode())
					{
						overriddenBehaviour.OnOverride();
						break;
					}
				}
			}
			overridingBehaviours.Add(behaviour);
			return true;
		}
		return false;
	}


	public bool RevokeOverridingBehaviour(GenericBehaviour behaviour)
	{
		if (overridingBehaviours.Contains(behaviour))
		{
			overridingBehaviours.Remove(behaviour);
			return true;
		}
		return false;
	}


	public bool IsOverriding(GenericBehaviour behaviour = null)
	{
		if (behaviour == null)
			return overridingBehaviours.Count > 0;
		return overridingBehaviours.Contains(behaviour);
	}


	public bool IsCurrentBehaviour(int behaviourCode)
	{
		return this.currentBehaviour == behaviourCode;
	}

	public bool GetTempLockStatus(int behaviourCodeIgnoreSelf = 0)
	{
		return (behaviourLocked != 0 && behaviourLocked != behaviourCodeIgnoreSelf);
	}


	public void LockTempBehaviour(int behaviourCode)
	{
		if (behaviourLocked == 0)
		{
			behaviourLocked = behaviourCode;
		}
	}


	public void UnlockTempBehaviour(int behaviourCode)
	{
		if(behaviourLocked == behaviourCode)
		{
			behaviourLocked = 0;
		}
	}

	public virtual bool IsSprinting()
	{
		return sprint && IsMoving() && CanSprint();
	}


	public bool CanSprint()
	{
		foreach (GenericBehaviour behaviour in behaviours)
		{
			if (!behaviour.AllowSprint ())
				return false;
		}
		foreach(GenericBehaviour behaviour in overridingBehaviours)
		{
			if (!behaviour.AllowSprint())
				return false;
		}
		return true;
	}


	public bool IsHorizontalMoving()
	{
		return h != 0;
	}


	public bool IsMoving()
	{
		return (h != 0)|| (v != 0);
	}


	public Vector3 GetLastDirection()
	{
		return lastDirection;
	}


	public void SetLastDirection(Vector3 direction)
	{
		lastDirection = direction;
	}


	public void Repositioning()
	{
		if(lastDirection != Vector3.zero)
		{
			lastDirection.y = 0;
			Quaternion targetRotation = Quaternion.LookRotation (lastDirection);
			Quaternion newRotation = Quaternion.Slerp(rBody.rotation, targetRotation, turnSmoothing);
			rBody.MoveRotation (newRotation);
		}
	}


	public bool IsGrounded()
	{
		Ray ray = new Ray(this.transform.position + Vector3.up * 2 * colExtents.x, Vector3.down);
		return Physics.SphereCast(ray, colExtents.x, colExtents.x + 0.2f);
	}
}


public abstract class GenericBehaviour : MonoBehaviour
{                     
	protected int speedFloat;                     
	protected PlayerBasicCode behaviourManager;     
	protected int behaviourCode;                  
	protected bool canSprint;                     

	void Awake()
	{
		behaviourManager = GetComponent<PlayerBasicCode> ();
		speedFloat = Animator.StringToHash("Speed");
		canSprint = true;

		behaviourCode = this.GetType().GetHashCode();
	}


	

	public virtual void LocalFixedUpdate() { }

	public virtual void LocalLateUpdate() { }

	public virtual void OnOverride() { }


	public int GetBehaviourCode()
	{
		return behaviourCode;
	}


	public bool AllowSprint()
	{
		return canSprint;
	}
}
