using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interfacetest : MonoBehaviour , IConsole
{
    public void ConsoleCommand()
    {
        Debug.Log("msg");
        Destroy(this.gameObject);
    }
}
