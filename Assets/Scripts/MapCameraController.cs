using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class MapCameraController : MonoBehaviour
{
    [Header("Ground")]
    [SerializeField] private float groundY = 0f;

    [Header("Map Bounds")]
    [SerializeField] private float minX = -30f;
    [SerializeField] private float maxX = 30f;
    [SerializeField] private float minZ = -30f;
    [SerializeField] private float maxZ = 30f;

    [Header("Drag")]
    [SerializeField] private float inertiaStrength = 0.85f;
    [SerializeField] private float inertiaDamping = 7f;
    [SerializeField] private float maximumInertiaSpeed = 25f;

    [Header("Zoom")]
    [SerializeField] private float minCameraHeight = 7f;
    [SerializeField] private float maxCameraHeight = 22f;

    [SerializeField] private float mouseZoomSpeed = 1.5f;
    [SerializeField] private float pinchZoomSpeed = 0.012f;

    [Header("Desktop")]
    [SerializeField] private float keyboardMoveSpeed = 10f;

    private Camera targetCamera;
    private Plane groundPlane;

    private bool mouseDragging;
    private Vector3 mouseGrabWorldPoint;

    private bool touchDragging;
    private Vector3 touchGrabWorldPoint;

    private Vector3 inertiaVelocity;

    private Vector2 previousPinchCenter;
    private float previousPinchDistance;

    private bool pinchActive;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();

        groundPlane = new Plane(
            Vector3.up,
            new Vector3(
                0f,
                groundY,
                0f
            )
        );

        if (targetCamera == null)
        {
            Debug.LogError(
                "MapCameraController must be attached to a Camera."
            );
        }
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        if (targetCamera == null)
            return;

        bool desktopUsed =
            HandleDesktopInput();

        bool touchUsed =
            HandleTouchInput();

        if (
            !desktopUsed &&
            !touchUsed &&
            !mouseDragging &&
            !touchDragging &&
            !pinchActive
        )
        {
            ApplyInertia();
        }

        ClampToMapBounds();
    }

    private bool HandleDesktopInput()
    {
        bool usedInput = false;

        Mouse mouse =
            Mouse.current;

        Keyboard keyboard =
            Keyboard.current;

        if (keyboard != null)
        {
            Vector2 movementInput =
                Vector2.zero;

            if (
                keyboard.aKey.isPressed ||
                keyboard.leftArrowKey.isPressed
            )
            {
                movementInput.x -= 1f;
            }

            if (
                keyboard.dKey.isPressed ||
                keyboard.rightArrowKey.isPressed
            )
            {
                movementInput.x += 1f;
            }

            if (
                keyboard.wKey.isPressed ||
                keyboard.upArrowKey.isPressed
            )
            {
                movementInput.y += 1f;
            }

            if (
                keyboard.sKey.isPressed ||
                keyboard.downArrowKey.isPressed
            )
            {
                movementInput.y -= 1f;
            }

            if (movementInput.sqrMagnitude > 0f)
            {
                MoveWithKeyboard(
                    movementInput.normalized
                );

                inertiaVelocity =
                    Vector3.zero;

                usedInput = true;
            }
        }

        if (mouse == null)
            return usedInput;

        Vector2 mousePosition =
            mouse.position.ReadValue();

        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (!IsPointerOverUI())
            {
                if (TryGetGroundPoint(
                    mousePosition,
                    out Vector3 groundPoint
                ))
                {
                    mouseDragging = true;

                    mouseGrabWorldPoint =
                        groundPoint;

                    inertiaVelocity =
                        Vector3.zero;

                    usedInput = true;
                }
            }
        }

        if (
            mouse.leftButton.isPressed &&
            mouseDragging
        )
        {
            if (TryGetGroundPoint(
                mousePosition,
                out Vector3 currentGroundPoint
            ))
            {
                Vector3 movement =
                    mouseGrabWorldPoint -
                    currentGroundPoint;

                movement.y = 0f;

                transform.position +=
                    movement;

                if (Time.unscaledDeltaTime > 0f)
                {
                    Vector3 frameVelocity =
                        movement /
                        Time.unscaledDeltaTime;

                    inertiaVelocity =
                        Vector3.Lerp(
                            inertiaVelocity,
                            frameVelocity,
                            0.35f
                        );
                }

                if (TryGetGroundPoint(
                    mousePosition,
                    out Vector3 correctedPoint
                ))
                {
                    mouseGrabWorldPoint =
                        correctedPoint;
                }

                usedInput = true;
            }
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            mouseDragging = false;

            LimitInertiaVelocity();
        }

        float scroll =
            mouse.scroll.ReadValue().y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 zoomAnchor;

            if (!TryGetGroundPoint(
                mousePosition,
                out zoomAnchor
            ))
            {
                TryGetScreenCenterGroundPoint(
                    out zoomAnchor
                );
            }

            float normalizedScroll =
                Mathf.Sign(scroll);

            ApplyPhysicalZoom(
                normalizedScroll *
                mouseZoomSpeed,
                zoomAnchor
            );

            inertiaVelocity =
                Vector3.zero;

            usedInput = true;
        }

        return usedInput;
    }

    private bool HandleTouchInput()
    {
        var touches =
            EnhancedTouch.activeTouches;

        if (touches.Count == 0)
        {
            touchDragging = false;
            pinchActive = false;

            return false;
        }

        if (touches.Count >= 2)
        {
            touchDragging = false;

            var touch0 =
                touches[0];

            var touch1 =
                touches[1];

            Vector2 position0 =
                touch0.screenPosition;

            Vector2 position1 =
                touch1.screenPosition;

            Vector2 pinchCenter =
                (position0 + position1) *
                0.5f;

            float pinchDistance =
                Vector2.Distance(
                    position0,
                    position1
                );

            if (!pinchActive)
            {
                pinchActive = true;

                previousPinchCenter =
                    pinchCenter;

                previousPinchDistance =
                    pinchDistance;

                inertiaVelocity =
                    Vector3.zero;

                return true;
            }

            float pinchDelta =
                pinchDistance -
                previousPinchDistance;

            Vector3 zoomAnchor;

            if (!TryGetGroundPoint(
                pinchCenter,
                out zoomAnchor
            ))
            {
                TryGetScreenCenterGroundPoint(
                    out zoomAnchor
                );
            }

            ApplyPhysicalZoom(
                pinchDelta *
                pinchZoomSpeed,
                zoomAnchor
            );

            previousPinchCenter =
                pinchCenter;

            previousPinchDistance =
                pinchDistance;

            inertiaVelocity =
                Vector3.zero;

            return true;
        }

        pinchActive = false;

        var touch =
            touches[0];

        Vector2 touchPosition =
            touch.screenPosition;

        if (
            touch.phase ==
            UnityEngine.InputSystem.TouchPhase.Began
        )
        {
            if (IsTouchOverUI(touch.touchId))
            {
                touchDragging = false;

                return true;
            }

            if (TryGetGroundPoint(
                touchPosition,
                out Vector3 groundPoint
            ))
            {
                touchDragging = true;

                touchGrabWorldPoint =
                    groundPoint;

                inertiaVelocity =
                    Vector3.zero;
            }

            return true;
        }

        if (
            touch.phase ==
                UnityEngine.InputSystem.TouchPhase.Moved &&
            touchDragging
        )
        {
            if (TryGetGroundPoint(
                touchPosition,
                out Vector3 currentGroundPoint
            ))
            {
                Vector3 movement =
                    touchGrabWorldPoint -
                    currentGroundPoint;

                movement.y = 0f;

                transform.position +=
                    movement;

                if (Time.unscaledDeltaTime > 0f)
                {
                    Vector3 frameVelocity =
                        movement /
                        Time.unscaledDeltaTime;

                    inertiaVelocity =
                        Vector3.Lerp(
                            inertiaVelocity,
                            frameVelocity,
                            0.35f
                        );
                }

                if (TryGetGroundPoint(
                    touchPosition,
                    out Vector3 correctedPoint
                ))
                {
                    touchGrabWorldPoint =
                        correctedPoint;
                }
            }

            return true;
        }

        if (
            touch.phase ==
                UnityEngine.InputSystem.TouchPhase.Ended ||
            touch.phase ==
                UnityEngine.InputSystem.TouchPhase.Canceled
        )
        {
            touchDragging = false;

            LimitInertiaVelocity();

            return true;
        }

        return touchDragging;
    }

    private void MoveWithKeyboard(
        Vector2 input
    )
    {
        Vector3 right =
            transform.right;

        right.y = 0f;
        right.Normalize();

        Vector3 forward =
            transform.forward;

        forward.y = 0f;
        forward.Normalize();

        Vector3 movement =
            (
                right *
                input.x +
                forward *
                input.y
            ) *
            keyboardMoveSpeed *
            Time.unscaledDeltaTime;

        transform.position +=
            movement;
    }

    private void ApplyPhysicalZoom(
        float amount,
        Vector3 anchorPoint
    )
    {
        if (Mathf.Abs(amount) < 0.0001f)
            return;

        Vector3 originalPosition =
            transform.position;

        Vector3 movement =
            transform.forward *
            amount;

        transform.position +=
            movement;

        ClampCameraHeight();

        if (TryGetScreenPointForWorldAnchor(
            anchorPoint,
            out Vector2 anchorScreenPosition
        ))
        {
            if (TryGetGroundPoint(
                anchorScreenPosition,
                out Vector3 newAnchorPoint
            ))
            {
                Vector3 correction =
                    anchorPoint -
                    newAnchorPoint;

                correction.y = 0f;

                transform.position +=
                    correction;
            }
        }

        ClampCameraHeight();

        if (
            float.IsNaN(transform.position.x) ||
            float.IsNaN(transform.position.y) ||
            float.IsNaN(transform.position.z)
        )
        {
            transform.position =
                originalPosition;
        }
    }

    private void ClampCameraHeight()
    {
        float currentY =
            transform.position.y;

        if (
            currentY >= minCameraHeight &&
            currentY <= maxCameraHeight
        )
        {
            return;
        }

        float targetY =
            Mathf.Clamp(
                currentY,
                minCameraHeight,
                maxCameraHeight
            );

        float forwardY =
            transform.forward.y;

        if (Mathf.Abs(forwardY) < 0.0001f)
            return;

        float requiredDistance =
            (targetY - currentY) /
            forwardY;

        transform.position +=
            transform.forward *
            requiredDistance;
    }

    private void ApplyInertia()
    {
        if (inertiaVelocity.sqrMagnitude < 0.001f)
        {
            inertiaVelocity =
                Vector3.zero;

            return;
        }

        transform.position +=
            inertiaVelocity *
            Time.unscaledDeltaTime;

        float damping =
            Mathf.Exp(
                -inertiaDamping *
                Time.unscaledDeltaTime
            );

        inertiaVelocity *=
            damping;
    }

    private void LimitInertiaVelocity()
    {
        inertiaVelocity *=
            inertiaStrength;

        inertiaVelocity =
            Vector3.ClampMagnitude(
                inertiaVelocity,
                maximumInertiaSpeed
            );
    }

    private void ClampToMapBounds()
    {
        if (!TryGetScreenCenterGroundPoint(
            out Vector3 centerPoint
        ))
        {
            return;
        }

        float clampedX =
            Mathf.Clamp(
                centerPoint.x,
                minX,
                maxX
            );

        float clampedZ =
            Mathf.Clamp(
                centerPoint.z,
                minZ,
                maxZ
            );

        Vector3 correction =
            new Vector3(
                clampedX - centerPoint.x,
                0f,
                clampedZ - centerPoint.z
            );

        if (correction.sqrMagnitude > 0f)
        {
            transform.position +=
                correction;

            inertiaVelocity =
                Vector3.zero;
        }
    }

    private bool TryGetGroundPoint(
        Vector2 screenPosition,
        out Vector3 worldPoint
    )
    {
        Ray ray =
            targetCamera.ScreenPointToRay(
                screenPosition
            );

        if (groundPlane.Raycast(
            ray,
            out float enter
        ))
        {
            worldPoint =
                ray.GetPoint(enter);

            return true;
        }

        worldPoint =
            Vector3.zero;

        return false;
    }

    private bool TryGetScreenCenterGroundPoint(
        out Vector3 worldPoint
    )
    {
        Vector2 screenCenter =
            new Vector2(
                Screen.width * 0.5f,
                Screen.height * 0.5f
            );

        return TryGetGroundPoint(
            screenCenter,
            out worldPoint
        );
    }

    private bool TryGetScreenPointForWorldAnchor(
        Vector3 worldPoint,
        out Vector2 screenPoint
    )
    {
        Vector3 viewportPoint =
            targetCamera.WorldToScreenPoint(
                worldPoint
            );

        if (viewportPoint.z <= 0f)
        {
            screenPoint =
                Vector2.zero;

            return false;
        }

        screenPoint =
            new Vector2(
                viewportPoint.x,
                viewportPoint.y
            );

        return true;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current
            .IsPointerOverGameObject();
    }

    private bool IsTouchOverUI(
        int touchId
    )
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current
            .IsPointerOverGameObject(
                touchId
            );
    }
}