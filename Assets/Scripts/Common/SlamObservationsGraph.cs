﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Elektronik.Common.Containers;

namespace Elektronik.Common
{
    [RequireComponent(typeof(FastLinesCloud))]
    public class SlamObservationsGraph : MonoBehaviour
    {
        public GameObject observationPrefab;

        ISlamContainer<SlamLine> m_linesContainer;

        SortedDictionary<int, SlamObservationNode> m_slamObservationNodes;

        private void Awake()
        {
            m_slamObservationNodes = new SortedDictionary<int, SlamObservationNode>();
            m_linesContainer = new SlamLinesContainer(GetComponent<FastLinesCloud>());
        }

        /// <summary>
        /// Добавить Observation
        /// </summary>
        /// <param name="observation"></param>
        public void Add(SlamObservation observation)
        {
            if (observation.covisibleObservationsIds == null || observation.covisibleObservationsOfCommonPointsCount == null)
            {
                Debug.LogWarningFormat("Wrong observation id {0}", observation.id);
                return;
            }
            var newNode = new SlamObservationNode(MF_AutoPool.Spawn(observationPrefab, observation.position, observation.orientation)); // создаём новый узел

            newNode.SlamObservation = new SlamObservation(observation);
            m_slamObservationNodes.Add(observation.id, newNode);

            // обновляем связи
            UpdateConnections(observation);
        }

        /// <summary>
        /// Переместить Observation
        /// </summary>
        /// <param name="observation">Observation с абсолютными координатами</param>
        public void Replace(SlamObservation observation)
        {
            Debug.AssertFormat(m_slamObservationNodes.ContainsKey(observation.id), "Observation with Id {0} doesn't exist", observation.id);

            // находим узел, который необходимо переместить
            SlamObservationNode nodeToReplace = m_slamObservationNodes[observation.id];

            // Перемещаем в сцене
            nodeToReplace.ObservationObject.transform.position = observation.position;
            nodeToReplace.ObservationObject.transform.rotation = observation.orientation;

            nodeToReplace.SlamObservation.position = observation.position;
            nodeToReplace.SlamObservation.orientation = observation.orientation;

            // Обновляем связи
            UpdateConnections(nodeToReplace.SlamObservation);
        }

        /// <summary>
        /// Обновить соединения для узла графа
        /// </summary>
        /// <param name="observation"></param>
        public void UpdateConnections(SlamObservation observation)
        {
            if (observation.id == -1)
                return;
            Debug.AssertFormat(ObservationExists(observation.id), "[Graph update connections] observation {0} doesn't exists", observation.id);
            
            SlamObservationNode obsNode = m_slamObservationNodes[observation.id];
            obsNode.SlamObservation.covisibleObservationsIds = observation.covisibleObservationsIds;
            obsNode.SlamObservation.covisibleObservationsOfCommonPointsCount = observation.covisibleObservationsOfCommonPointsCount;

            // 1. Найти существующих в графе соседей
            SlamObservationNode[] existsNeighbors = observation.covisibleObservationsIds
                .Where(ObservationExists)
                .Select(obsId => m_slamObservationNodes[obsId])
                .ToArray();

            foreach (var neighbor in existsNeighbors)
            {
                // 2. Проверить наличие соединения между ними
                if (!neighbor.NodeLineIDPair.ContainsKey(obsNode))
                {
                    // 3. Где соединение отсутствует добавить соединение
                    SlamLine newLineCinema = new SlamLine()
                    {
                        color = Color.gray,
                        pointId1 = obsNode.SlamObservation.id,
                        pointId2 = neighbor.SlamObservation.id,
                        isRemoved = false,
                        vert1 = obsNode.SlamObservation.position,
                        vert2 = neighbor.SlamObservation.position,
                    };
                    int lineId = m_linesContainer.Add(newLineCinema);
                    neighbor.NodeLineIDPair.Add(obsNode, lineId);
                    obsNode.NodeLineIDPair.Add(neighbor, lineId);
                }
            }

            // 4. Обновить вершины соединений
            foreach (var connection in obsNode.NodeLineIDPair)
            {
                int existsLineId = connection.Value;
                SlamLine currentConnection = m_linesContainer.Get(existsLineId);

                Debug.Assert(
                    obsNode.SlamObservation.id == currentConnection.pointId1 || obsNode.SlamObservation.id == currentConnection.pointId2,
                    "[SlamObservationGraph.UpdateConnections] at least one of vertex point id must be equal to observation id");

                if (obsNode.SlamObservation.id == currentConnection.pointId1)
                {
                    currentConnection.vert1 = obsNode.SlamObservation.position;
                    currentConnection.vert2 = connection.Key.SlamObservation.position;
                }
                else if (obsNode.SlamObservation.id == currentConnection.pointId2)
                {
                    currentConnection.vert1 = connection.Key.SlamObservation.position;
                    currentConnection.vert2 = obsNode.SlamObservation.position;
                }
                m_linesContainer.Update(currentConnection);
            }
        }

        /// <summary>
        /// Удалить Observation
        /// </summary>
        /// <param name="observationId"></param>
        public void Remove(int observationId)
        {
            // находим узел, который необходимо удалить
            SlamObservationNode observationToRemove = m_slamObservationNodes[observationId];

            // находим узлы из которых нужно выпилить текущий узел
            SlamObservationNode[] covisibleNodes = observationToRemove.NodeLineIDPair.Select(kv => kv.Key).ToArray();
            
            // выпиливаем узел из совидимых узлов и очищаем облако
            foreach (var covisibleNode in covisibleNodes)
            {
                int lineId = covisibleNode.NodeLineIDPair[observationToRemove]; // ID линии, которую нужно убрать
                covisibleNode.NodeLineIDPair.Remove(observationToRemove);
                m_linesContainer.Remove(lineId);
            }

            MF_AutoPool.Despawn(observationToRemove.ObservationObject); // выпиливаем со сцены
            m_slamObservationNodes.Remove(observationId); // выпиливаем из графа
        }

        /// <summary>
        /// Полностью обновить информацию об Observation
        /// </summary>
        /// <param name="observation"></param>
        public void UpdateNode(SlamObservation observation)
        {
            Debug.AssertFormat(m_slamObservationNodes.ContainsKey(observation.id), "Observation with Id {0} doesn't exist", observation.id);

            Remove(observation.id);
            Add(observation);
        }

        /// <summary>
        /// Обновить информацию об Observation, либо добавить его, если он не существует в графе
        /// </summary>
        /// <param name="observation"></param>
        public void UpdateOrAdd(SlamObservation observation)
        {
            if (ObservationExists(observation.id))
            {
                UpdateNode(observation);
            }
            else
            {
                Add(observation);
            }
        }

        public SlamObservation Get(int observationId)
        {
            Debug.AssertFormat(ObservationExists(observationId), "[SlamObservationsGraph.Get] graph doesn't contain observation {0}", observationId);
            return m_slamObservationNodes[observationId].SlamObservation;
        }

        public bool ObservationExists(int observationId)
        {
            return m_slamObservationNodes.ContainsKey(observationId);
        }

        public void Repaint()
        {
            m_linesContainer.Repaint();
        }

        public void Clear()
        {
            m_slamObservationNodes.Clear();
            m_linesContainer.Clear();
            MF_AutoPool.DespawnPool(observationPrefab);
        }

        public SlamObservation[] GetAll()
        {
            return m_slamObservationNodes.Select(obsNode => obsNode.Value.SlamObservation).ToArray();
        }
    }
}