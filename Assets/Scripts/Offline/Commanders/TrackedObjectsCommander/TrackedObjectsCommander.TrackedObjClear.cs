﻿using Elektronik.Common.Containers;
using Elektronik.Common.Data.PackageObjects;
using Elektronik.Common.Data.Pb;
using Elektronik.Common.Maps;
using Elektronik.Common.PackageViewUpdateCommandPattern.Slam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Elektronik.Offline.Commanders.TrackedObjectsCommander
{
    public partial class TrackedObjectsCommander
    {
        private class TrackedObjClear : ClearCommand<TrackedObjPb>
        {
            private Dictionary<int, SlamLine[]> m_trackStates;

            private GameObjectsContainer<TrackedObjPb> m_goContainer;

            public TrackedObjClear(GameObjectsContainer<TrackedObjPb> container)
                : base(container)
            {
                m_goContainer = container;
                m_trackStates = new Dictionary<int, SlamLine[]>();
                foreach (var o in container)
                {
                    if (container.TryGet(o, out GameObject gameObject))
                    {
                        var helmet = gameObject.GetComponent<Helmet>();
                        m_trackStates[o.Id] = helmet.GetTrackState();
                    }
                }
            }

            public override void UnExecute()
            {
                base.UnExecute();
                foreach (var o in m_undoObjects)
                {
                    if (m_goContainer.TryGet(o, out GameObject gameObject))
                    {
                        var helmet = gameObject.GetComponent<Helmet>();
                        if (m_trackStates.ContainsKey(o.Id))
                            helmet.RestoreTrackState(m_trackStates[o.Id]);
                    }
                }
            }
        }
    }
}
