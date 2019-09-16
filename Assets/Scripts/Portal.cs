using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Portal connectedPortal;
    public uint Id = 1;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(connectedPortal.connectedPortal == this, "The connected portal doesn't go back to the current portal!");

        PortalRegisterySingleton.Instance.RegisterPortal(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
