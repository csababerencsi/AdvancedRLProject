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
    [SerializeField] float _speed = 0.5f;
    Vector3 pointA;
    Vector3 pointB;
    

    void Start()
    {
            pointA = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
            pointB = new Vector3(transform.localPosition.x, transform.localPosition.y, -transform.localPosition.z);       
    }

    void Update()
    {
            float time = Mathf.PingPong(Time.time, 2f);
            transform.localPosition = Vector3.Lerp(pointA, pointB, time * _speed);
    }
}
