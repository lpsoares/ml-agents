using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class FireEscapeAgent3 : Agent
{
    public GameObject ground;
    public GameObject area;
    public GameObject orangeGoal;
    public GameObject redBlock;
    public bool useVectorObs;
    RayPerception rayPer;
    Rigidbody shortBlockRB;
    Rigidbody agentRB;
    Material groundMaterial;
    Renderer groundRenderer;
    FireEscapeAcademy academy;
    float touch;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        academy = FindObjectOfType<FireEscapeAcademy>();
        rayPer = GetComponent<RayPerception>();
        agentRB = GetComponent<Rigidbody>();
        groundRenderer = ground.GetComponent<Renderer>();
        groundMaterial = groundRenderer.material;
        touch = 0.0f;
    }

    public override void CollectObservations()
    {
        if (useVectorObs)
        {
            float rayDistance = 10f;
            float[] rayAngles = { 0f, 50f, 90f, 130f, 180f };
            string[] detectableObjects = { "orangeGoal", "redBlock", "wall" };
            AddVectorObs(GetStepCount() / (float)agentParameters.maxStep); // sensação de tempo passando
            AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f)); // sensação de visão em fumaça
            AddVectorObs(agentRB.velocity);  // sensação de acelaração
            AddVectorObs(touch); // sensação de toque

        }
    }

    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        groundRenderer.material = mat;
        yield return new WaitForSeconds(time);
        groundRenderer.material = groundMaterial;
    }

    public void MoveAgent(float[] act)
    {

        Vector3 dirToGo = Vector3.zero;
        Vector3 rotateDir = Vector3.zero;

        if (brain.brainParameters.vectorActionSpaceType == SpaceType.continuous)
        {
            dirToGo = transform.forward * Mathf.Clamp(act[0], -1f, 1f);
            rotateDir = transform.up * Mathf.Clamp(act[1], -1f, 1f);
        }
        else
        {
            int action = Mathf.FloorToInt(act[0]);
            switch (action)
            {
                case 1:
                    dirToGo = transform.forward * 1f;
                    break;
                case 2:
                    dirToGo = transform.forward * -1f;
                    break;
                case 3:
                    rotateDir = transform.up * 1f;
                    break;
                case 4:
                    rotateDir = transform.up * -1f;
                    break;
            }
        }
        transform.Rotate(rotateDir, Time.deltaTime * 150f);
        agentRB.AddForce(dirToGo * academy.agentRunSpeed, ForceMode.VelocityChange);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        float temp_inst = 1.0f / (float)Math.Pow(Vector3.Distance(redBlock.transform.position, transform.position), 3);
        AddReward(-1f / agentParameters.maxStep);
        AddReward(-temp_inst);
        MoveAgent(vectorAction);
    }

    void OnCollisionEnter(Collision col)
    {
        touch = 1.0f;
        if (col.gameObject.CompareTag("orangeGoal"))
        {
            SetReward(1f);
            StartCoroutine(GoalScoredSwapGroundMaterial(academy.goalScoredMaterial, 0.5f));
            Done();
        }
    }
    void OnCollisionExit(Collision other)
    {
        touch = 0.0f;
    }

    public override void AgentReset()
    {
        float agentOffset = -14f;

        redBlock.transform.position =
            new Vector3(UnityEngine.Random.Range(-8f, 8f), 2f, -23f)
            + ground.transform.position;

        transform.position = new Vector3(UnityEngine.Random.Range(-7f, 7f),
                                         1f, agentOffset + UnityEngine.Random.Range(-3f, 3f))
            + ground.transform.position;

        transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
        agentRB.velocity *= 0f;

        orangeGoal.transform.position = new Vector3(0f, 0.5f, 10f) + area.transform.position;

        touch = 0.0f;

    }
}
