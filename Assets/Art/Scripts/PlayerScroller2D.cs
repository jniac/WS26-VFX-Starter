using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScroller2D : MonoBehaviour
{
    static Vector2 GetInput()
    {
        var inputX = 0f;
        var inputY = 0f;

        if (Keyboard.current != null)
        {
            inputX += Keyboard.current.aKey.isPressed ? -1f : 0f;
            inputX += Keyboard.current.dKey.isPressed ? 1f : 0f;
            inputY += Keyboard.current.sKey.isPressed ? -1f : 0f;
            inputY += Keyboard.current.wKey.isPressed ? 1f : 0f;
        }

        if (Gamepad.current != null)
        {
            inputX += Gamepad.current.leftStick.x.ReadValue();
            inputY += Gamepad.current.leftStick.y.ReadValue();
        }

        return Vector2.ClampMagnitude(new(inputX, inputY), 1f);
    }

    public float velocityMax = 4f;
    public float acceleration = 12f;
    [UnclampedRange(.9f, 1f)]
    public float friction = 0.99f;

    Vector2 velocity = Vector2.zero;

    void Update()
    {
        var inputVector = GetInput();

        var frictionFromInput = (1f - inputVector.magnitude) * friction;
        var decayRatio = Mathf.Exp(Mathf.Log(1f - frictionFromInput) * Time.deltaTime);
        velocity *= decayRatio;
        velocity += acceleration * Time.deltaTime * inputVector;
        velocity = Vector2.ClampMagnitude(velocity, velocityMax);

        transform.position += (Vector3)(velocity * Time.deltaTime);
    }
}
