using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableOnAwake : MonoBehaviour
{
    public bool Recursive = true;
        
    void Awake()
    {
        if (Recursive)
        {
            gameObject.SetActiveRecursively(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
}
