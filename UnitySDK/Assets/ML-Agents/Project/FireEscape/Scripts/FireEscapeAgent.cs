﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class FireEscapeAgent : Agent
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
    int selection;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        academy = FindObjectOfType<FireEscapeAcademy>();
        rayPer = GetComponent<RayPerception>();
        agentRB = GetComponent<Rigidbody>();
        groundRenderer = ground.GetComponent<Renderer>();
        groundMaterial = groundRenderer.material;
    }

    public override void CollectObservations()
    {
        if (useVectorObs)
        {
            float rayDistance = 20f;
            float[] rayAngles = { 0f, 50f, 90f, 130f, 180f };
            string[] detectableObjects = { "orangeGoal", "redGoal", "orangeBlock", "redBlock", "wall" };   // AJUSTAR
            AddVectorObs(GetStepCount() / (float)agentParameters.maxStep);
            AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
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
        AddReward(-1f / (float)Math.Pow(Vector3.Distance(redBlock.transform.position, transform.position),2));
        MoveAgent(vectorAction);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("orangeGoal"))
        {
            SetReward(1f);
            StartCoroutine(GoalScoredSwapGroundMaterial(academy.goalScoredMaterial, 0.5f));
            Done();
        }
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
        
    }
}