using FIMSpace.Generating;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FIMSpace.Graph
{
    public static class FGraph_RunHandler
    {

        public static void RefreshConnections<T>(List<T> drawingNodes) where T : FGraph_NodeBase
        {
            for (int p = 0; p < drawingNodes.Count; p++)
            {
                RefreshConnectorsConnections(drawingNodes[p], drawingNodes);
                RefreshPortConnections(drawingNodes[p], drawingNodes);
            }
        }


        /// <summary>
        /// Refreshing ports connections references basing on 'drawingNodes' references
        /// </summary>
        public static void RefreshPortConnections<T>(T node, List<T> drawingNodes) where T : FGraph_NodeBase
        {
            if (node == null) return;

            var inputPorts = node.GetInputPorts();

            if (inputPorts != null)
            {
                FGenerators.CheckForNulls(inputPorts);

                for (int i = 0; i < inputPorts.Count; i++)
                    inputPorts[i].RefreshPortConnections(drawingNodes);
            }

            var outputPorts = node.GetOutputPorts();

            if (outputPorts != null)
            {
                FGenerators.CheckForNulls(outputPorts);

                for (int i = 0; i < outputPorts.Count; i++)
                    outputPorts[i].RefreshPortConnections(drawingNodes);
            }
        }


        public static void RefreshConnectorsConnections<T>(T node, List<T> drawingNodes) where T : FGraph_NodeBase
        {
            if (node == null) return;

            var inputConns = node.InputConnections;
            FGenerators.CheckForNulls(inputConns);

            for (int i = inputConns.Count - 1; i >= 0; i--)
            {
                //InputConnections[i].Computing = true;
                inputConns[i].RefreshReferences(drawingNodes);

                if ((inputConns[i].From is null) || (inputConns[i].To is null))
                { inputConns.RemoveAt(i); }
            }

            var outputConns = node.OutputConnections;
            FGenerators.CheckForNulls(outputConns);

            for (int i = outputConns.Count - 1; i >= 0; i--)
            {
                //OutputConnections[i].Computing = true;
                outputConns[i].RefreshReferences(drawingNodes);

                if ((outputConns[i].From is null) || (outputConns[i].To is null))
                { outputConns.RemoveAt(i); }
            }
        }


        public static void ReconstructConnectionsRelations(List<FGraph_NodeBase> allNodes)
        {
            for (int p = 0; p < allNodes.Count; p++)
            {
                RefreshConnectorsConnections(allNodes[p], allNodes);
                RefreshPortConnections(allNodes[p], allNodes);
            }
        }

        public static void ReconstructConnectorsConnectionsRelations(FGraph_NodeBase node)
        {
            var inputConns = node.InputConnections;
            for (int i = inputConns.Count - 1; i >= 0; i--)
            {
                var conn = inputConns[i];
                if (conn == null || conn.ConnectionToID == -1 || conn.ConnectionFromID == -1)
                { inputConns.RemoveAt(i); continue; }
            }

            var outputConns = node.OutputConnections;
            for (int i = outputConns.Count - 1; i >= 0; i--)
            {
                var conn = outputConns[i];
                if (conn == null || conn.ConnectionToID == -1 || conn.ConnectionFromID == -1)
                { outputConns.RemoveAt(i); continue; }
            }
        }

        public static void ReconstructPortsConnectionsRelations(FGraph_NodeBase node, List<FGraph_NodeBase> allNodes)
        {
            var outPorts = node.GetOutputPorts();
            if (outPorts != null)
                for (int i = outPorts.Count - 1; i >= 0; i--)
                {
                    node.RefreshPort(outPorts[i]);
                    outPorts[i].RefreshPortConnections(allNodes);
                }

            var inPorts = node.GetInputPorts();
            if (inPorts != null)
                for (int i = inPorts.Count - 1; i >= 0; i--)
                {
                    node.RefreshPort(inPorts[i]);
                    inPorts[i].RefreshPortConnections(allNodes);
                }
        }




    }
}
