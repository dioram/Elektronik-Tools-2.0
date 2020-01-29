﻿using Elektronik.Common.Containers;
using Elektronik.Common.Data.PackageObjects;
using Elektronik.Common.Data.Packages;
using Elektronik.Common.Data.Packages.SlamActionPackages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elektronik.Common.PackageViewUpdateCommandPattern.Slam
{
    public class UpdateConnectionsCommand : IPackageViewUpdateCommand
    {
        private readonly SlamLine[] m_connections2Restore;
        private readonly SlamPoint[] m_pts;
        private readonly IConnectionsContainer<SlamLine> m_connections;

        private class ConnectionsComparer : IEqualityComparer<SlamLine>
        {
            public bool Equals(SlamLine x, SlamLine y) =>
                x.pt1.id == y.pt1.id && x.pt2.id == y.pt2.id ||
                x.pt1.id == y.pt2.id && x.pt2.id == y.pt1.id;
            public int GetHashCode(SlamLine obj) => obj.GetHashCode();
        }

        public UpdateConnectionsCommand(IConnectionsContainer<SlamLine> connections, IEnumerable<SlamPoint> changedObjs)
        {
            m_pts = changedObjs.ToArray();
            m_connections = connections;
            IEnumerable<SlamLine> connections2Restore = Enumerable.Empty<SlamLine>();
            foreach (var pckgPt in m_pts)
            {
                connections2Restore = connections2Restore.Concat(m_connections[pckgPt]);
            }
            m_connections2Restore = connections2Restore.Distinct(new ConnectionsComparer()).ToArray();
        }

        public void Execute()
        {
            m_connections.Update(m_pts);
        }

        public void UnExecute()
        {
            m_connections.Update(m_connections2Restore);
        }
    }
}
