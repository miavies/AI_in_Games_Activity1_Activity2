using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class MonsterAIMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    [SerializeField] private Transform currentDestination;
    [SerializeField] private string state;
    [SerializeField] private bool wasChasing = false;
    [SerializeField] private bool isSearching = false;

    [Header("Patrol Points")]
    [SerializeField] private List<GameObject> patrolPoints;
    private int count;
    private bool destinationReached;

    [Header("Chasing")]
    [SerializeField] private GameObject target;
    private Transform targetDestination;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float fieldOfView = 45f;
    [SerializeField] private float rangeOfView = 15f;
    [SerializeField] private float attackRange = 6f;

    [Header("Timers")]
    //[SerializeField] private float suspicionTimer;
    //[SerializeField] private float confirmDetectionTimer;
    [SerializeField] private float searchDuration = 5f;
    private float searchTimer;
    [SerializeField] private float lostSightDuration = 2f;
    private float lostSightTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        
        count = 0;
        destinationReached = false;

        searchTimer = searchDuration;
        lostSightTimer = lostSightDuration;
        GoToDestination();

    }

    // Update is called once per frame
    void Update()
    {
        state = CheckState();

        switch (state) 
        {
            case "Patrol":
                Debug.Log("Patrolling");
                agent.speed = 2;
                agent.isStopped = false;
                Patrol();
                break;

            case "Search":
                Debug.Log("Searching");
                Search();
                break;

            case "Chase":
                Debug.Log("Chasing");
                Chase();
                break;
        }      
    }

    private string CheckState()
    {
        if (isSearching) return "Search";

        CheckIfEnteredRange();

        if (IsTargetVisible())
        {
            Debug.Log("Target Visible");
            LookAtTarget();
            return "Chase";
        }

        if (wasChasing)
        {
            lostSightTimer -= Time.deltaTime;

            if (lostSightTimer <= 0f)
            {
                lostSightTimer = lostSightDuration;
                return "Search";
            }
            else
            {
                return "Chase";
            }
        }

        return "Patrol";
    }

    private void Chase()
    {
        if (target == null) return;

        agent.speed = 6;

        if (Vector3.Distance(transform.position, target.transform.position) <= attackRange)
        {
            agent.isStopped = true;
            anim.SetBool("Attacking", true);
            Debug.Log("Attacking");
            wasChasing = false;
        }
        else
        {
            agent.isStopped = false;
            anim.SetBool("Attacking", false);

            targetDestination = target.transform;
            currentDestination = targetDestination;

            wasChasing = true;
            GoToDestination();
        }
    }

    private void CheckIfEnteredRange()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, rangeOfView, playerLayer);
        float minDist = Mathf.Infinity;
        target = null;

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                target = hit.gameObject;
            }
        }
    }

    private bool IsTargetVisible()
    {
        if (target == null) return false;

        Vector3 toTarget = target.transform.position - transform.position;
        float distToTarget = toTarget.magnitude;

        //Check if in range
        if (distToTarget > rangeOfView) return false;

        //Check if within FOV
        Vector3 dir = toTarget.normalized;
        float dot = Vector3.Dot(transform.forward, dir);
        float threshold = Mathf.Cos(fieldOfView * Mathf.Deg2Rad);

        if (dot < threshold) return false;

        //Check line of sight
        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, rangeOfView))
        {
            return hit.transform.CompareTag("Player");
        }

        return false;
    }

    void LookAtTarget()
    {
        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void Search()
    {
        if (!isSearching)
        {
            isSearching = true;
            searchTimer = searchDuration;

            agent.isStopped = true;
            anim.SetTrigger("Search");
        }

        searchTimer -= Time.deltaTime;

        CheckIfEnteredRange();

        if (IsTargetVisible())
        {
            isSearching = false;
            wasChasing = false;
            agent.isStopped = false;
        }
        else if (searchTimer <= 0)
        {
            isSearching = false;
            wasChasing = false;
            agent.isStopped = false;
        }
    }

    private void Patrol()
    {
        //checks distance to waypoint
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            destinationReached = true;
        }

        if (destinationReached)
        {
            if (count >= patrolPoints.Count)
            {
                count = 0;
            }
            else
            {
                count++;
            }

            currentDestination = patrolPoints[count].transform;
            GoToDestination();
            destinationReached = false;
        }
    }

    private void GoToDestination()
    {
        agent.SetDestination(currentDestination.position);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangeOfView);

        Gizmos.color = Color.purple;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (target == null) return;

        // Line to target
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, target.transform.position);

        //Forward range
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * rangeOfView);

        /// FOV cone lines
        Gizmos.color = Color.yellow;
        Quaternion leftRot = Quaternion.Euler(0, -fieldOfView, 0);
        Quaternion rightRot = Quaternion.Euler(0, fieldOfView, 0);

        Gizmos.DrawLine(transform.position, transform.position + leftRot * transform.forward * 10);
        Gizmos.DrawLine(transform.position, transform.position + rightRot * transform.forward * 10);
    }
}
