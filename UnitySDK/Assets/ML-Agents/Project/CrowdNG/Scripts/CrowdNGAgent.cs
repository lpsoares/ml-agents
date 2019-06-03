using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MLAgents;

public class CrowdNGAgent : Agent
{
    // Depending on this value, the number of agents will be different
    int configuration;
    // Brain to use with 1 agent per color
    public Brain noCrowdBrain;
    // Brain to use with 1 to 4 agents per color
    public Brain fewCrowdBrain;
    // Brain to use with 5 to 8 agents per color
    public Brain halfFullCrowdBrain;
    // Brain to use with 9 to 12 agents per color
    public Brain fullCrowdBrain;
    public GameObject agentPrefab;
    public Transform Target;

    CrowdNGAcademy academy;
    bool iAmClone = false;


    public bool isBlue;
    public GameObject spawnArea;
    Bounds spawnAreaBounds;
    Rigidbody rBody;
    public override void InitializeAgent () {
        academy = FindObjectOfType<CrowdNGAcademy>();
        configuration = Random.Range(0, 5);

        rBody = GetComponent<Rigidbody>();
        spawnAreaBounds = spawnArea.GetComponent<Collider>().bounds;

        spawnArea.SetActive(false);
    }

    public override void AgentReset()
    {
        {
            // If the Agent fell, zero its momentum
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            // float z = isBlue ? 0.0f : 33.0f;
            // this.transform.localPosition = new Vector3( 0, 0, z);

            this.transform.localPosition = GetRandomSpawnPos();

            configuration = Random.Range(0, 5);

            if (iAmClone)
            {
                Done();
            }
        }

    }

    public override void CollectObservations()
    {
        // Agent position
        AddVectorObs(this.transform.localPosition);

        // Agent velocity
        AddVectorObs(rBody.velocity.x);
        AddVectorObs(rBody.velocity.z);

        // https://docs.unity3d.com/Manual/DirectionDistanceFromOneObjectToAnother.html
        // Agent heading
        // Gets a vector that points from the player's position to the target's.
        var heading = Target.localPosition - this.transform.localPosition;
        var distance = heading.magnitude;
        var direction = heading / distance; // This is now the normalized direction.
        AddVectorObs(direction);
    }

    public float speed = 10;
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];
        rBody.AddForce(controlSignal * speed);

        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.localPosition,
                                                Target.localPosition);

        // Reached target
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
            Done();
        } else
        {
            SetReward(0.001f);
        }

    }

    /// <summary>
    /// Gets a random spawn position in the spawningArea.
    /// </summary>
    /// <returns>The random spawn position.</returns>
    public Vector3 GetRandomSpawnPos()
    {
        Vector3 randomSpawnPos = Vector3.zero;
        float randomPosX = Random.Range(-spawnAreaBounds.extents.x,
                                        spawnAreaBounds.extents.x);
        float randomPosZ = Random.Range(-spawnAreaBounds.extents.z,
                                        spawnAreaBounds.extents.z);

        randomSpawnPos = spawnArea.transform.localPosition +
                                  new Vector3(randomPosX, 0.5f, randomPosZ);
        return randomSpawnPos;
    }

    void OnCollisionEnter(Collision col) {

        float vel = 0.01f + (col.relativeVelocity.magnitude* col.relativeVelocity.magnitude);

        if (col.gameObject.CompareTag("blueAgent"))
        {
            if ( isBlue )
            {
                AddReward(-0.001f);
            } else
            {
                AddReward(-0.1f);
            }

        }

        if (col.gameObject.CompareTag("redAgent"))
        {
            if ( ! isBlue )
            {
                AddReward(-0.001f);
            } else
            {
                AddReward(-0.1f);
            }

        }

        if (col.gameObject.CompareTag("wall"))
        {
            AddReward(-0.001f);
        }

        if (col.gameObject.CompareTag("blueGoal"))
        {
             if ( isBlue )
            {
                AddReward(1.0f);
                Done();
            } else
            {
                AddReward(-0.1f);
            }
           
        }

        if (col.gameObject.CompareTag("redGoal"))
        {
            if ( ! isBlue )
            {
                AddReward(1.0f);
                Done();
            } else
            {
                AddReward(-0.1f);
            }

        }

    }

    void OnCollisionStay(Collision col)
    {
        AddReward(-0.001f);
    }

    private void FixedUpdate()
    {
        if (configuration != -1)
        {
            ConfigureAgent(configuration);
            configuration = -1;
        }
    }

    /// <summary>
    /// Configures the agent. Given an integer config, the wall will have
    /// different height and a different brain will be assigned to the agent.
    /// </summary>
    /// <param name="config">Config. 
    /// If 0 : No crowd and noCrowdBrain.
    /// If 1:  Few agents and fewCrowdBrain.
    /// If 2:  Many agents and halfFullCrowdBrain.
    /// if 3 : A lot of agents and fullCrowdBrain. </param>
    void ConfigureAgent(int config)
    {
        if (config == 0)
        {
            print("No crowd. Num agent " + academy.resetParameters["number_of_agents"] );
            GiveBrain(noCrowdBrain);
        }
        else if (config == 1)
        {
            print("Few crowd. Num agent " + academy.resetParameters["few_number_of_agents"] );
            GiveBrain(fewCrowdBrain);
            CreateAgent(agentPrefab, fewCrowdBrain, GetRandomSpawnPos(), Quaternion.identity);
        }
        else if (config == 2)
        {
            print("Half full crowd. Num agent " + academy.resetParameters["half_full_number_of_agents"] );
            GiveBrain(halfFullCrowdBrain);
            // CreateAgent(agentPrefab, halfFullCrowdBrain, GetRandomSpawnPos(), Quaternion.identity);
        }
        else
        {
            print("Full crowd. Num agent " + academy.resetParameters["full_number_of_agents"] );
            GiveBrain(fullCrowdBrain);
            // CreateAgent(agentPrefab, fullCrowdBrain, GetRandomSpawnPos(), Quaternion.identity);
        }
    }

    private void CreateAgent(GameObject agentPrefab, Brain brain, Vector3 position, Quaternion orientation)
    {
        if (iAmClone)
        {
            return;
        }

        GameObject AgentObj = Instantiate(agentPrefab, position, orientation);
        AgentObj.transform.SetParent(this.transform.parent);
        CrowdNGAgent agent = AgentObj.GetComponent<CrowdNGAgent>();
        agent.iAmClone = true;
        agent.GiveBrain(brain);
        agent.AgentReset();
    }

    public override void AgentOnDone()
    {
        if (iAmClone)
        {
            Destroy(gameObject);
        }
    }
}
