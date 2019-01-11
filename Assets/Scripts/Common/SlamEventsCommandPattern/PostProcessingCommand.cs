﻿using Elektronik.Common;
using Elektronik.Common.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Elektronik.Common.Containers;

namespace Elektronik.Common.SlamEventsCommandPattern
{
    public class PostProcessingCommand : MacroCommand
    {
        public PostProcessingCommand(
            ISlamContainer<SlamPoint> pointsContainer,
            ISlamContainer<SlamLine> linesContainer,
            SlamObservationsGraph graph,
            Helmet helmet,
            ISlamEvent slamEvent)
        {
            SlamPoint[] points;
            if (slamEvent.Points != null)
            {
                points = slamEvent.Points.Where(p => p.id != -1).ToArray();
                for (int i = 0; i < points.Length; ++i)
                {
                    points[i].color = Color.black;
                }
                m_commands.Add(new UpdateCommand(pointsContainer, graph, helmet, points, null));
                m_commands.Add(new RemoveCommand(pointsContainer, linesContainer, graph, slamEvent));
            }
            
        }
    }
}