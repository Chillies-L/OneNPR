using UnityEngine;

/// <summary>
/// Rotates the attached object for model or material presentation.
/// </summary>
public class ShowcaseRotator : MonoBehaviour
{
    [Header("Rotation")]
    [Tooltip("Degrees per second.")]
    [SerializeField] private float speed = 30f;

    [Tooltip("Rotation axis. Y axis is the usual turntable direction.")]
    [SerializeField] private Vector3 axis = Vector3.up;

    [Tooltip("Use the object's local axis instead of the world axis.")]
    [SerializeField] private bool useLocalSpace = true;

    [Tooltip("Start rotating automatically when the scene begins.")]
    [SerializeField] private bool rotateOnStart = true;

    private bool isRotating;

    public float Speed
    {
        get => speed;
        set => speed = value;
    }

    public bool IsRotating => isRotating;

    private void Awake()
    {
        isRotating = rotateOnStart;
        NormalizeAxisIfNeeded();
    }

    private void OnValidate()
    {
        NormalizeAxisIfNeeded();
    }

    private void Update()
    {
        if (!isRotating || Mathf.Approximately(speed, 0f))
        {
            return;
        }

        Space rotationSpace = useLocalSpace ? Space.Self : Space.World;
        transform.Rotate(axis, speed * Time.deltaTime, rotationSpace);
    }

    public void Play()
    {
        isRotating = true;
    }

    public void Pause()
    {
        isRotating = false;
    }

    public void Toggle()
    {
        isRotating = !isRotating;
    }

    private void NormalizeAxisIfNeeded()
    {
        if (axis.sqrMagnitude < 0.0001f)
        {
            axis = Vector3.up;
            return;
        }

        axis.Normalize();
    }
}
