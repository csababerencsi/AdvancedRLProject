using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Integrations.Match3;
using UnityEngine.EventSystems;
using UnityEngine.AI;

public class BarrierController : MonoBehaviour
{
    [SerializeField] float _speed = 1.5f;
    Vector3 _startPos = new(1.5f, 0.5f, 2.1f);
    Vector3 _endPos = new(1.5f, 0.5f, -2.1f);
    bool _isMovingForward = true;

    void FixedUpdate()
    {
        Vector3 _target = _isMovingForward ? _endPos : _startPos;

        transform.localPosition = Vector3.MoveTowards(transform.localPosition, _target, Time.deltaTime * _speed);

        if(Vector3.Distance(transform.localPosition, _target) < 0.001f)
        {
            _isMovingForward = !_isMovingForward;
        }
    }
}
