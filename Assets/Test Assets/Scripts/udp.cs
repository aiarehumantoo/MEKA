using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class udp : MonoBehaviour
{
    IPEndPoint iep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);
    Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    void Start()
    {
        //IPEndPoint iep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);
        //Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        server.Bind(iep);
        server.Listen(10);
        Console.WriteLine("Waiting for connection...");
    }

    void Update()
    {
        using (Socket client = server.Accept())
        {
            //while (true)
            //{
                string s = Console.ReadLine().ToUpper();

                if (s.Equals("QUIT"))
                {
                    //break;
                }
                if (s.Equals("SEND"))
                {
                    // send the file
                    byte[] buffer = File.ReadAllBytes("testimage.jpg");
                    client.Send(buffer, buffer.Length, SocketFlags.None);
                    Console.WriteLine("Send success!");
                }

            if (s.Equals("TEST"))
            {
                Debug.Log("received message");
                //break;
            }

            //}
        }
    }
}
