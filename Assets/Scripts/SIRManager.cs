using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CSimHelper.Data;
using CSimHelper;

public class SIRManager : MonoBehaviour
{

    [Header("Simulation")]
    // Number of agents travelling from a SIR region to another one per second
    [Range(0,100)]
    public int AgentsTravellingPerSecond;

    private SIR[] SIRs;

    [Header("Stats")]
    [StackedPlot(LabelX = "Time", LabelY = "Agents",
        Labels = new string[] { "Susceptible", "Infected", "Removed" },
        GridX = 60 * 5, // fps
        GridY = 50
        )]
    public StackedData Plot = new StackedData();


    private bool Running = false;

    

    [Button(Editor=false)]
    void StartSimulation ()
    {
        SIRs = FindObjectsOfType<SIR>();
        foreach (SIR sir in SIRs)
            sir.StartSimulation();


        StartCoroutine(Travel());

        Running = true;
        
    }

    // Update is called once per frame
    IEnumerator Travel()
    {
        while (true)
        {
            for (int i = 0; i < AgentsTravellingPerSecond; i ++)
            {
                // Finds two random neighbours
                int a = Random.Range(0, SIRs.Length);
                int b = Random.Range(0, SIRs.Length);

                // Finds two random agents to swap
                int ai = Random.Range(0, SIRs[a].Agents.Count);
                Agent agent_a = SIRs[a].Agents[ai];


                // Debug draw
                DebugDraw.Arrow
                (
                    agent_a.transform.position,
                    SIRs[b].transform.position,
                    Color.blue.xA(0.5f), 0.5f
                );

                // Moves the agent
                SIRs[a].Agents.Remove(agent_a);
                SIRs[b].Agents.Add(agent_a);

                agent_a.transform.parent = SIRs[b].transform;
                agent_a.transform.position = SIRs[b].transform.position;
            }

            yield return new WaitForSeconds(1f);
        }
    }


    void Update()
    {
        if (!Running)
            return;

        var agents = SIRs
            .Select(sir => sir.Agents)
            .SelectMany(x => x); // Flattens the list

        Plot.Add
            (
                agents.Count(agent => agent.State == InfectionState.Infected),
                agents.Count(agent => agent.State == InfectionState.Susceptible),
                agents.Count(agent => agent.State == InfectionState.Removed)
            );
    }
}
