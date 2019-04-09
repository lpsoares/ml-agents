using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class FireEscapeAgent5 : Agent
{
    public GameObject ground;
    public GameObject area;
    public GameObject exit;
    public GameObject fire;
    public GameObject wall1;
    public GameObject wall2;
    public GameObject wall3;
    public GameObject wall4;
    public bool useVectorObs;
    RayPerception rayPer;
    Rigidbody shortBlockRB;
    Rigidbody agentRB;
    Material groundMaterial;
    Renderer groundRenderer;
    FireEscapeAcademy academy;
    float touch;
    float heat;

    float air;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        academy = FindObjectOfType<FireEscapeAcademy>();
        rayPer = GetComponent<RayPerception>();
        agentRB = GetComponent<Rigidbody>();
        groundRenderer = ground.GetComponent<Renderer>();
        groundMaterial = groundRenderer.material;
        touch = 0.0f;
        heat = 0.0f;

        air = 0.0f;
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
            AddVectorObs(1.0f / (float)Math.Pow(Vector3.Distance(fire.transform.position, transform.position), 3));  // sensação de temperatura

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
        float temp_inst = 0.05f / (float)Math.Pow(Vector3.Distance(fire.transform.position, transform.position), 3);
        float aging = 0.2f / agentParameters.maxStep;
        AddReward(-aging);
        AddReward(-temp_inst);
        heat += temp_inst;
        air += aging;
        if (heat > 0.8f)  // morreu queimado
        {
            AddReward(-0.8f);
            StartCoroutine(GoalScoredSwapGroundMaterial(academy.failMaterial, 0.5f));
            print("morreu queimado");
            Done();
        }
        if (air > 0.19f)  // morreu sufocado
        {
            AddReward(-0.8f);
            StartCoroutine(GoalScoredSwapGroundMaterial(academy.failMaterial, 0.5f));
            print("morreu sufocado");
            Done();
        }
        MoveAgent(vectorAction);
    }
    
    void OnCollisionEnter(Collision col)
    {
        touch = 1.0f;
        if (col.gameObject.CompareTag("orangeGoal"))
        {
            SetReward(1.0f);
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

        if(IsMaxStepReached())
        {
            StartCoroutine(GoalScoredSwapGroundMaterial(academy.failMaterial, 0.5f));
        }

        print("air = " + air + "   heat = " + heat);

        touch = 0.0f;
        heat = 0.0f;
        air = 0.0f;

        // Posição do Fogo
        fire.transform.position =
            new Vector3(UnityEngine.Random.Range(-8f, 8f), 2f, -23f)
            + ground.transform.position;

        // Posição do Personagem
        transform.position = new Vector3(UnityEngine.Random.Range(-7f, 7f),
                                         1f, -17.0f + UnityEngine.Random.Range(-3f, 3f))
                                         + ground.transform.position;
        transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
        agentRB.velocity *= 0f;

        // Posição da saída
        exit.transform.position =
                    new Vector3(UnityEngine.Random.Range(-6f, 6f), 0.5f, 10f)
                    + area.transform.position;

        // Posição das paredes
        if (UnityEngine.Random.Range(0.0f,1.0f) > 0.5)
        {
            wall1.transform.position = new Vector3(2.5f, 1.5f, -4.0f + UnityEngine.Random.Range(-4f, 4f)) + area.transform.position;
            wall2.transform.position = new Vector3(-2.5f, 1.5f, -18.0f + UnityEngine.Random.Range(-4f, 4f)) + area.transform.position;
        } else {
            wall1.transform.position = new Vector3(2.5f, 1.5f, -18.0f + UnityEngine.Random.Range(-4f, 4f)) + area.transform.position;
            wall2.transform.position = new Vector3(-2.5f, 1.5f, -4.0f + UnityEngine.Random.Range(-4f, 4f)) + area.transform.position;
        }
        float pos = exit.transform.position.x - area.transform.position.x;
        wall3.transform.position =  // parede ao lado da saida
                    new Vector3((pos>0.0f?pos-3.5f:pos+3.5f), 1.5f, 9.5f)
                    + area.transform.position;

        // cruz no meio
        if( System.Math.Abs(wall2.transform.position.z-wall1.transform.position.z)<10 )
        {
            wall4.transform.position = new Vector3(UnityEngine.Random.Range(-2f, 2f), -50f, -11.0f) + area.transform.position;
        } else
        {
            wall4.transform.position = new Vector3(UnityEngine.Random.Range(-2f, 2f), 1.5f, -11.0f) + area.transform.position;
        }
        

    }

}
