using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class PlayerData
{
    static public int model=0;
    static public int costume = 0;
    static public bool host = false;
    static public string lobby = null;
    static public string username = "Player";

    static public void SetModel(int ind)
    {
        model = ind;
    }

    static public void SetCostume(int ind)
    {
        costume = ind;
    }

    static public void SetHost(bool val)
    {
        host = val;
    }

    static public void SetLobby(string val)
    {
        lobby = val;
    }

    static public void SetUsername(string usrname)
    {
        username = usrname;
    }
}
