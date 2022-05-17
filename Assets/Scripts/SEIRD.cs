using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSimHelper.Data;
using System.Linq;

public class SEIRD : MonoBehaviour
{
    [Header("Agents")]
    public Agent_SEIRD AgentPrefab;
    public LayerMask AgentMask;
    public bool AgentCollisions = true; // if false, agents don't collide with eachother
    
    [Range(0,2000)]
    public int AgentCounts;
    [Range(0, 10)]
    public float AgentScale = 1f;
    [HideInInspector]
    public List<Agent_SEIRD> Agents = new List<Agent_SEIRD>();

    [Header("Environment")]
    public Transform WallPrefab;
    public Vector2 EnvironmentSize;


    [Header("Infection")]
    [Range(0,100)]
    public int InfectedAgents;
    [Space]
    
    [MinMaxSlider(0f, 1f)]
    public Vector2 InfectionProbability; // probability per second

    [MinMaxSlider(0f, 5f)]
    public Vector2 InfectionRadius; // in metres

    [MinMaxSlider(0f, 30f)]
    public Vector2 InfectionDuration; // in seconds


    [Header("Stats")]
    [StackedPlot(LabelX = "Time",LabelY = "Agents",
        Labels = new string[] { "Susceptible", "Infected", "Removed" },
        GridX = 60*5, // fps
        GridY = 10
        )]
    public StackedData Plot = new StackedData();


    private bool Running = false;

    


    [Button(Editor=false)]
    public void StartSimulation ()
    {
        if (!AgentCollisions)
        {
            int layerNumber = (int)Mathf.Log(AgentMask, 2);
            Physics2D.IgnoreLayerCollision(layerNumber, layerNumber);
        }


        // Creates the walls
        Transform wallNorth = Instantiate
        (
            WallPrefab,
            transform.position + new Vector3(0f, EnvironmentSize.y / 2f),
            Quaternion.identity,
            transform
        );
        wallNorth.transform.localScale = new Vector3(EnvironmentSize.y, 0.25f, 1f);

        Transform wallSouth = Instantiate
        (
            WallPrefab,
            transform.position + new Vector3(0f, -EnvironmentSize.x / 2f),
            Quaternion.identity,
            transform
        );
        wallSouth.transform.localScale = new Vector3(EnvironmentSize.x, 0.25f, 1f);

        Transform wallWest = Instantiate
        (
            WallPrefab,
            transform.position + new Vector3(-EnvironmentSize.x / 2f, 0f),
            Quaternion.identity,
            transform
        );
        wallWest.transform.localScale = new Vector3(0.25f, EnvironmentSize.y, 1f);

        Transform wallEast = Instantiate
        (
            WallPrefab,
            transform.position + new Vector3(+EnvironmentSize.x / 2f, 0f),
            Quaternion.identity,
            transform
        );
        wallEast.transform.localScale = new Vector3(0.25f, EnvironmentSize.y, 1f);


        // Instantiates the agents
        Agents.Clear();
        for (int i = 0; i < AgentCounts; i ++)
        {
            Vector2 position = (Vector2) transform.position + new Vector2
            (
                Random.Range(-EnvironmentSize.x / 2f, +EnvironmentSize.x / 2f),
                Random.Range(-EnvironmentSize.y / 2f, +EnvironmentSize.y / 2f)
            );
            Agent_SEIRD agent = Instantiate(AgentPrefab, position, Quaternion.identity, transform);
            agent.transform.localScale = Vector3.one * AgentScale; // graphics
            Agents.Add(agent);

            // Parameters
            agent.InfectionProbability = Random.Range(InfectionProbability.x, InfectionProbability.y);
            agent.InfectionRadius = Random.Range(InfectionRadius.x, InfectionRadius.y);
            agent.InfectionDuration = Random.Range(InfectionDuration.x, InfectionDuration.y);
        }


        // Infects the agents
        for (int i = 0; i < InfectedAgents; i ++)
        {
            Agent_SEIRD agent = Agents[Random.Range(0, Agents.Count)];
            // agent.Infect();
            agent.ChangeState(InfectionState.Exposed);
        }


        Running = true;
    }

    void Update()
    {
        if (Running)
            Plot.Add
            (
                Agents.Count(agent => agent.State == InfectionState.Infected),
                Agents.Count(agent => agent.State == InfectionState.Susceptible),
                Agents.Count(agent => agent.State == InfectionState.Removed)
            );
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(transform.position, EnvironmentSize);
    }
}
