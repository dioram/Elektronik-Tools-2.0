﻿using Elektronik.Common.Settings;
using Elektronik.Online.Settings;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Elektronik.Online.VRHacker
{
    public class VRHackerUDPConnection : MonoBehaviour
    {
        private UdpClient m_client;

        private IPEndPoint m_ep;

        void Awake()
        {
            m_ep = new IPEndPoint(SettingsBag.Current[SettingName.IPAddress].As<IPAddress>(), SettingsBag.Current[SettingName.Port].As<int>());
            m_client = new UdpClient(m_ep);
        }

        void OnDestroy()
        {
            m_client.Client.Shutdown(SocketShutdown.Both);
            m_client.Close();
        }

        private Pose Trans(Pose pose)
        {
            Matrix4x4 m_initPose = Matrix4x4.identity;
            Vector3 curPos = pose.position;
            Quaternion curRot = pose.rotation.normalized;
            curPos.y = -curPos.y;
            curRot.y = -curRot.y; curRot.w = -curRot.w;

            Matrix4x4 curHomo = Matrix4x4.TRS(curPos, curRot, Vector3.one);

            curHomo = m_initPose * curHomo;
            curPos = curHomo.GetColumn(3);
            curRot = Quaternion.LookRotation(curHomo.GetColumn(2), curHomo.GetColumn(1));
            return new Pose(curPos, curRot);
        }

        public bool GetPose(out Pose pose)
        {
            bool result = false;
            Quaternion rot = Quaternion.identity;
            Vector3 pos = Vector3.one;
            if (m_client.Available > 0)
            {
                byte[] datagram = m_client.Receive(ref m_ep);
                float[] fdata = Enumerable.Range(0, 7)
                    .Select(idx => BitConverter.ToSingle(datagram, idx * sizeof(float)))
                    .ToArray();
                pos = new Vector3(fdata[0], fdata[1], fdata[2]);
                rot = new Quaternion(fdata[4], fdata[5], fdata[6], fdata[3]);
                result = true;
            }
            pose = Trans(new Pose(pos, rot));
            return result;
        }
    }
}