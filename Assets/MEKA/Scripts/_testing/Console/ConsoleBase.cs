//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

/*public class ConsoleBase : MonoBehaviour
{
    public interface IConsole
    {
        void ReceiveCommand(string command);
    }
}*/

//using UnityEngine;
//using System.Collections;

//using UnityEngine;
//using UnityEngine.Networking;
//using System.Collections;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

// Basic interface
public interface IConsole
{
    //void ReceiveCommand(string command);

    void ConsoleCommand();
}

//**************************************************
public class ConsoleBase : ConsoleTcpClient
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        //base.Update();
    }

    protected override void ConsoleCommand(string message)
    {
        //base.ConsoleCommand(message);

        if (message == "FIRE") // for testing
        {
            Debug.Log("COMMAND WAS TO FIRE");
            SendReply("Fired a shot");
        }

        if (message == "HELP")
        {
            Debug.Log("Sending back list of console commands");
            SendReply("List of console commands: \n" +"Command1 - does stuff \nCommand2 - does more stuff \n...");
        }

        SendReply("Invalid command");
    }
}

#if false
//This is a generic interface where T is a placeholder
//for a data type that will be provided by the 
//implementing class.
public interface IDamageable<T>
{
    void Damage(T damageTaken);
}

using UnityEngine;
using System.Collections;

public class Avatar : MonoBehaviour, IKillable, IDamageable<float>
{
    //The required method of the IKillable interface
    public void Kill()
    {
        //Do something fun
    }

    //The required method of the IDamageable interface
    public void Damage(float damageTaken)
    {
        //Do something fun
    }
}

#endif