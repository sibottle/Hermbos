using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class thingCoonect : NetworkBehaviour
{
    public Button sercli;
    public Button server;
    public Button cli;
    public TMP_InputField tf;
    public GameObject ui;

        NetworkManager manager;

        public int offsetX;
        public int offsetY;

        void Awake()
        {
            manager = GetComponent<NetworkManager>();
        }

        public void onClickServer()
        {
            // Server Only
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                // cant be a server in webgl build
                server.gameObject.GetComponentInChildren<TMP_Text>().text = "(  WebGL cannot be server  )";
            }
            else
            {
                manager.StartServer();
                ui.SetActive(false);
            }
        }

        public void onClickAll()
        {
            // Server + Client
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                manager.StartHost();
                ui.SetActive(false);
            }
        }

        public void onClickClient()
        {
            manager.StartClient();
            ui.SetActive(false);
            manager.networkAddress = tf.text;
        }

        void StatusLabels()
        {
            // host mode
            // display separately because this always confused people:
            //   Server: ...
            //   Client: ...
            if (NetworkServer.active && NetworkClient.active)
            {
                GUILayout.Label($"<b>Host</b>: running via {Transport.activeTransport}");
            }
            // server only
            else if (NetworkServer.active)
            {
                GUILayout.Label($"<b>Server</b>: running via {Transport.activeTransport}");
            }
            // client only
            else if (NetworkClient.isConnected)
            {
                GUILayout.Label($"<b>Client</b>: connected to {manager.networkAddress} via {Transport.activeTransport}");
            }
        }

        void StopButtons()
        {
            // stop host if host mode
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                if (GUILayout.Button("Stop Host"))
                {
                    manager.StopHost();
                }
            }
            // stop client if client-only
            else if (NetworkClient.isConnected)
            {
                if (GUILayout.Button("Stop Client"))
                {
                    manager.StopClient();
                }
            }
            // stop server if server-only
            else if (NetworkServer.active)
            {
                if (GUILayout.Button("Stop Server"))
                {
                    manager.StopServer();
                }
            }
        }
}
