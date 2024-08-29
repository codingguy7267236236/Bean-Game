using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

static public class SceneChanger
{
    static public void ChangeScene(int index)
    {
        SceneManager.LoadScene(index);
    }
}
