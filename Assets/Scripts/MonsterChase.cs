using UnityEngine; // Gives access to Unity core types like MonoBehaviour, Vector3, Transform, Time, Rigidbody, etc.
using UnityEngine.Pool; // Gives access to Unity’s object pooling interfaces/classes like IObjectPool.

public class PooledBullet : MonoBehaviour // Defines a bullet component that can be reused via pooling.
{
    [SerializeField] private float speed = 30f; // Bullet travel speed (units per second).
    [SerializeField] private float lifetime = 2f; // How long the bullet stays alive before returning to the pool.

    private IObjectPool<PooledBullet> pool; // Reference to the pool that owns this bullet (used to release it back).
    private float timer; // Tracks how long this bullet has been active since it was fired.

    // Optional: if using Rigidbody, you must reset velocity too // Comment: physics-based bullets need velocity resets when reused.
    private Rigidbody rb; // Cached Rigidbody reference (if the bullet uses physics).

    private void Awake() // Unity event called when the object is initialized.
    {
        rb = GetComponent<Rigidbody>(); // Tries to grab a Rigidbody if attached; safe even if none exists (rb stays null).
    }

    public void SetPool(IObjectPool<PooledBullet> owningPool) // Called by the gun/pool setup to assign the owning pool.
    {
        pool = owningPool; // Stores the pool reference so the bullet can return itself later.
    }

    public void Fire(Vector3 direction) // Called when the bullet is spawned/reused to “start” it moving.
    {
        // Reset pooled state every time you reuse it // Comment: important because pooled objects keep old state unless reset.
        timer = 0f; // Resets lifetime timer so the bullet lives a full lifetime again.
        transform.forward = direction; // Points the bullet in the desired direction (used for movement).

        // If you use Rigidbody movement, reset velocity/angVel here // Comment: prevents old physics motion from carrying over.
        if (rb != null) // Only do physics resets if a Rigidbody exists.
        {
            rb.linearVelocity = Vector3.zero; // Clears current linear velocity so it doesn’t keep moving from last use.
            rb.angularVelocity = Vector3.zero; // Clears rotation velocity so it doesn’t keep spinning from last use.
        }
    }

    private void Update() // Unity event called every frame while this bullet is active.
    {
        // Simple movement // Comment: manual transform movement (not physics-based).
        transform.position += transform.forward * speed * Time.deltaTime; // Moves forward each frame by speed (frame-rate independent).

        timer += Time.deltaTime; // Adds time since last frame to the lifetime timer.
        if (timer >= lifetime) // If the bullet has existed longer than its allowed lifetime...
        {
            pool.Release(this); // Returns the bullet to the pool instead of destroying it (performance-friendly).
        }
    }

    private void OnDisable() // Unity event called when the GameObject becomes disabled (e.g., when returned to pool).
    {
        // Extra safety resets when returning to pool // Comment: ensures state is clean even if disabled unexpectedly.
        timer = 0f; // Resets timer so it starts fresh next time.
        if (rb != null) // Only reset physics if using Rigidbody.
        {
            rb.linearVelocity = Vector3.zero; // Ensures no leftover velocity while in pool.
            rb.angularVelocity = Vector3.zero; // Ensures no leftover rotation while in pool.
        }
    }
}
