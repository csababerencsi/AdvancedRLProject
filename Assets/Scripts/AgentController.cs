using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using TMPro;

public class AgentController: Agent
{
    [SerializeField] Transform _target;
    [SerializeField] Transform _obstacle;
    [SerializeField] float _moveSpeed = 3.0f;
    int _pelletsCollected = 0;
    int _overallScore = 0;
    //private const float MAX_DISTANCE = 28.28427f;

    [SerializeField] TextMeshProUGUI _pelletText;
    [SerializeField] TextMeshProUGUI _overallText;
    public override void OnEpisodeBegin()
    {
        Vector3 upperLeftCorner = new Vector3(-4.44f, 0.25f, -1.7f);
        Vector3 lowerLeftCorner = new Vector3(-4.44f, 0.25f, 1.7f);
        Vector3 upperRightCorner = new Vector3(4.44f, 0.25f, 1.7f);
        Vector3 lowerRightCorner = new Vector3(4.44f, 0.25f, -1.7f);

        int _randomizedAgentPosition = Random.Range(0, 0);
        Vector3 agentSpawnPoint;

        switch (_randomizedAgentPosition)
        {
            case 0:
                agentSpawnPoint = upperLeftCorner;
                break;
            case 1:
                agentSpawnPoint = lowerLeftCorner;
                break;
            case 2:
                agentSpawnPoint = upperRightCorner;
                break;
            case 3:
                agentSpawnPoint = lowerRightCorner;
                break;
            default:
                agentSpawnPoint = new Vector3(0f, 0f, 0f);
                break;
        }
        // Starting point
        transform.localPosition = agentSpawnPoint;

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
        //AddReward(-0.01f);
        /*
        float distance_scaled = Vector3.Distance(TargetTransform.localPosition, transform.localPosition) / MAX_DISTANCE;
        //Debug.Log(distance_scaled);

        AddReward(-distance_scaled / 10); // [0, 0.1]
        */
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

    private void OnCollisionEnter(Collision collision)
    {
        int _reward;
        if (collision.collider.CompareTag("Pellet"))
        {
            _reward = 2;
            AddReward(_reward);
            _pelletsCollected++;
            _overallScore += _reward;
            EndEpisode();
        }
        if(collision.collider.CompareTag("Wall") || collision.collider.CompareTag("Barrier"))
        {
            _reward = -1;
            AddReward(_reward);
            if (_overallScore > 0)
                _overallScore += _reward;
            EndEpisode();
        }

    }
    private void Update()
    {
        _pelletText.text = "Pellets collected: " + _pelletsCollected;
        _overallText.text = "Overall Score: " + _overallScore;
    }
}
