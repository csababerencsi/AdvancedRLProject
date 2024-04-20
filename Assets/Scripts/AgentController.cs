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
    [SerializeField] float _moveSpeed = 5.0f;
    int _pelletsCollected = 0;
    int _overallScore = 0;
    //private const float MAX_DISTANCE = 28.28427f;

    [SerializeField] TextMeshProUGUI _pelletText;
    [SerializeField] TextMeshProUGUI _overallText;
    public override void OnEpisodeBegin()
    {
        Vector3 upperLeftCorner = new(-4f, 0.25f, -1.7f);
        Vector3 lowerLeftCorner = new(-4f, 0.25f, 1.7f);
        Vector3 upperRightCorner = new(4f, 0.25f, 1.7f);
        Vector3 lowerRightCorner = new(4f, 0.25f, -1.7f);

        int _randomizedAgentPosition = 0;   //Random.Range(0, 4)
        var agentSpawnPoint = _randomizedAgentPosition switch
        {
            0 => upperLeftCorner,
            1 => lowerLeftCorner,
            2 => upperRightCorner,
            3 => lowerRightCorner,
            _ => new Vector3(0f, 0f, 0f),
        };

        // Starting point
        transform.localPosition = agentSpawnPoint;
        transform.rotation = Quaternion.identity;

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
        
        // Obstacle position
        sensor.AddObservation(_obstacle.localPosition.x);
        sensor.AddObservation(_obstacle.localPosition.y);

        // Distance between the agent and the target
        sensor.AddObservation(Vector3.Distance(_target.localPosition, transform.localPosition));

        // Distance between the agent and the obstacle
        sensor.AddObservation(Vector3.Distance(_obstacle.localPosition, transform.localPosition));
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
