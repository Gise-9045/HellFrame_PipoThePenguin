using System;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RailCollider : MonoBehaviour
{
    [SerializeField] private RailInfo railInfo;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MainManager.Instance.RailCamera.SetRail(railInfo);
        }
    }
}