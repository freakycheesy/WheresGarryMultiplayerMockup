using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace WheresGarryMultiplayerMockup.Network
{
    public static class NetworkGUI
    {
        static string address = "127.0.0.1";
        static bool usePort = true;
        public static bool renderUi
        {
            get { 
                return Core.controller != null ? Core.controller.pause : false;
            }
        }
        public static void RenderGUI()
        {
            GUILayout.Box("Where's Garry Multiplayer Mockup");
            Cursor.visible = renderUi;
            if (!renderUi)
            {
                return;
            }
            if (!NetworkManager.isRunning)
            {
                if (GUILayout.Button("Create Server"))
                {
                    NetworkManager.CreateServer();
                    NetworkManager.JoinServer("127.0.0.1", true);
                }
                usePort = GUILayout.Toggle(usePort, "Use Port to connect");
                GUILayout.BeginHorizontal();
                address = GUILayout.TextField(address);
                if (GUILayout.Button("Join Server"))
                {
                    NetworkManager.JoinServer(address, usePort);
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button("Shutdown"))
                {
                    NetworkManager.Shutdown();
                }
            }
        }
    }
}
