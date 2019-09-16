using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalRegisterySingleton : MonoBehaviour
{
    public static PortalRegisterySingleton Instance { get; private set; }
    public List<KeyValuePair<Portal, Portal>> Portals = new List<KeyValuePair<Portal, Portal>>();

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            throw new Exception("PortalRegisterySingleton instance is already filled in! Is this script placed twice in the scene?");
        }
    }

    public void RegisterPortal(Portal portal)
    {
        Debug.Assert(portal.connectedPortal != null && portal.connectedPortal.connectedPortal == portal);

        foreach (KeyValuePair<Portal, Portal> pairOfPortals in Portals)
        {
            if (pairOfPortals.Key == portal || pairOfPortals.Key == portal.connectedPortal ||
                pairOfPortals.Value == portal || pairOfPortals.Value == portal.connectedPortal)
            {
                return;
            }
        }

        Portals.Add(new KeyValuePair<Portal, Portal>(portal, portal.connectedPortal));
    }

    public Portal GetPortalById(uint id)
    {
        foreach (KeyValuePair<Portal, Portal> pairOfPortals in Portals)
        {
            if (pairOfPortals.Key.Id == id  )
            {
                return pairOfPortals.Key;
            }
            if( pairOfPortals.Value.Id == id)
            {
                return pairOfPortals.Value;
            }
        }

        return null;
    }
}
