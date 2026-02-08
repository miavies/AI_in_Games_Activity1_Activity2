using UnityEngine;

public class Door : MonoBehaviour
{
    public Vector3 doorUpOffset = new Vector3(0, 0, 0); 
    public float moveSpeed = 2f;

    private Vector3 doorUp;
    private Vector3 doorDown;

    private bool doorIsUp = false;
    [SerializeField] private bool playerInRange = false;
    private bool isMoving = false;

    void Start()
    {
        doorDown = transform.position;
        doorUp = doorDown + doorUpOffset;
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isMoving)
        {
            doorIsUp = !doorIsUp;
            StopAllCoroutines();
            StartCoroutine(MoveDoor(doorIsUp ? doorUp : doorDown));
        }
    }

    private System.Collections.IEnumerator MoveDoor(Vector3 target)
    {
        isMoving = true;

        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                target,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = target;
        isMoving = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
