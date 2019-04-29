using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class CrowdAgent1 : Agent
{
    //public GameObject destiny;
    public float[] goals = new float[4];
    public float initialPos;
    RayPerception rayPer;
    Rigidbody shortBlockRB;
    Rigidbody agentRB;
    CrowdAcademy academy;    // Testar polimorfirmos aqui

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        academy = FindObjectOfType<CrowdAcademy>();
        rayPer = GetComponent<RayPerception>();
        agentRB = GetComponent<Rigidbody>();
    }

    public override void CollectObservations()
    {
        float rayDistance = 12f;
        //float[] rayAngles = { 0f, 50f, 90f, 130f, 180f };
        float[] rayAngles = new float[36];
        for (int i = 0; i < 36; i++) rayAngles[i] = 10.0f * i;
        string[] detectableObjects = { "agent", "blueGoal", "redGoal", "wall" };
        AddVectorObs(goals); // sensação de tempo passando
        AddVectorObs(GetStepCount() / (float)agentParameters.maxStep); // sensação de tempo passando
        AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f)); // sensação de visão
        AddVectorObs(agentRB.velocity);  // sensação de acelaração
        AddVectorObs(transform.localPosition);

        // Mostra os raios
        //List<float> raios = rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f);
        //String raiosS = "raio " + gameObject.name;
        //for (int i = 0; i < raios.Count; i++) {
        //    if ((i % 6) == 0) raiosS += " | ";
        //    raiosS += " " + raios[i];
        //}
        //print(raiosS);

    }

    public void MoveAgent(float[] act)
    {
        Vector3 dirToGo = Vector3.zero;
        Vector3 rotateDir = Vector3.zero;
        if (brain.brainParameters.vectorActionSpaceType == SpaceType.continuous) {
            dirToGo = transform.forward * Mathf.Clamp(act[0], -1f, 1f);
            rotateDir = transform.up * Mathf.Clamp(act[1], -1f, 1f);
        } else // NESSE MOMENTO ENTRANDO AQUI !!!
        {
            int action = Mathf.FloorToInt(act[0]);
            switch (action) {
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

    public override void AgentAction(float[] vectorAction, string textAction) {
        AddReward(-1.0f / agentParameters.maxStep);
        MoveAgent(vectorAction);
    }
    
    void OnCollisionEnter(Collision col) {

        float vel = 0.01f + (col.relativeVelocity.magnitude* col.relativeVelocity.magnitude);
        //float vel = col.relativeVelocity.magnitude;

        if (col.gameObject.CompareTag("agent"))
        {
            //print("agent");
            //AddReward(-0.01f);  // bateu
            AddReward(-0.001f * vel);  // bateu
        }

        if (col.gameObject.CompareTag("wall"))
        {
            //AddReward(-0.001f);  // bateu
            AddReward(-0.001f * vel);  // bateu
        }

        if (col.gameObject.CompareTag("blueGoal"))
        {
            //print("blue");
            if(goals[1]>0.5f)
            {
                //SetReward(40.0f);
                AddReward(200.0f);
                Done();
            } else
            {
                //AddReward(-100f);  // bateu
                AddReward(-0.001f);  // bateu
                //SetReward(0.0f);
            }
        }

        if (col.gameObject.CompareTag("redGoal"))
        {
            //print("red");
            if (goals[2] > 0.5f)
            {
                //SetReward(40.0f);
                AddReward(200.0f);
                Done();
            } else
            {
                //AddReward(-100f);  // bateu
                AddReward(-0.001f);  // bateu
                //SetReward(0.0f);
                //Done();
            }

        }


    }

    void OnCollisionStay(Collision col)
    {
        AddReward(-0.001f);  // esta batendo
    }

    public override void AgentReset() {

        // Posição do Personagem
        transform.localPosition = new Vector3(UnityEngine.Random.Range(-7f, 7f),
                                         1f, initialPos + UnityEngine.Random.Range(-5f, 5f));
        transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
        agentRB.velocity *= 0f;

    }

}
