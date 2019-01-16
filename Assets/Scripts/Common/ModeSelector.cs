﻿using Elektronik.Offline;
using Elektronik.Online;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Elektronik.Common
{
    public class ModeSelector : MonoBehaviour
    {
        public GameObject filePlayer;
        public GameObject onlinePlayer;

        private void Start()
        {
            if (FileModeSettings.Current != null)
            {
                filePlayer.SetActive(true);
            }
            else if (OnlineModeSettings.Current != null)
            {
                onlinePlayer.SetActive(true);
            }
            else
            {
                throw new NullReferenceException("No one of modes are initialized");
            }
        }
    }
}
