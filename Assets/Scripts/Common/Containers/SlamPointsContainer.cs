﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Elektronik.Common.Containers
{
    public class SlamPointsContainer : ISlamContainer<SlamPoint>
    {
        private SortedDictionary<int, SlamPoint> m_points;
        private FastPointCloud m_pointCloud;

        private int m_added = 0;
        private int m_removed = 0;
        private int m_diff = 0;

        public SlamPointsContainer(FastPointCloud cloud)
        {
            m_points = new SortedDictionary<int, SlamPoint>();
            m_pointCloud = cloud;
        }

        public int Add(SlamPoint point)
        {
            ++m_diff;
            ++m_added;
            m_pointCloud.SetPoint(point.id, point.position, point.color);
            m_points.Add(point.id, point);
            return point.id;
        }

        public void AddRange(SlamPoint[] points)
        {
            foreach (var point in points)
            {
                Add(point);
            }
        }

        public void Update(SlamPoint point)
        {
            Debug.AssertFormat(m_points.ContainsKey(point.id), "[Update] Container doesn't contain point with id {0}", point.id);
            m_pointCloud.SetPoint(point.id, point.position, point.color);
            m_points[point.id] = point;
        }

        public void ChangeColor(SlamPoint point)
        {
            //Debug.LogFormat("[Change color] point {0} color: {1}", point.id, point.color);
            Debug.AssertFormat(m_points.ContainsKey(point.id), "[Change color] Container doesn't contain point with id {0}", point.id);
            m_pointCloud.SetPointColor(point.id, point.color);
            SlamPoint current = m_points[point.id];
            current.color = point.color;
            m_points[point.id] = current;
        }

        public void Remove(int pointId)
        {
            --m_diff;
            ++m_removed;
            //Debug.LogFormat("Removing point {0}", pointId);
            Debug.AssertFormat(m_points.ContainsKey(pointId), "[Remove] Container doesn't contain point with id {0}", pointId);
            m_pointCloud.SetPoint(pointId, Vector3.zero, new Color(0, 0, 0, 0));
            m_points.Remove(pointId);
        }

        public void Remove(SlamPoint point)
        {
            Remove(point.id);
        }

        public void Clear()
        {
            int[] pointsIds = m_points.Keys.ToArray();
            for (int i = 0; i < pointsIds.Length; ++i)
            {
                Remove(pointsIds[i]);
            }
            m_points.Clear();
            Repaint();

            Debug.LogFormat("[Clear] Added points: {0}; Removed points: {1}; Diff: {2}", m_added, m_removed, m_diff);
            m_added = 0;
            m_removed = 0;
        }

        public SlamPoint[] GetAll()
        {
            return m_points.Select(kv => kv.Value).ToArray();
        }

        public void Set(SlamPoint point)
        {
            SlamPoint buttPlug;
            if (!TryGet(point, out buttPlug))
            {
                Add(point);
            }
            else
            {
                Update(point);
            }
        }

        public SlamPoint Get(int pointId)
        {
            //Debug.AssertFormat(m_points.ContainsKey(pointId), "[Get point] Container doesn't contain point with id {0}", pointId);
            if (!m_points.ContainsKey(pointId))
            {
                Debug.LogWarningFormat("[Get point] Container doesn't contain point with id {0}", pointId);
                return new SlamPoint();
            }
            
            return m_points[pointId];
        }

        public SlamPoint Get(SlamPoint point)
        {
            return Get(point.id);
        }

        public bool Exists(int pointId)
        {
            //return m_pointCloud.PointExists(pointId);
            return m_points.ContainsKey(pointId);
        }

        public bool Exists(SlamPoint point)
        {
            return Exists(point.id);
        }

        public bool TryGet(SlamPoint point, out SlamPoint current)
        {
            current = new SlamPoint();
            if (m_pointCloud.PointExists(point.id))
            {
                current = Get(point.id);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Repaint()
        {
            m_pointCloud.Repaint();
        }
    }
}