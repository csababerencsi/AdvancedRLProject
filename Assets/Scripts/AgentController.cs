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
    [SerializeField] float _moveSpeed = 2.5f;
    int _pelletsCollected = 0;
    int _overallScore = 0;
    int _episodeCount = 0;
    Vector3 _agentSpawnPoint;

    //private const float MAX_DISTANCE = 28.28427f;

    [SerializeField] TextMeshProUGUI _pelletText;
    [SerializeField] TextMeshProUGUI _overallText;
    public override void OnEpisodeBegin()
    {

        // Lower Left Corner
        Vector3 lowerLeftCorner = new(-4f, 0.25f, -1.7f);
        var _lowerLeftCornerRotation = Quaternion.Euler(0f, 45f, 0f);

        // Lower Right Corner
        Vector3 lowerRightCorner = new(4f, 0.25f, -1.7f);
        var _lowerRightCornerRotation = Quaternion.Euler(0f, -45f, 0f);

        // Upper Left Corner
        Vector3 upperLeftCorner = new(-4f, 0.25f, 1.7f);
        var _upperLeftCornerRotation = Quaternion.Euler(0f, 135f, 0f);

        // Upper Right Corner
        Vector3 upperRightCorner = new(4f, 0.25f, 1.7f);
        var _upperRightCornerRotation = Quaternion.Euler(0f, -135f, 0f);

        // Middle
        Vector3 middlePoint = new(0f, 0f, 0f);
        var _middlePointRotation = Quaternion.Euler(0f, 0f, 0f);

        if (_episodeCount % 100 == 0)
        {
            int _randomNumber = Random.Range(0, 5);
            switch (_randomNumber)
            {
                case 0:
                    _agentSpawnPoint = lowerLeftCorner;
                    break;

                case 1:
                    _agentSpawnPoint = lowerRightCorner;
                    break;

                case 2:
                    _agentSpawnPoint = upperLeftCorner;
                    break;

                case 3:
                    _agentSpawnPoint = upperRightCorner;
                    break;

                case 4:
                    _agentSpawnPoint = middlePoint;
                    break;

                default:
                    break;
            }
            //Debug.Log("Starting Position: " + _agentSpawnPoint);
        }
        switch (_agentSpawnPoint)
        {
            case Vector3 vector when vector.Equals(lowerLeftCorner):
                transform.rotation = _lowerLeftCornerRotation;
                break;

            case Vector3 vector when vector.Equals(lowerRightCorner):
                transform.rotation = _lowerRightCornerRotation;
                break;

            case Vector3 vector when vector.Equals(upperLeftCorner):
                transform.rotation = _upperLeftCornerRotation;
                break;

            case Vector3 vector when vector.Equals(upperRightCorner):
                transform.rotation = _upperRightCornerRotation;
                break;

            case Vector3 vector when vector.Equals(middlePoint):
                transform.rotation = _middlePointRotation;
                break;

            default:
                break;
        }
        transform.localPosition = _agentSpawnPoint;
        do
        {
            // Pellet random location
            float randomizerX = Random.Range(-4.5f, 4.5f);
            float randomizerZ = Random.Range(-2f, 2f);
            _target.localPosition = new Vector3(randomizerX, 0.25f, randomizerZ);
        } while (Vector3.Distance(_target.localPosition, transform.localPosition) < 1.5f && Vector3.Distance(_target.localPosition, _obstacle.localPosition) < 1.5f); //&& Vector3.Distance(_obstacle.localPosition, _target.localPosition) < 1f);

        // Barrier position
        _obstacle.localPosition = new Vector3(1.5f, 0.5f, 2.1f);

        _episodeCount++;
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
        transform.Translate(_actionSpeed * Vector3.forward * _moveSpeed * Time.deltaTime);
        transform.Rotate(Vector3.up, _actionSteering * 180f * Time.deltaTime);

        
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
            actions[0] = +1;
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
        }
        if(collider.CompareTag("Wall") || collider.CompareTag("Barrier"))
        {
            _reward = -1;
            AddReward(_reward);
            _overallScore = 0;
            EndEpisode();
        }
    }
    private void Update()
    {
        _pelletText.text = "Pellets collected: " + _pelletsCollected;
        _overallText.text = "Overall score: " + _overallScore;
    }
}
