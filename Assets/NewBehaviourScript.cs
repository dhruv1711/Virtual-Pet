using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;
using UnityEditor;
using UnityEditor.Animations;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class NewBehaviourScript : MonoBehaviour
{
    System.Threading.Thread SocketThread;
    volatile bool keepReading = false;
    public Animator anim;
    string data;
    public Transform imageTarget;

    public void Start()
    {
        startServer();
    }

    public void Update()
    { 
        if (Input.GetKeyDown("1"))
        {
            anim.Play("sit");
        }
        if (Input.GetKeyDown("2"))
        {
            anim.Play("stand");
        }
        if (Input.GetKeyDown("3"))
        {
            anim.Play("run");
        }

        
        StartCoroutine(HandleIt());


    }

    private IEnumerator HandleIt()
    {
        // process pre-yield
        float old_pos = imageTarget.position.x;
        yield return new WaitForSeconds(1.0f);
        float new_pos = imageTarget.position.x;
        float dist = Math.Abs(old_pos - new_pos);
        Debug.Log("old " + old_pos.ToString());
        Debug.Log("new " + new_pos.ToString());
        Debug.Log("dist" + dist.ToString());
        if (dist > 0.1)
        {
            anim.Play("run");
        }
        else
        {
            anim.Play("stand");
        }
        // process post-yield
    }

    public void startServer()
    {
        SocketThread = new System.Threading.Thread(networkCode);
        SocketThread.IsBackground = true;
        SocketThread.Start();
    }
    


    private string getIPAddress()
    {
        IPHostEntry host;
        string localIP = "";
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
            }

        }
        return localIP;
    }


    Socket listener;
    Socket handler;

    public void networkCode()
    {
        byte[] bytes = new Byte[1024];

        Debug.Log("Ip " + getIPAddress().ToString());
        IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);

        listener = new Socket(ipAddr.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);


        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(10);


            while (true)
            {
                keepReading = true;

                Debug.Log("Waiting for Connection");

                handler = listener.Accept();
                Debug.Log("Client Connected");
 
                
                while (keepReading)
                {
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    Debug.Log("Received from Server");
                    data = null;

                    if (bytesRec <= 0)
                    {
                        keepReading = false;
                        handler.Disconnect(true);
                        break;
                    }

                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }

                    System.Threading.Thread.Sleep(1);
                }
                Debug.Log("Text received ->" + data.Replace("<EOF>", ""));

                byte[] message = Encoding.ASCII.GetBytes("Test Server");

                handler.Send(message);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

                System.Threading.Thread.Sleep(1);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    public void stopServer()
    {
        keepReading = false;

        if (SocketThread != null)
        {
            SocketThread.Abort();
        }

        if (handler != null && handler.Connected)
        {
            handler.Disconnect(false);
            Debug.Log("Disconnected!");
        }
    }

    public void OnDisable()
    {
        stopServer();
    }
}
