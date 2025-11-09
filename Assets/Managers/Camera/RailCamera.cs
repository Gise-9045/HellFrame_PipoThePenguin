using System;
using UnityEngine;

[Serializable]
public class RailInfo
{
    [Header("Camera Position")]
    public Vector3 angle;
    
    public float distance = 10f;
    
    [Header("Transition")]
    public float transitionDuration = 1f;
    
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Movement Limits")]
    public bool useHorizontalLimitOnly = false;
    
    public float horizontalDeadzone = 2f;
    
    public float verticalDeadzone = 1f;
    
    public float maxHorizontalOffset = 8f;
    
    public float maxVerticalOffset = 5f;
}

public class RailCamera : MonoBehaviour
{
    private Transform cameraTransform;
    private Transform playerTransform;
    
    [SerializeField] private bool editMode = false;
    
    [Header("Current Rail Settings")]
    [SerializeField] private Vector3 angle;
    
    [SerializeField] private float distance = 10f;
    
    [Header("Follow Smoothness")]
    [SerializeField] private float followSmoothnessPosition = 10f;
    
    [SerializeField] private float followSmoothnessRotation = 10f;
    
    [Header("Current Movement Limits (Set by Active Rail)")]
    [SerializeField] private bool useHorizontalLimitOnly = false;
    
    [SerializeField] private float horizontalDeadzone = 2f;
    
    [SerializeField] private float verticalDeadzone = 1f;
    
    [SerializeField] private float maxHorizontalOffset = 8f;
    
    [SerializeField] private float maxVerticalOffset = 5f;
    
    [Header("Rail Switching Options")]
    [SerializeField] private bool resetConstrainedPositionOnRailChange = true;
    
    private Vector3 targetAngle;
    private float targetDistance;
    private Vector3 currentAngle;
    private float currentDistance;
    
    private bool isTransitioning = false;
    private float transitionTimer = 0f;
    private float transitionDuration;
    private AnimationCurve transitionCurve;
    private Vector3 transitionStartAngle;
    private float transitionStartDistance;
    
    private Vector3 currentCameraOffset;
    private Vector3 desiredPosition;
    private Vector3 currentVelocity;
    private Vector3 constrainedPlayerPosition;
    private Vector3 railAnchorPosition;
    
    private const float DEG_TO_RAD = Mathf.Deg2Rad;
    
    private void Awake()
    {
        cameraTransform = MainManager.Instance.MainCamera.transform;
        playerTransform = MainManager.Instance.PlayerGameobject.transform;
        
        currentAngle = angle;
        targetAngle = angle;
        currentDistance = distance;
        targetDistance = distance;
        
        constrainedPlayerPosition = playerTransform.position;
        railAnchorPosition = playerTransform.position;
    }
    
    private void OnValidate()
    {
        if (editMode)
        {
            currentAngle = angle;
            targetAngle = angle;
            currentDistance = distance;
            targetDistance = distance;
            
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                UnityEditor.SceneView.RepaintAll();
            }
#endif
        }
    }

    private void LateUpdate()
    {
        if (editMode)
        {
            currentAngle = angle;
            currentDistance = distance;
            FollowCharacter();
        }
        else
        {
            if (isTransitioning)
            {
                UpdateTransition();
            }
            FollowCharacter();
        }
    }

    public void SetRail(RailInfo railInfo)
    {
        // Store transition start values
        transitionStartAngle = currentAngle;
        transitionStartDistance = currentDistance;
        
        // Set transition targets
        targetAngle = railInfo.angle;
        targetDistance = railInfo.distance;
        
        transitionDuration = railInfo.transitionDuration;
        transitionCurve = railInfo.transitionCurve;
        
        // Apply movement limits immediately from the new rail
        ApplyRailLimits(railInfo);
        
        // Optionally reset constrained position to prevent sudden jumps
        if (resetConstrainedPositionOnRailChange)
        {
            ResetConstrainedPosition();
        }
        
        isTransitioning = true;
        transitionTimer = 0f;
    }
    
    private void ApplyRailLimits(RailInfo railInfo)
    {
        useHorizontalLimitOnly = railInfo.useHorizontalLimitOnly;
        horizontalDeadzone = railInfo.horizontalDeadzone;
        verticalDeadzone = railInfo.verticalDeadzone;
        maxHorizontalOffset = railInfo.maxHorizontalOffset;
        maxVerticalOffset = railInfo.maxVerticalOffset;
    }
    
    public void ApplyRailInfoForEditing(RailInfo railInfo)
    {
        angle = railInfo.angle;
        distance = railInfo.distance;
        currentAngle = angle;
        targetAngle = angle;
        currentDistance = distance;
        targetDistance = distance;
        ApplyRailLimits(railInfo);
    }
    
    public void CopyCurrentToRailInfo(RailInfo railInfo)
    {
        railInfo.angle = angle;
        railInfo.distance = distance;
        railInfo.useHorizontalLimitOnly = useHorizontalLimitOnly;
        railInfo.horizontalDeadzone = horizontalDeadzone;
        railInfo.verticalDeadzone = verticalDeadzone;
        railInfo.maxHorizontalOffset = maxHorizontalOffset;
        railInfo.maxVerticalOffset = maxVerticalOffset;
    }
    
    private void ResetConstrainedPosition()
    {
        constrainedPlayerPosition = playerTransform.position;
        railAnchorPosition = playerTransform.position;
    }

    private void UpdateTransition()
    {
        transitionTimer += Time.deltaTime;
        float normalizedTime = transitionTimer / transitionDuration;
        
        if (normalizedTime >= 1f)
        {
            currentAngle = targetAngle;
            currentDistance = targetDistance;
            angle = targetAngle;
            distance = targetDistance;
            isTransitioning = false;
            return;
        }
        
        float curveValue = transitionCurve.Evaluate(normalizedTime);
        
        currentAngle = Vector3.LerpUnclamped(transitionStartAngle, targetAngle, curveValue);
        currentDistance = Mathf.LerpUnclamped(transitionStartDistance, targetDistance, curveValue);
    }

   private void FollowCharacter()
{
    if (playerTransform == null || cameraTransform == null)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && editMode)
        {
            if (MainManager.Instance != null)
            {
                if (cameraTransform == null && MainManager.Instance.MainCamera != null)
                    cameraTransform = MainManager.Instance.MainCamera.transform;
                if (playerTransform == null && MainManager.Instance.PlayerGameobject != null)
                    playerTransform = MainManager.Instance.PlayerGameobject.transform;
            }

            if (playerTransform == null || cameraTransform == null)
                return;

            constrainedPlayerPosition = playerTransform.position;
            railAnchorPosition = playerTransform.position;
        }
        else
#endif
        {
            return;
        }
    }

    Vector3 playerPosition = playerTransform.position;
    UpdateConstrainedPosition(playerPosition);

    // ángulos en radianes
    float angleYRad = currentAngle.y * DEG_TO_RAD;
    float angleXRad = currentAngle.x * DEG_TO_RAD;

    float sinY = Mathf.Sin(angleYRad);
    float sinX = Mathf.Sin(angleXRad);
    float cosX = Mathf.Cos(angleXRad);
    float cosY = Mathf.Cos(angleYRad);

    currentCameraOffset.x = sinY * currentDistance;
    currentCameraOffset.y = sinX * currentDistance;
    currentCameraOffset.z = cosX * cosY * currentDistance;

    desiredPosition = constrainedPlayerPosition + currentCameraOffset;

#if UNITY_EDITOR
    if (!Application.isPlaying && editMode)
    {
        cameraTransform.position = desiredPosition;

        Vector3 lookDirection = constrainedPlayerPosition - cameraTransform.position;
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            cameraTransform.rotation = Quaternion.LookRotation(lookDirection);
        }
        return;
    }
#endif

    // --- SMOOTHING parameters protegidos ---
    float dt = Mathf.Max(Time.deltaTime, 0.00001f);

    // followSmoothnessPosition interpreta "velocidad" (mayor = más rápido). 
    // Convertimos a lambda (constante de tiempo) usable en la función exponencial:
    // t = 1 - exp(-lambda * dt). Elegimos lambda = followSmoothnessPosition (>= 0).
    float posLambda = Mathf.Max(followSmoothnessPosition, 0.0001f);
    float posT = 1f - Mathf.Exp(-posLambda * dt);

    // interpolamos posición con exponencial (frame-rate independent)
    cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPosition, posT);

    // Rotación: usamos la misma técnica exponencial para Slerp
    Vector3 lookDir = constrainedPlayerPosition - cameraTransform.position;
    if (lookDir.sqrMagnitude > 0.0001f)
    {
        Quaternion targetRot = Quaternion.LookRotation(lookDir);

        // followSmoothnessRotation interpreta "velocidad" (mayor = más rápido)
        float rotLambda = Mathf.Max(followSmoothnessRotation, 0.0001f);
        float rotT = 1f - Mathf.Exp(-rotLambda * dt);

        cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetRot, rotT);
    }
}

    
    private void UpdateConstrainedPosition(Vector3 playerPosition)
    {
        Vector3 offset = playerPosition - constrainedPlayerPosition;
        
        if (useHorizontalLimitOnly)
        {
            float absOffsetX = Mathf.Abs(offset.x);
            
            if (absOffsetX > horizontalDeadzone)
            {
                float excess = absOffsetX - horizontalDeadzone;
                constrainedPlayerPosition.x += Mathf.Sign(offset.x) * excess;
            }
            
            constrainedPlayerPosition.y = playerPosition.y;
            constrainedPlayerPosition.z = playerPosition.z;
        }
        else
        {
            float absOffsetX = Mathf.Abs(offset.x);
            float absOffsetY = Mathf.Abs(offset.y);
            
            if (absOffsetX > horizontalDeadzone)
            {
                float excess = absOffsetX - horizontalDeadzone;
                constrainedPlayerPosition.x += Mathf.Sign(offset.x) * excess;
            }
            
            if (absOffsetY > verticalDeadzone)
            {
                float excess = absOffsetY - verticalDeadzone;
                constrainedPlayerPosition.y += Mathf.Sign(offset.y) * excess;
            }
            
            constrainedPlayerPosition.z = playerPosition.z;
        }
        
        constrainedPlayerPosition.x = Mathf.Clamp(
            constrainedPlayerPosition.x,
            railAnchorPosition.x - maxHorizontalOffset,
            railAnchorPosition.x + maxHorizontalOffset
        );
        
        if (!useHorizontalLimitOnly)
        {
            constrainedPlayerPosition.y = Mathf.Clamp(
                constrainedPlayerPosition.y,
                railAnchorPosition.y - maxVerticalOffset,
                railAnchorPosition.y + maxVerticalOffset
            );
        }
    }

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        if (!editMode) return;
        
        Transform player = playerTransform;
        Transform cam = cameraTransform;
        
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (MainManager.Instance != null)
            {
                if (player == null && MainManager.Instance.PlayerGameobject != null)
                    player = MainManager.Instance.PlayerGameobject.transform;
                if (cam == null && MainManager.Instance.MainCamera != null)
                    cam = MainManager.Instance.MainCamera.transform;
            }
            
            // In edit mode, use player position as anchor for visualization
            if (player != null)
            {
                railAnchorPosition = player.position;
                constrainedPlayerPosition = player.position;
            }
        }
#endif
        
        if (player == null) return;
        
        Vector3 playerPos = player.position;
        
        // Draw the player
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(playerPos, 0.3f);
        
        // Draw the constrained position (camera look target)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(constrainedPlayerPosition, 0.25f);
        Gizmos.DrawLine(playerPos, constrainedPlayerPosition);
        
        // Draw deadzone around the constrained position (this moves with camera)
        Gizmos.color = new Color(1f, 1f, 0f, 0.5f); // Yellow, semi-transparent
        if (useHorizontalLimitOnly)
        {
            DrawWireCube(constrainedPlayerPosition, new Vector3(horizontalDeadzone * 2, 0.5f, 0.5f));
        }
        else
        {
            DrawWireCube(constrainedPlayerPosition, new Vector3(horizontalDeadzone * 2, verticalDeadzone * 2, 0.5f));
        }
        
        // Draw FIXED world-space limit box (centered on rail anchor)
        Gizmos.color = new Color(1f, 0f, 0f, 0.8f); // Red
        if (useHorizontalLimitOnly)
        {
            DrawWireCube(railAnchorPosition, new Vector3(maxHorizontalOffset * 2, 2f, 2f));
        }
        else
        {
            DrawWireCube(railAnchorPosition, new Vector3(maxHorizontalOffset * 2, maxVerticalOffset * 2, 2f));
        }
        
        // Draw anchor point
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(railAnchorPosition, 0.4f);
        
        // Draw camera position and look line
        if (cam != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(constrainedPlayerPosition, cam.position);
            Gizmos.DrawWireSphere(cam.position, 0.5f);
        }
        
        // Draw labels
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(playerPos + Vector3.up * 0.5f, "Player");
        UnityEditor.Handles.Label(constrainedPlayerPosition + Vector3.up * 0.5f, "Camera Target");
        UnityEditor.Handles.Label(railAnchorPosition + Vector3.up * 0.5f, "Rail Anchor");
        #endif
    }
    
    private void DrawWireCube(Vector3 center, Vector3 size)
    {
        Vector3 halfSize = size * 0.5f;
        
        Vector3[] points = new Vector3[8];
        points[0] = center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
        points[1] = center + new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
        points[2] = center + new Vector3(halfSize.x, -halfSize.y, halfSize.z);
        points[3] = center + new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
        points[4] = center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
        points[5] = center + new Vector3(halfSize.x, halfSize.y, -halfSize.z);
        points[6] = center + new Vector3(halfSize.x, halfSize.y, halfSize.z);
        points[7] = center + new Vector3(-halfSize.x, halfSize.y, halfSize.z);
        
        // Bottom face
        Gizmos.DrawLine(points[0], points[1]);
        Gizmos.DrawLine(points[1], points[2]);
        Gizmos.DrawLine(points[2], points[3]);
        Gizmos.DrawLine(points[3], points[0]);
        
        // Top face
        Gizmos.DrawLine(points[4], points[5]);
        Gizmos.DrawLine(points[5], points[6]);
        Gizmos.DrawLine(points[6], points[7]);
        Gizmos.DrawLine(points[7], points[4]);
        
        // Vertical edges
        Gizmos.DrawLine(points[0], points[4]);
        Gizmos.DrawLine(points[1], points[5]);
        Gizmos.DrawLine(points[2], points[6]);
        Gizmos.DrawLine(points[3], points[7]);
    }

    #endregion
    
}