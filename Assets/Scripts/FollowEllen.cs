using UnityEngine;
using UnityEngine.AI;

public class FollowEllen : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private Animator anim;
    private NavMeshAgent agent;
    private bool destinationReached;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        destinationReached = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        agent.SetDestination(player.transform.position);

        bool isFollowing = agent.hasPath && agent.remainingDistance > agent.stoppingDistance;

        anim.SetBool("Running", isFollowing);
    }
}
