using UnityEngine;

namespace ScenarioKit.Demo
{
    /// <summary>WASD / arrow movement on the floor plane.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerMover : MonoBehaviour
    {
        [SerializeField, Min(0f)] float speed = 5f;

        Rigidbody body;

        void Awake() => body = GetComponent<Rigidbody>();

        void FixedUpdate()
        {
            var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            var velocity = Vector3.ClampMagnitude(input, 1f) * speed;
            velocity.y = body.linearVelocity.y;
            body.linearVelocity = velocity;
        }
    }
}
