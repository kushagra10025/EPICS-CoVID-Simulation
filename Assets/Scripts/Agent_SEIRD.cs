using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CSimHelper;

public class Agent_SEIRD : MonoBehaviour
{
    [Header("Movement")] 
    [EditorOnly] public LayerMask AgentMask;
    [EditorOnly] public SpriteRenderer Sprite;
    [EditorOnly] public Rigidbody2D Rigidbody;

    // Random walk
    [Range(0, 1)] public float Speed; // m/s
    private Vector2 Direction; // random direction


    [Header("SEIRD")]
    // Probability of transmitting the infection to a neighbour, per second
    [ReadOnly] public InfectionState State = InfectionState.Susceptible;
    [Range(0, 1)]
    public float InfectionProbability; // per second
    [Range(0, 10)] public float InfectionRadius; // metres
    [Range(0f, 10f)] public float InfectionDuration = 1f; // Seconds
    
    private float InfectionTimer = 0f;

    [Header("Graphics")]
    public Color ColorS;
    public Color ColorI;
    public Color ColorR;
    public Color ColorE;
    public Color ColorD;
    public SpriteRenderer SpriteRadius;


    IEnumerator Start()
    {
        // Initial colour
        if (State == InfectionState.Susceptible)
        {
            Sprite.color = ColorS;
            SpriteRadius.enabled = false;
        }

        // Random starting offset
        yield return new WaitForSeconds(Random.Range(0f, 2f));

        // Changes the direction every three seconds
        while (true)
        {
            Direction = Random.insideUnitCircle.normalized;
            yield return new WaitForSeconds(1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Movement
        Rigidbody.MovePosition(Rigidbody.position + Direction * (Speed * Time.deltaTime));

        float pf = 1f - Mathf.Pow(1f - InfectionProbability, Time.deltaTime);
        var agents = NearbyAgents()
            .Where(agent => (agent.State == InfectionState.Susceptible || agent.State == InfectionState.Exposed))
            .Where(_ => Random.Range(0f, 1f) <= pf);
        
        switch (State)
        {
            case InfectionState.Susceptible:
                // Expose other agents
                foreach (var agent in agents)
                    ChangeState(InfectionState.Exposed);

                // // Evolves expose
                // InfectionTimer -= Time.deltaTime;
                // if (InfectionTimer <= 0)
                // {
                //     // Remove();
                //     ChangeState(InfectionState.Infected);
                // }
                break;
            case InfectionState.Exposed:
                // Infects other agents
                foreach (var agent in agents)
                    ChangeState(InfectionState.Infected);

                // Evolves infection
                InfectionTimer -= Time.deltaTime;
                if (InfectionTimer <= 0)
                {
                    // After Infection Get Exposed
                    // Remove();
                    ChangeState(InfectionState.Infected);
                }
                break;
            case InfectionState.Infected:
                foreach (var agent in agents)
                    ChangeState(InfectionState.Removed);
                
                InfectionTimer -= Time.deltaTime;
                if (InfectionTimer <= 0)
                {
                    // After Infection Get Exposed
                    // Remove();
                    float randProb = Random.Range(0f, 1f);
                    if (randProb > 0.5f)
                    {
                        ChangeState(InfectionState.Removed);
                    }
                    else
                    {
                        ChangeState(InfectionState.Death);
                    }
                }
                break;
            case InfectionState.Removed:
                break;
            case InfectionState.Death:
                break;
        }
    }

    public void ChangeState(InfectionState newState)
    {
        switch (newState)
        {
            case InfectionState.Exposed:
                Expose();
                break;
            case InfectionState.Infected:
                Infect();
                break;
            case InfectionState.Death:
                Death();
                break;
            case InfectionState.Removed:
            default: Remove();
                break;
        }
    }

    private void Death()
    {
        State = InfectionState.Death;
        Sprite.color = ColorD;
        Speed = 0.0f;
        SpriteRadius.enabled = false;
    }

    private void Infect()
    {
        // Can only infect exposed agents
        if (State != InfectionState.Exposed) return;
        State = InfectionState.Infected;

        InfectionTimer = InfectionDuration;
        Sprite.color = ColorI;
        SpriteRadius.color = ColorI.xA(0.125f);
        SpriteRadius.transform.localScale = Vector3.one * (InfectionRadius * 10f) /
                                            transform.lossyScale.x;
        SpriteRadius.enabled = true;
    }

    private void Expose()
    {
        // Can only infect susecptible agents
        if (State != InfectionState.Susceptible) return;
        State = InfectionState.Exposed;

        InfectionTimer = InfectionDuration;
        Sprite.color = ColorE;
        SpriteRadius.color = ColorE.xA(0.125f);
        SpriteRadius.transform.localScale = Vector3.one * (InfectionRadius * 10f) /
                                            transform.lossyScale.x;
        SpriteRadius.enabled = true;
    }

    public void Remove()
    {
        // SEIRD model
        State = InfectionState.Removed;
        Sprite.color = ColorR;

        // SIS model
        //State = InfectionState.Susceptible;
        //Sprite.color = ColorS;

        SpriteRadius.enabled = false;
    }

    // Retrieves the nearby agents
    public IEnumerable<Agent_SEIRD> NearbyAgents()
    {
        return NearbyAgents(InfectionRadius);
    }

    public IEnumerable<Agent_SEIRD> NearbyAgents(float radius)
    {
        int n = Physics2D.OverlapCircleNonAlloc(transform.position, radius, Colliders, AgentMask);
        return Colliders
            .Take(n)
            .Select(collider => collider.GetComponent<Agent_SEIRD>());
    }

    private Collider2D[] Colliders = new Collider2D[10]; // max people can be infected


    private void OnDrawGizmos()
    {
        if (State == InfectionState.Infected)
            DebugDraw.Circle(transform.position, InfectionRadius, Color.red);
        if (State == InfectionState.Exposed)
            DebugDraw.Circle(transform.position, InfectionRadius, Color.blue);
    }
}