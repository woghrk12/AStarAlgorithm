using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform cameraTransform = null;

    [SerializeField] private Transform target = null;

    private void Awake()
    {
        cameraTransform = GetComponent<Transform>();
    }

    private void LateUpdate()
    {
        cameraTransform.position = new Vector3(target.position.x, target.position.y, cameraTransform.position.z);
    }
}
