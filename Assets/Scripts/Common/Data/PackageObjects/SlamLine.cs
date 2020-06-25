﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;


namespace Elektronik.Common.Data.PackageObjects
{
    public struct SlamLine
    {
        public readonly SlamPoint pt1;
        public readonly SlamPoint pt2;
        public SlamLine(SlamPoint pt1, SlamPoint pt2)
        {
            this.pt1 = pt1;
            this.pt2 = pt2;
        }
    }
}
