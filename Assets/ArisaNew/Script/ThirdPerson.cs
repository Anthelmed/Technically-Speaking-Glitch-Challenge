using UnityEngine;

public class ThirdPerson : MonoBehaviour 
{
	public Transform player;                                          
	public Vector3 pivotOffset = new Vector3(0.0f, 1.7f,  0.0f);       
	public Vector3 camOffset   = new Vector3(0.0f, 0.0f, -3.0f);       
	public float smooth = 10f;                                        
	public float horizontalAimingSpeed = 6f;                          
	public float verticalAimingSpeed = 6f;                            
	public float maxVerticalAngle = 30f;                             
	public float minVerticalAngle = -60f;                              
	public string XAxis = "Analog X";                                 
	public string YAxis = "Analog Y";                                

	private float angleH = 0;                                        
	private float angleV = 0;                                          
	private Transform cam;                                             
	private Vector3 smoothPivotOffset;                                 
	private Vector3 smoothCamOffset;                                  
	private Vector3 targetPivotOffset;                                 
	private Vector3 targetCamOffset;                                  
	private float defaultFOV;                                         
	private float targetFOV;                                          
	private float targetMaxVerticalAngle;                             
	private bool isCustomOffset;                                      


	public float GetH { get { return angleH; } }

	void Awake()
	{
		cam = transform;

		cam.position = player.position + Quaternion.identity * pivotOffset + Quaternion.identity * camOffset;
		cam.rotation = Quaternion.identity;

		smoothPivotOffset = pivotOffset;
		smoothCamOffset = camOffset;
		defaultFOV = cam.GetComponent<Camera>().fieldOfView;
		angleH = player.eulerAngles.y;

		ResetTargetOffsets ();
		ResetFOV ();
		ResetMaxVerticalAngle();

		if (camOffset.y > 0)
			Debug.LogWarning("Vertical Cam Offset (Y) will be ignored during collisions!\n" +
				"It is recommended to set all vertical offset in Pivot Offset.");
	}

	void Update()
	{

		angleH += Mathf.Clamp(Input.GetAxis("Mouse X"), -1, 1) * horizontalAimingSpeed;
		angleV += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1, 1) * verticalAimingSpeed;

		angleV = Mathf.Clamp(angleV, minVerticalAngle, targetMaxVerticalAngle);

		Quaternion camYRotation = Quaternion.Euler(0, angleH, 0);
		Quaternion aimRotation = Quaternion.Euler(-angleV, angleH, 0);
		cam.rotation = aimRotation;

		cam.GetComponent<Camera>().fieldOfView = Mathf.Lerp (cam.GetComponent<Camera>().fieldOfView, targetFOV,  Time.deltaTime);

		Vector3 baseTempPosition = player.position + camYRotation * targetPivotOffset;
		Vector3 noCollisionOffset = targetCamOffset;
		while (noCollisionOffset.magnitude >= 0.2f)
		{
			if (DoubleViewingPosCheck(baseTempPosition + aimRotation * noCollisionOffset))
				break;
			noCollisionOffset -= noCollisionOffset.normalized * 0.2f;
		}
		if (noCollisionOffset.magnitude < 0.2f)
			noCollisionOffset = Vector3.zero;

		bool customOffsetCollision = isCustomOffset && noCollisionOffset.sqrMagnitude < targetCamOffset.sqrMagnitude;

		smoothPivotOffset = Vector3.Lerp(smoothPivotOffset, customOffsetCollision ? pivotOffset : targetPivotOffset, smooth * Time.deltaTime);
		smoothCamOffset = Vector3.Lerp(smoothCamOffset, customOffsetCollision ? Vector3.zero : noCollisionOffset, smooth * Time.deltaTime);

		cam.position =  player.position + camYRotation * smoothPivotOffset + aimRotation * smoothCamOffset;
	}


	public void SetTargetOffsets(Vector3 newPivotOffset, Vector3 newCamOffset)
	{
		targetPivotOffset = newPivotOffset;
		targetCamOffset = newCamOffset;
		isCustomOffset = true;
	}


	public void ResetTargetOffsets()
	{
		targetPivotOffset = pivotOffset;
		targetCamOffset = camOffset;
		isCustomOffset = false;
	}


	public void ResetYCamOffset()
	{
		targetCamOffset.y = camOffset.y;
	}


	public void SetYCamOffset(float y)
	{
		targetCamOffset.y = y;
	}


	public void SetXCamOffset(float x)
	{
		targetCamOffset.x = x;
	}


	public void SetFOV(float customFOV)
	{
		this.targetFOV = customFOV;
	}

	public void ResetFOV()
	{
		this.targetFOV = defaultFOV;
	}

	public void SetMaxVerticalAngle(float angle)
	{
		this.targetMaxVerticalAngle = angle;
	}

	public void ResetMaxVerticalAngle()
	{
		this.targetMaxVerticalAngle = maxVerticalAngle;
	}

	bool DoubleViewingPosCheck(Vector3 checkPos)
	{
		return ViewingPosCheck (checkPos) && ReverseViewingPosCheck (checkPos);
	}

	bool ViewingPosCheck (Vector3 checkPos)
	{
		Vector3 target = player.position + pivotOffset;
		Vector3 direction = target - checkPos;

		if (Physics.SphereCast(checkPos, 0.2f, direction, out RaycastHit hit, direction.magnitude))
		{
			if(hit.transform != player && !hit.transform.GetComponent<Collider>().isTrigger)
			{
				return false;
			}
		}

		return true;
	}


	bool ReverseViewingPosCheck(Vector3 checkPos)
	{
		Vector3 origin = player.position + pivotOffset;
		Vector3 direction = checkPos - origin;
		if (Physics.SphereCast(origin, 0.2f, direction, out RaycastHit hit, direction.magnitude))
		{
			if(hit.transform != player && hit.transform != transform && !hit.transform.GetComponent<Collider>().isTrigger)
			{
				return false;
			}
		}
		return true;
	}

	public float GetCurrentPivotMagnitude(Vector3 finalPivotOffset)
	{
		return Mathf.Abs ((finalPivotOffset - smoothPivotOffset).magnitude);
	}
}
