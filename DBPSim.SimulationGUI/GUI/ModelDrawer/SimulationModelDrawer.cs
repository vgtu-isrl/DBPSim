using DBPSim.Simulation.ProcessModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace DBPSim.SimulationGUI.ModelDrawer
{
    class SimulationModelDrawer
    {

        private static TreeContainer.TreeNode rootNode = null;
        private static TreeContainer.TreeNode endNode = null;
        private static Dictionary<string, TreeContainer.TreeNode> nodes = new Dictionary<string, TreeContainer.TreeNode>();
        public static void DrawSimulationModel(TreeContainer.TreeContainer modelContainer, ProcessModelInstance modelInstance)
        {
            //ClearModel(modelContainer);

            foreach (var val in nodes.Values)
            {
                ((Border)val.Content).Background = Brushes.Gray;
            }
            if (modelContainer.Connections != null)
            {
                modelContainer.Connections.ForEach(x => x.Active = false);
            }

            TreeContainer.TreeNode lastNode = null;

            ProcessAction firstProcessAction = modelInstance.FirstProcessAction;
            if (firstProcessAction != null)
            {
                if (rootNode == null)
                {
                    rootNode = modelContainer.AddRoot(SimulationModelInstanceElementHelper.GetStartElement(), "__TreeNode" + ((uint)("root".GetHashCode())).ToString());
                    nodes.Add("root", rootNode);
                }

                TreeContainer.TreeNode node = null;
                bool init = firstProcessAction.Title.StartsWith("init_");
                // Add first element
                if (!init)
                {
                    if (nodes.ContainsKey(firstProcessAction.Title))
                    {
                        node = nodes[firstProcessAction.Title];
                        ((Border)node.Content).Background = firstProcessAction.Color;
                    }
                    else
                    {
                        Border guiElement = (Border)firstProcessAction.GUIElement;
                        guiElement.Background = firstProcessAction.Color;
                        node = modelContainer.AddNode(guiElement, "__TreeNode" + ((uint)firstProcessAction.Title.GetHashCode()).ToString(), "__TreeNode" + ((uint)("root".GetHashCode())).ToString());
                        nodes.Add(firstProcessAction.Title, node);

                    }
                    if (!rootNode.TreeChildren.Contains(node))
                    {
                        rootNode.TreeChildren.Add(node);
                    }
                }
                DrawSimulationModelRecursive(modelContainer, init ? rootNode : node, firstProcessAction, out lastNode, nodes);

                // Add last element
                if (modelInstance.Finished)
                {
                    if (!nodes.ContainsKey("endnode"))
                    {
                        endNode = modelContainer.AddNode(SimulationModelInstanceElementHelper.GetFinishElement(), "__TreeNode" + ((uint)("endnode".GetHashCode())).ToString(), lastNode);
                        nodes.Add("endnode", endNode);
                    }
                    if (!lastNode.TreeChildren.Contains(endNode))
                    {
                        lastNode.TreeChildren.Add(endNode);
                    }
                }
            }
            modelContainer.InvalidateVisual();
        }


        public static void ClearModel(TreeContainer.TreeContainer modelContainer)
        {
            nodes = new Dictionary<string, TreeContainer.TreeNode>();
            rootNode = null;
            endNode = null;
            if (modelContainer.Connections != null)
            {
                while (modelContainer.Connections.Count > 0)
                {
                    modelContainer.Connections.Remove(modelContainer.Connections[0]);
                }
            }
            modelContainer.Clear();
        }


        private static void DrawSimulationModelRecursive(TreeContainer.TreeContainer modelContainer, TreeContainer.TreeNode node, ProcessAction currentAction, out TreeContainer.TreeNode lastNode, Dictionary<string, TreeContainer.TreeNode> nodes)
        {
            TreeContainer.TreeNode currentContainer = node;
            TreeContainer.TreeNode nextContainer = node;
            lastNode = node;

            foreach (ProcessAction action in currentAction.ProcessActions)
            {
                TreeContainer.TreeNode nd = null;
                bool hidden = action.Title.StartsWith("hidden_");
                if (hidden)
                {
                    continue;
                }
                if (nodes.TryGetValue(action.Title, out nd))
                {
                    ((Border)nd.Content).Background = action.Color;
                    if (!lastNode.TreeChildren.Contains(node))
                    {
                        lastNode.TreeChildren.Add(node);
                    }
                }
                else
                {
                    Border guiElement = (Border)action.GUIElement;
                    guiElement.Background = action.Color;
                    nd = modelContainer.AddNode(guiElement, "__TreeNode" + ((uint)(action.Title.GetHashCode())).ToString(), node);
                    nodes.Add(action.Title, nd);
                }

                var cn = modelContainer.Connections.Where(x => x.IgnParent == node && x.IgnChild == nd).FirstOrDefault();
                if (cn.IgnChild == null)
                {
                    cn = new GraphLayout.TreeConnection(lastNode, nd, null);
                    modelContainer.Connections.Add(cn);
                }
                cn.Active = true;

                DrawSimulationModelRecursive(modelContainer, nd, action, out lastNode, nodes);
            }
        }

    }
}
