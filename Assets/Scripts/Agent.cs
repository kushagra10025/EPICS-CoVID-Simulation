using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CSimHelper;

public enum InfectionState
{
    Susceptible,
    Infected,
    Removed,
    Exposed,
    Death
}

public class Agent : MonoBehaviour
{
    [Header("Movement")] [EditorOnly] public LayerMask AgentMask;
    [EditorOnly] public SpriteRenderer Sprite;
    [EditorOnly] public Rigidbody2D Rigidbody;

    // Random walk
    [Range(0, 1)] public float Speed; // m/s
    private Vector2 Direction; // random direction


    [Header("SIR")]
    // Probability of transmitting the infection to a neighbour, per second
    [Range(0, 1)]
    public float InfectionProbability; // per second

    [Range(0, 10)] public float InfectionRadius; // metres

    [ReadOnly] public InfectionState State = InfectionState.Susceptible;

    [Range(0f, 10f)] public float InfectionDuration = 1f; // Seconds
    private float InfectionTimer = 0f;
    
    [Header("Graphics")] public Color ColorS;
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

        // Infection evolution
        // if (State != InfectionState.Infected)
        //     return;
        
        switch (State)
        {
            case InfectionState.Infected:
                Infection();
                break;
        }
    }

    private void Infection()
    {
        // Infects other agents
        float pf = 1f - Mathf.Pow(1f - InfectionProbability, Time.deltaTime);
        // root(x, n) = pow (x, 1/n)
        // root(x, fps) = pow (x, 1/fps) , but fps = 1/deltatime!

        var agents = NearbyAgents()
            .Where(agent => agent.State == InfectionState.Susceptible)
            .Where(_ => Random.Range(0f, 1f) <= pf);

        foreach (Agent agent in agents)
            agent.Infect();

        // Evolves infection
        InfectionTimer -= Time.deltaTime;
        if (InfectionTimer <= 0)
        {
            // After Infection Get Exposed
            Remove();
        }
    }

    [Button(Editor = false)]
    public bool Infect()
    {
        // Can only infect susceptible agents
        if (State != InfectionState.Susceptible)
            return false;

        State = InfectionState.Infected;
        InfectionTimer = InfectionDuration;
        Sprite.color = ColorI;
        SpriteRadius.color = ColorI.xA(0.125f);
        //SpriteRadius.transform.localScale = Vector3.one * InfectionRadius * 10f;
        SpriteRadius.transform.localScale = Vector3.one * InfectionRadius * 10f /
                                            transform.lossyScale.x;
        SpriteRadius.enabled = true;
        return true;
    }

    public void Remove()
    {
        // SIR model
        State = InfectionState.Removed;
        Sprite.color = ColorR;

        // SIS model
        //State = InfectionState.Susceptible;
        //Sprite.color = ColorS;

        SpriteRadius.enabled = false;
    }

    // Retrieves the nearby agents
    public IEnumerable<Agent> NearbyAgents()
    {
        return NearbyAgents(InfectionRadius);
    }

    public IEnumerable<Agent> NearbyAgents(float radius)
    {
        int n = Physics2D.OverlapCircleNonAlloc(transform.position, radius, Colliders, AgentMask);
        return Colliders
            .Take(n)
            .Select(collider => collider.GetComponent<Agent>());
    }

    private Collider2D[] Colliders = new Collider2D[10]; // max people can be infected


    private void OnDrawGizmos()
    {
        if (State == InfectionState.Infected)
            DebugDraw.Circle(transform.position, InfectionRadius, Color.red);
    }
}