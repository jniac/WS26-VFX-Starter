using UnityEngine;
using UnityEngine.InputSystem;

public class TunnelPlayer : MonoBehaviour
{
    public float tunnelRadius = 2f;

    [Range(-1f, 1f)]
    public float turn = 0f;

    void Update()
    {
        var arrowInput = Keyboard.current.leftArrowKey.isPressed ? -1f :
            Keyboard.current.rightArrowKey.isPressed ? 1f :
                0f;
        turn +=
            Gamepad.current.leftStick.x.ReadValue() * Time.deltaTime * 0.5f
            + arrowInput * Time.deltaTime * 0.5f;

        var angle = (turn - 0.25f) * 2f * Mathf.PI;
        var x = Mathf.Cos(angle) * tunnelRadius;
        var y = Mathf.Sin(angle) * tunnelRadius;
        transform.localPosition = new Vector3(x, y, transform.localPosition.z);
    }

    void OnValidate()
    {
        Update();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.parent.localToWorldMatrix;
        GizmosUtils.DrawCircle(Vector3.forward * transform.localPosition.z, tunnelRadius);
    }
}
