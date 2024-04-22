using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using TMPro;

public class AgentController: Agent
{
    [SerializeField] Transform _target; //GameObject?
    [SerializeField] Transform _obstacle; //GameObject?
    [SerializeField] float _moveSpeed = 5.0f;
    int _pelletsCollected = 0;
    int _overallScore = 0;
    int _episodeCount = 0;
    Vector3 _agentSpawnPoint = new(0f, 0f, 0f);
    //private const float MAX_DISTANCE = 28.28427f;

    [SerializeField] TextMeshProUGUI _pelletText;
    [SerializeField] TextMeshProUGUI _overallText;
    public override void OnEpisodeBegin() //count number of episodes
    {

        Vector3 upperLeftCorner = new(-4f, 0.25f, 1.7f);
        var _upperLeftCornerRotation = Quaternion.Euler(0f, 135f, 0f);

        Vector3 lowerLeftCorner = new(-4f, 0.25f, -1.7f);
        var _lowerLeftCornerRotation = Quaternion.Euler(0f, 45f, 0f);

        Vector3 upperRightCorner = new(4f, 0.25f, 1.7f);
        var _upperRightCornerRotation = Quaternion.Euler(0f, -135f, 0f);

        Vector3 lowerRightCorner = new(4f, 0.25f, -1.7f);
        var _lowerRightCornerRotation = Quaternion.Euler(0f, -45f, 0f);


        if (_episodeCount < 1000) {
            _agentSpawnPoint = lowerLeftCorner;
            transform.rotation = _lowerLeftCornerRotation;
        }
        else if (_episodeCount < 2000) {
            _agentSpawnPoint = lowerRightCorner;
            transform.rotation = _lowerRightCornerRotation;
        }
        else if (_episodeCount < 3000) {
            _agentSpawnPoint = upperLeftCorner;
            transform.rotation = _upperLeftCornerRotation;
        }
        else if (_episodeCount < 4000) {
            _agentSpawnPoint = upperRightCorner;
            transform.rotation = _upperRightCornerRotation;
        }

        // Starting point
        transform.localPosition = _agentSpawnPoint;
        float randomizerX = 0f;
        float randomizerZ = 0f;

        do
        {
            // Pellet random location
            randomizerX = Random.Range(-4.5f, 4.5f);
            randomizerZ = Random.Range(-2f, 2f);
            _target.localPosition = new Vector3(randomizerX, 0.25f, randomizerZ);
        } while (Vector3.Distance(_target.localPosition, transform.localPosition) < 2f);

        // Barrier position
        _obstacle.localPosition = new Vector3(1.5f, 0.5f, 2.1f);
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent position
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);
        sensor.AddObservation(transform.rotation.x);
        sensor.AddObservation(transform.rotation.z);

        // Target position
        sensor.AddObservation(_target.localPosition.x);
        sensor.AddObservation(_target.localPosition.z);
        
        // Obstacle position
        sensor.AddObservation(_obstacle.localPosition.x);
        sensor.AddObservation(_obstacle.localPosition.z);

        // Distance between the agent and the target
        sensor.AddObservation(Vector3.Distance(_target.localPosition, transform.localPosition));

        // Distance between the agent and the obstacle
        sensor.AddObservation(Vector3.Distance(_obstacle.localPosition, transform.localPosition));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var actionTaken = actions.ContinuousActions;

        float _actionSpeed = (actionTaken[0] + 1)/2; // actionTaken[0]; // for backward
        float _actionSteering = actionTaken[1];
        transform.Translate(_actionSpeed * Vector3.forward * _moveSpeed * Time.fixedDeltaTime);
        transform.Rotate(Vector3.up, _actionSteering * 180f * Time.fixedDeltaTime);
        //transform.rotation = Quaternion.Euler(0f, _actionSteering * 180f , 0f);
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
        actions[0] = -1; // Vertical // actions[0] = 0;  // for backward
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

    private void OnTriggerEnter(Collider collider)
    {
        int _reward;
        if (collider.CompareTag("Pellet"))
        {
            _reward = 2;
            AddReward(_reward);
            _pelletsCollected++;
            _overallScore += _reward;
            EndEpisode();
            _episodeCount++;
        }
        if(collider.CompareTag("Wall") || collider.CompareTag("Barrier"))
        {
            _reward = -1;
            AddReward(_reward);
            if (_overallScore > 0)
                _overallScore += _reward;
            EndEpisode();
            _episodeCount++;
        }
    }
    private void Update()
    {
        _pelletText.text = "Pellets collected: " + _pelletsCollected;
        _overallText.text = "Overall Score: " + _overallScore;
    }
}
