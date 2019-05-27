using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MLAgents;

public class CrowdNGAgent : Agent
{
    public Transform Target;
    public bool isBlue;
     public GameObject spawnArea;
    Bounds spawnAreaBounds;
    Rigidbody rBody;
    public override void InitializeAgent () {
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
}
