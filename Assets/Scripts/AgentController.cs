using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Integrations.Match3;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using UnityEditor;



public class AgentController: Agent
{
    [SerializeField] Transform _target;
    [SerializeField] Transform _obstacle;
    [SerializeField] float _moveSpeed = 3.0f;
    int _pelletsCollected = 0;
    int _overallScore = 0;
    readonly string _logMessage1 = "Pellets collected: ";
    readonly string _logMessage2 = "Overall Score: ";

    private void Update()
    {
        Debug.Log(_logMessage1 + _pelletsCollected);
        Debug.Log(_logMessage2 + _overallScore);
    }

    public override void OnEpisodeBegin()
    {
        float randomizerAgentX = Random.Range(-4.44f, 4.44f);
        float randomizerAgentZ = Random.Range(-1.7f, 1.7f);
        // Starting point
        transform.localPosition = new Vector3(randomizerAgentX, 0.25f, randomizerAgentZ);

        // Pellet random location
        float randomizerX = Random.Range(-4.5f, 4.5f);
        float randomizerZ = Random.Range(-2f,2f);
        _target.localPosition = new Vector3(randomizerX, 0.25f, randomizerZ);

        // Barrier position
        _obstacle.localPosition = new Vector3(1.5f, 0.5f, 2.1f);

    }
    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent position
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.y);

        // Target position
        sensor.AddObservation(_target.localPosition.x);
        sensor.AddObservation(_target.localPosition.y);

        // Distance between the agent and the target
        sensor.AddObservation(Vector3.Distance(_target.localPosition, transform.localPosition));

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var actionTaken = actions.ContinuousActions;

        float _actionSpeed = actionTaken[0];
        float _actionSteering = actionTaken[1];
        transform.Translate(_actionSpeed * _moveSpeed * Time.fixedDeltaTime * Vector3.forward);
        transform.Rotate(Vector3.up, _actionSteering * 225f * Time.fixedDeltaTime);
        AddReward(-0.01f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> actions = actionsOut.ContinuousActions;
        actions[0] = 0; // Vertical
        actions[1] = 0; // Horizontal

        if (Input.GetKey(KeyCode.W))
        {
            actions[0] = 1;
        }
            
        if (Input.GetKey(KeyCode.A))
        {
            actions[1] = -1;
        }
            
        if (Input.GetKey(KeyCode.S))
        {
            actions[0] = -1;
        }
            
        if (Input.GetKey(KeyCode.D))
        {
            actions[1] = +1;
        }
              
    }
    public void UpdateScore(int _receivedReward)
    {
        _overallScore += _receivedReward;

    }

    private void OnTriggerEnter(Collider other)
    {
        int _reward;
        if (other.gameObject.CompareTag("Pellet"))
        {
            _reward = 2;
            AddReward(_reward);
            _pelletsCollected ++;
            UpdateScore(_reward);
            EndEpisode();
        }
        if(other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Barrier"))
        {
            _reward = -1;
            AddReward(_reward);
            UpdateScore(_reward);
            EndEpisode();
        }

    }
}
