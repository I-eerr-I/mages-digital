using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicLightController : MonoBehaviour
{

    [SerializeField] private float _rotationSpeed = 0.01f;

    void Update()
    {
        transform.Rotate(_rotationSpeed * Time.deltaTime, 0.0f, 0.0f, Space.Self);
    }
}
