using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BBNGs.TraceLog;
using BBNGs.Utilities;

namespace BBNGs.Graph
{
    [Serializable()]
    public class Tree
    {
        /// <summary>
        /// Root node of the tree
        /// </summary>
        public Node rootNode;
        /// <summary>
        /// Child nodes of the root Node
        /// </summary>
        public List<Node> childNodes { get { return rootNode.childNodes; } }
        /// <summary>
        /// Distinct nodes in the tree
        /// </summary>
        public Dictionary<string, Node> distinctNodes { get; private set; }
        /// <summary>
        /// Identified loops
        /// </summary>
        public List<List<string>> loops = new List<List<string>>();
        /// <summary>
        /// Alternate tree constructs conflicting with current tree.
        /// </summary>
        public List<Tree> alternateTrees = new List<Tree>();
        public List<Trace> unusedTraces = new List<Trace>();
        public List<Trace> usedTraces = new List<Trace>();
        public Node selectedNode;

        public SquareMatrix frequencyMatrix;
        public SquareMatrix setFrequencyMatrix;
        public SquareMatrix mustFollowMatrix;
        public SquareMatrix orMatrix;
        public SquareMatrix childParentMatrix;
        public SquareMatrix treeEdgesMatrix;
        public SquareMatrix treeParentPathMatrix;

        public Tree(SquareMatrix frequencyMatrix, SquareMatrix setFrequencyMatrix, SquareMatrix mustFollowMatrix,SquareMatrix orMatrix,SquareMatrix childParentMatrix)
        {


            distinctNodes = new Dictionary<string, Node>();
            foreach(string col in frequencyMatrix.columns.Keys)
            {
                distinctNodes.Add(col, Node.CreateDummyNode(null,col));
            }

            Node startNode = null;
            if (!distinctNodes.TryGetValue("start_event", out startNode))
            {
                startNode = Node.CreateDummyNode(null, "start_event");
                distinctNodes.Add("start_event", startNode);
            }
            this.rootNode = startNode;
            this.frequencyMatrix = frequencyMatrix;
            this.setFrequencyMatrix = setFrequencyMatrix;
            this.mustFollowMatrix = mustFollowMatrix;
            this.orMatrix = orMatrix;
            this.childParentMatrix = childParentMatrix;
            this.treeEdgesMatrix = setFrequencyMatrix.CopyEmpty();
            this.treeParentPathMatrix = setFrequencyMatrix.CopyEmpty();
        }

        public bool AppendNode(string parentNode, string childNode, Event childObject, Trace trace)
        {
            Node child = null;

            //look for existing child node. If failed it means a new node in the tree msut be created.
            if (!distinctNodes.TryGetValue(childNode, out child))
            {
                child = CreateNewTreeNode(null, childNode, childObject, trace);
            }
            child.AppendNodeInfo(trace, childObject);

            //if (parentNode == null)
            //{                
            //    return true;
            //}
            //if (setFrequencyMatrix.GetValue(childNode, childNode) < setFrequencyMatrix.AlphaLevel)
            //{
            //    return true;
            //}
            //try to add as root node. If successful, the parameters were for the root node and no more work needed.
            if (TryAddRootNode(parentNode, childNode, childObject, trace))
                return true;

            

            //select parent node
            Node parent = distinctNodes[parentNode];
            
            if (child.parentNodes.Contains(parent))
                return true;

            bool isChildParent = childParentMatrix.GetValue(parent.name, child.name) > 0;
            bool mustFollow = mustFollowMatrix.GetValue(parent.name, child.name) > 0;
            bool mustbeOr = orMatrix.GetValue(child.name, parent.name) > 0;


            if (!isChildParent || mustbeOr)
            {
                TryConnectNearestParent(parent, child,trace);
            }
            else
                parent.AddChildWithParentRelationship(child);

            return true;
        }

        private Node CreateNewTreeNode(Node parent, string childNode, Event childObject, Trace trace)
        {
            Node child = new Node(null, childNode, childObject, trace);
            distinctNodes.Add(child.name, child);
            return child;            
        }

        private void TryConnectNearestParent(Node parent, Node child,Trace t)
        {
            var orNodes = (from i in orMatrix.GetNonEmptyColumnRows(child.name,0) select i);
            var mustFollowNodes = (from i in mustFollowMatrix.GetNonEmptyColumnRows(child.name, 0) select i);
            var possibleParents = childParentMatrix.GetNonEmptyRowColumns(child.name, 0).Where(x => !orNodes.Contains(x) && !mustFollowNodes.Contains(x));//.Union(mustFollowMatrix.GetNonEmptyRowColumns(child.name, 0).Where(x => !orNodes.Contains(x)));


            Event bestMatchEvent = t.events.Where(x => childParentMatrix.GetValue(x.fullName,child.name) >0)/*  possibleParents.Contains(x.fullName))*/.OrderByDescending(x => x.timestamp).FirstOrDefault();
            if (bestMatchEvent != null)
            {
                Node pNode = distinctNodes[bestMatchEvent.fullName];
                pNode.AddChildWithParentRelationship(child);
                return;
            }

            bool found = false;
            Queue<Node> parentNodes = new Queue<Node>();
            parent.parentNodes.ForEach(x => parentNodes.Enqueue(x));
            List<string> checkedNodes = new List<string>();
            while (parentNodes.Count > 0)
            {
                Node pn = parentNodes.Dequeue();
                if (possibleParents.Contains(pn.name))
                {
                    found = true;
                    pn.AddChildWithParentRelationship(child);
                    treeEdgesMatrix.AddValue(parent.name, child.name, 1);
                    treeParentPathMatrix.AddValue(child.name, parent.name, 1);
                    break;
                }
                else
                {
                    checkedNodes.Add(pn.name);
                }

                pn.parentNodes.Where(x => !checkedNodes.Contains(x.name)).ToList().ForEach(x => parentNodes.Enqueue(x));
            }
            if (!found)
            {
                if (child.name == rootNode.name)
                    return;
                if (child.name == "start_event")
                {
                    this.rootNode.AddChildWithParentRelationship(child);
                }
                //this.rootNode.AddChildWithParentRelationship(child);
                // treeEdgesMatrix.AddValue(parent.name, child.name, 1);
                // treeParentPathMatrix.AddValue(child.name, parent.name, 1);
            }
        }

        private List<string> AddedRoots = new List<string>();
        private bool TryAddRootNode(string parentNode, string childNode, Event childObject, Trace trace)
        {
            //if no root node, this one is the root.
            if (rootNode == null)
            {
                AddRootNode(childNode, childObject, trace);
                return true;
            }

            //if no parent node and root node is not the parent node then creates pseudo root node
            if (parentNode == null)
            {
                if (AddedRoots.Contains(childNode))
                {

                }
                else
                {
                    CreateRootNode(childNode, childObject, trace);
                    AddedRoots.Add(childNode);
                }
                return true;
            }
            return false;
        }   

        /// <summary>
        /// Appends the node to the tree. Identifies loops and process variations.
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="childNode"></param>
        /// <param name="childObject"></param>
        /// <param name="trace"></param>
        public bool AppendNode_old(string parentNode, string childNode, Event childObject, Trace trace)
        {
         
            //if no root node, this one is the root.
            if (rootNode == null)
            {
                AddRootNode(childNode, childObject, trace);
            }


            //if no parent node and root node is not the parent node then throws conflict error
            else if (parentNode == null)
            {
                CreateRootNode(childNode, childObject, trace);
            }



            //if parent node exists in tree
            else if (distinctNodes.Keys.Contains(parentNode))
            {
                //select parent node
                Node parent = distinctNodes[parentNode];
                Node child = null;

                //look for existing child node or create new child upon failure to find one
                if (!distinctNodes.TryGetValue(childNode, out child))
                    child = new Node(parent, childNode, childObject, trace);

                //check for loops in trace instance
                Node loopingInstanceNode = parent.WouldLoop(child, trace);
                //check for loops in overall model
                Node loopingModelNode = parent.WouldFullLoop(child);

                if (loopingInstanceNode != null)
                    ExtractLoop(trace, parent, child);

                //if found loop in overall model create alternate tree
                if (loopingModelNode != null && loopingInstanceNode == null)
                {
                    bool foundParallel = false;

                    List<Node> loopPath = parent.GetLoop(child);
                    Node workingNode = parent.GetShortestPathToParent(child.parentNodes, loopPath);

                    if (workingNode == child || workingNode == parent)
                    {
                        return false;
                    }

                    Node intermediateNode = null;
                    for (int i = 0; i < loopPath.Count; i++)
                    {
                        if (loopPath[i] == workingNode)
                            intermediateNode = loopPath[i + 1];
                    }
                    if (workingNode != null && intermediateNode != null)
                    {
                        if (workingNode.parentNodes.Count > 1 && parent.childNodes.Count > 1)
                        {
                            workingNode.RemoveChildParentRelationship(intermediateNode);
                            BlockPath(workingNode, intermediateNode);
                            // parent.RemoveChildParentRelationship(intermediateNode);
                        }
                        //int index = trace.events.IndexOf((from b in parent.traces from c in b.events where b == trace && parent.NodeObjects.Contains(c) select c).FirstOrDefault());
                        //if (index > 1)
                        //{
                        //    int counter = 1;

                        //    while (trace.events.Count < index + counter - 1 && !AppendNode(parent.name, trace.events[index + counter + 1].name, trace.events[index + counter + 1], trace))
                        //        counter++;
                        //}
                    }
                    else
                    {
                        child.RemoveChildParentRelationship(loopPath[1]);
                        BlockPath(child, loopPath[1]);

                    }

                    if (!foundParallel && !child.blockedPaths.Keys.Contains(parent.name))
                    {
                        this.AddAlternateTree(trace);
                        return false;
                    }
                    else
                        this.usedTraces.Add(trace);

                }
                //if trace does not loop add node to the tree or append info to existing node
                else if (loopingInstanceNode == null)
                {
                    Node blocked = null;
                    if (!child.blockedPaths.TryGetValue(parentNode, out blocked))
                    {
                        if (!parent.childNodes.Contains(child))
                            parent.childNodes.Add(child);
                        if (!child.parentNodes.Contains(parent))
                            child.parentNodes.Add(parent);
                        if (!distinctNodes.Keys.Contains(childNode))
                            distinctNodes.Add(childNode, child);
                        if (!child.traces.Contains(trace))
                            child.traces.Add(trace);
                    }
                    if (!child.NodeObjects.Contains(childObject))
                        child.NodeObjects.Add(childObject);

                    if (!child.traces.Contains(trace))
                        child.traces.Add(trace);
                }
                //if found loop in trace instance, extract it, prevent the loop but leave elements. TODO: what if multiple loops? What if most traces contain loops?
                else
                {
                    ExtractLoop(trace, parent, child);
                    return false;
                }
            }




            //if all else fails, admit dumbness
            else if (parentNode != null && childNode != null)
            {
                CreateRootForMultipleRootNodes(childNode, childObject, trace);
            }
            else
                throw new Exception("Can't infer node object");

            
            return true;
        }

        private void CreateRootNode(string childNode, Event childObject, Trace trace)
        {
            if (rootNode.name != childNode)
            {
                //if the found root node is not the existing root node,
                //then we create "start_event" root node and append root  nodes as children for the new root node
                CreateRootForMultipleRootNodes(childNode, childObject, trace);

            }

            Node child = distinctNodes[childNode];

            if (!child.NodeObjects.Contains(childObject))
                child.NodeObjects.Add(childObject);

            if (!child.traces.Contains(trace))
                child.traces.Add(trace);
        }

        private static void BlockPath(Node workingNode, Node intermediateNode)
        {
            Node val = null;
            if (!workingNode.blockedPaths.TryGetValue(intermediateNode.name, out val))
                workingNode.blockedPaths.Add(intermediateNode.name, intermediateNode);
            if (!intermediateNode.blockedPaths.TryGetValue(workingNode.name, out val))
                intermediateNode.blockedPaths.Add(workingNode.name, workingNode);
        }

        public bool CheckIfTraceWouldLoop(Trace t)
        {
            Event lastEvent = null;
            foreach (Event e in t.events)
            {
                if (lastEvent == null)
                {
                    if (this.CheckIfChildWouldLoop(null, e.fullName))
                        return true;
                    lastEvent = e;
                }
                else
                {
                    if (this.CheckIfChildWouldLoop(lastEvent.fullName, e.fullName))
                        return true;
                    lastEvent = e;
                }
            }
            return false;
        }

        public bool CheckIfChildWouldLoop(string parentNode, string childNode)
        {
            if (parentNode == null)
                return false;

            Node parent = null;
            Node child = null;
            if (this.distinctNodes.TryGetValue(parentNode, out parent) && this.distinctNodes.TryGetValue(childNode, out child))
                return false;

            if (parent == null)
                return false;

            child = Node.CreateDummyNode(parent, childNode);

            return parent.WouldFullLoop(child) == null ? false : true;
        }

        /// <summary>
        ///  Create "start_event" root node and append root nodes as children for the new root node
        /// </summary>
        /// <param name="childNode"></param>
        /// <param name="childObject"></param>
        /// <param name="trace"></param>
        private void CreateRootForMultipleRootNodes(string childNode, Event childObject, Trace trace)
        {
            if (this.rootNode.name == "start_event")
                AppendNode("start_event", childNode, childObject, trace);
            else
            {
                Node oldRoot = rootNode;
                Node newRoot = null;
                if(!distinctNodes.TryGetValue("start_event",out newRoot))
                {
                    newRoot = new Node(null, "start_event", null, null);
                    distinctNodes.Add(newRoot.name, newRoot);
                }
                newRoot.childNodes.Add(oldRoot);
                oldRoot.parentNodes = new List<Node>() { newRoot };
                rootNode = newRoot;
                AppendNode("start_event", childNode, childObject, trace);
            }
        }

        /// <summary>
        /// Creates alternate tree if found loop in overall trace model
        /// </summary>
        /// <param name="trace">conflicting trace</param>
        private void AddAlternateTree(Trace trace)
        {
            if (!unusedTraces.Contains(trace))
                unusedTraces.Add(trace);
            //remove all instances of the trace in current model
            var trinti = (from i in distinctNodes.Values where i.traces.Contains(trace) select i);
            trinti.ToList().ForEach(x =>
            {
                if (x.traces.Count > 1)
                {
                    var trinami = (from j in x.NodeObjects where j.trace == trace select j);
                    trinami.ToList().ForEach(y => x.NodeObjects.Remove(y));
                    x.traces.Remove(trace);
                }
                else
                {
                    while(x.childNodes.Count >0)
                    {
                        x.childNodes[0].RemoveChildParentRelationship(x);
                    }
                    while(x.parentNodes.Count>0)
                    {
                        x.parentNodes[0].RemoveChildParentRelationship(x);
                    }
                    distinctNodes.Remove(x.name);
                }
            });


            //check if the trace can be appended anywhere
            foreach (Tree t in alternateTrees)
            {
                if (!t.CheckIfTraceWouldLoop(trace))
                {
                    //AppendTraceToTree(t, trace);
                    return;
                }
            }

            //create alternate Tree

            Tree alternate = CreateTree(new List<Trace> { trace },null,null,null,null,null);
            Tree alternateTreeClone = null;

            for (int i = 0; i < alternateTrees.Count; i++)
                if (alternate.IsSame(alternateTrees[i]))
                {
                    alternateTreeClone = alternateTrees[i];
                    break;
                }

            if (alternateTreeClone != null)
            {
                AppendTraceToTree(alternateTreeClone, trace);
            }
            else
            {
                alternateTrees.Add(alternate);
            }

        }

        /// <summary>
        /// Extracts loop in trace instance from current trace model
        /// </summary>
        /// <param name="trace">working trace</param>
        /// <param name="parent">first element in loop</param>
        /// <param name="child">last element in loop</param>
        private void ExtractLoop(Trace trace, Node parent, Node child)
        {
            //finds path of the loop
            List<Node> kelias = parent.GetLoop(child, trace);

            //instantiate loop as tree
            //Tree t = new Tree();
            ////add root node to the loop path tree
            //t.AppendNode(null, kelias[0].name, (from i in kelias[0].NodeObjects where i.trace == trace select i).FirstOrDefault(), trace);

            ////append in-path elements
            //for (int i = 1; i < kelias.Count; i++)
            //{
            //    Event parentNode = (from j in kelias[i - 1].NodeObjects where j.trace == trace select j).FirstOrDefault();

            //    t.AppendNode(
            //        parentNode == null ? null : parentNode.name,
            //        kelias[i - 1].name,
            //        (from j in kelias[i].NodeObjects where j.trace == trace select j).FirstOrDefault(),
            //        trace);
            //}
            //last element of the loop is the loop
            ////t.AppendNode(
            ////    kelias[kelias.Count - 1].name,
            ////    child.name,
            ////    (from i in child.NodeObjects where i.trace == trace select i).FirstOrDefault(),
            ////    trace);

            //extract loop path from the tree
            //List<string> loopPath = GetLoopPath(t);
            List<string> loopPath = new List<string>();
            loopPath.Add(kelias[kelias.Count - 1].name);
            kelias.ForEach(x =>
                loopPath.Add(x.name));
            loopPath.RemoveAt(loopPath.Count - 1);

            bool rasta = false;
            //look for existing loop in loops
            foreach (List<string> l in loops)
            {
                //some heuristics
                if (loopPath.Count != l.Count)
                    continue;

                bool suits = true;
                //compare loop sequence
                for (int i = 0; i < loopPath.Count; i++)
                {
                    if (loopPath[i] != l[i])
                    {
                        suits = false;
                        break;
                    }
                }
                //no differences found, means identical loop
                if (suits)
                {
                    rasta = true;
                    break;
                }
            }
            if (!rasta)
                loops.Add(loopPath);
        }

        /// <summary>
        /// Adds root node
        /// </summary>
        /// <param name="childNode">name of the root node</param>
        /// <param name="childObject">associated object of the root node</param>
        /// <param name="trace">current working trace</param>
        private void AddRootNode(string childNode, Event childObject, Trace trace)
        {
            Node possibleRootNode = null;
            if (distinctNodes.TryGetValue(childNode, out possibleRootNode))
            {
                rootNode = possibleRootNode;
            }
            else
            {
                rootNode = new Node(null, childNode, childObject, trace);
                distinctNodes.Add(rootNode.name, rootNode);
            }
        }
        
        /// <summary>
        /// Extracts loop path from the tree
        /// </summary>
        /// <param name="t">Tree from which to extract the tree</param>
        /// <returns>Sequential list of nodes reflecting the loop.</returns>
        private static List<Node> GetLoopPath(Tree t)
        {
            List<Node> path = new List<Node>();
            path.Add(t.rootNode);
            Dictionary<string, Node> pathNodes = new Dictionary<string, Node>();
            foreach (KeyValuePair<string, Node> n in t.distinctNodes)
                pathNodes.Add(n.Key, n.Value);
            pathNodes.Remove(t.rootNode.name);

            Node tmp = t.rootNode;
            while (pathNodes.Count > 0)
            {
                if (tmp.childNodes.Count > 0)
                {
                    path.Add(tmp.childNodes[0]);
                    tmp = tmp.childNodes[0];
                    pathNodes.Remove(tmp.name);
                }
                else
                {
                    tmp = t.distinctNodes.ElementAt(0).Value;
                    pathNodes.Remove(tmp.name);
                }
            }
            return path;
        }

        public List<string> GetEdges()
        {
            List<string> edges = new List<string>();

            foreach (Node n in distinctNodes.Values)
            {
                edges.AddRange(n.GetEdges());
            }
            return edges;
        }

        public Dictionary<string, Node> GetTracesOfPath(List<string> path)
        {
            Dictionary<string, Node> ats = new Dictionary<string, Node>();
            foreach (string s in path)
            {
                Node val;
                if (!distinctNodes.TryGetValue(s, out val))
                    continue;
                //return null;
                if (ats.Keys.Contains(s))
                    return null;
                ats.Add(s, val);
            }
            return ats;
        }
        
        public static Tree CreateTree(List<Trace> traces, SquareMatrix freqM, SquareMatrix setFreqM, SquareMatrix mustFM, SquareMatrix orM, SquareMatrix cpM)
        {
            int i = 0;
            Console.WriteLine();
            Tree traceTree = new Tree(freqM, setFreqM, mustFM, orM, cpM);

            //foreach(var col in mustFM.columns)
            //{
            //    foreach(var row in mustFM.rows)
            //    {
            //        cpM.AddValue(col.Key, row.Key, cpM[col.Value][row.Value]+mustFM[col.Value][row.Value]);
            //    }
            //}

            Dictionary<string, SquareMatrix> matrixes = new Dictionary<string, SquareMatrix>();

            matrixes.Add("sequenceFrequency", freqM);
            matrixes.Add("setFrequency", setFreqM);
            matrixes.Add("mustFollow", mustFM);
            matrixes.Add("orMatrix", orM);
            matrixes.Add("childParent", cpM);

            Utility.OutputMatrix(matrixes, "matrix stats.csv");

            foreach (Trace t in traces)
            {
                i++;
                if(i%250==0)
                    Console.WriteLine(i);
                AppendTraceToTree(traceTree, t);
            }
            Console.WriteLine(i);

            //for (int j = 0; j < traceTree.unusedTraces.Count; j++)
            //{
            //    AppendTraceToTree(traceTree, traceTree.unusedTraces[j]);
            //}
            
            return traceTree;
        }

        public static void AppendTraceToTree(Tree traceTree, Trace t)
        {
            Event lastEvent = null;

            for (int i = 0; i < t.events.Count; i++)
            {
                Event e = t.events[i];
                if (lastEvent == null)
                {
                    if (!traceTree.AppendNode(null, e.fullName, e, t))
                    {
                        //RemoveTraceFromTree(traceTree, t);
                        //break;
                    }
                    lastEvent = e;
                }
                else
                {
                    int idx = 1;
                    while (lastEvent.fullName == e.fullName && i - idx >= 0)
                    {
                        lastEvent = t.events[i - idx];
                        idx++;
                    }

                    if (!traceTree.AppendNode(lastEvent.fullName, e.fullName, e, t))
                    {
                        //RemoveTraceFromTree(traceTree, t);
                        //break;
                    }
                    lastEvent = e;
                }

            }

        }

        public static void RemoveTraceFromTree(Tree traceTree, Trace t)
        {
            traceTree.distinctNodes.Values.ToList().ForEach(x =>
            {

                for (int i = 0; i < x.NodeObjects.Count; i++)
                {
                    if (x.NodeObjects[i].trace == t)
                    {
                        x.NodeObjects.Remove(x.NodeObjects[i]);
                        break;
                    }
                }
            });

        }

        public bool IsSame(Tree t)
        {
            foreach (Node n in distinctNodes.Values)
            {
                Node foundNode;
                if (!t.distinctNodes.TryGetValue(n.name, out foundNode))
                    return false;


                if (!CompareNodeLists(n.parentNodes, foundNode.parentNodes))
                    return false;
                if (!CompareNodeLists(n.childNodes, foundNode.childNodes))
                    return false;
            }
            return true;
        }

        private static bool CompareNodeLists(List<Node> nNodes, List<Node> xNodes)
        {

            if (nNodes == null && xNodes != null || nNodes != null && xNodes == null)
                return false;

            if (nNodes == null & xNodes == null)
                return true;

            for (int i = 0; i < nNodes.Count; i++)
            {
                bool found = false;
                for (int j = 0; j < xNodes.Count; j++)
                {
                    if (nNodes[i].Equals(xNodes[j]))
                        found = true;
                }
                if (!found)
                    return false;
            }
            return true;
        }
        
        public List<string> OutputGraphs()
        {
            List<Tree> outputGraphs = new List<Tree>();
            List<string> fileNames = new List<string>();
            outputGraphs.Add(this);
            this.alternateTrees.ForEach(x => outputGraphs.Add(x));
            for (int i = 0; i < outputGraphs.Count; i++)
            {
                string fileName = Environment.CurrentDirectory + @"\graph" + i + ".gv";
                fileNames.Add(fileName);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(fileName);
                List<string> edges = outputGraphs[i].GetEdges();
                List<string> updatedEdges = new List<string>();
                edges.ForEach(x => updatedEdges.Add(x.Replace("##", "->").Replace(",","").Replace("(","").Replace(")","") + ";"));

                sw.WriteLine("digraph " + i + " {");
                updatedEdges.ForEach(x => sw.WriteLine(x));
                sw.WriteLine("}");
                sw.Close();
            }
            return fileNames;
        }


        public List<List<string>> FindLoops()
        {
            List<List<string>> loops = new List<List<string>>();
            foreach (Node n in distinctNodes.Values)
            {
                List<string> loop = n.FindLoop();

                if (loop != null)
                {
                    loops.Add(loop);

                    List<Node> loopingNodes = new List<Node>();
                    foreach (string loopingNodeName in loop)
                        loopingNodes.Add(distinctNodes[loopingNodeName]);

                    if (loop.Count == 1)
                    {
                        Node on = distinctNodes[loop[0]];
                        double ion = setFrequencyMatrix.GetValue(n.name, on.name);
                        double oin = setFrequencyMatrix.GetValue(on.name, n.name);

                        if (oin > ion)
                        {
                            n.childNodes.Remove(on);
                            on.parentNodes.Remove(n);
                        }
                        else
                        {
                            on.childNodes.Remove(on);
                            n.parentNodes.Remove(on);
                        }
                    }
                    else
                    {
                        Node on = distinctNodes[loop[0]];
                        on.RemoveChildParentRelationship(n);
                    }
                   
                }
            }
            return loops;
        }

        public Event ReplayTrace(Trace t)
        {
            List<Node> ActivatedNodes = new List<Node>();
            Node startNode = null;

           if (distinctNodes.TryGetValue("start_event", out startNode))
                ActivatedNodes.Add(startNode);


            for (int i = 0; i < t.events.Count;i++)
            {
                Event e = t.events[i];

                Node n = distinctNodes[e.fullName];

                if (i == 0)
                {
                    ActivatedNodes.Add(n);
                    continue;
                }

                var actNs = ActivatedNodes.Where(x => x.childNodes.Contains(n) || x == n);
                Node actN = null;
                foreach(Node aN in actNs)
                {
                    if(aN.name.StartsWith("start_event"))
                    {
                        continue;
                    }
                    if (orMatrix.GetValue(aN.name, n.name) > 0)
                    {
                        actN = aN;
                        break;
                    }

                }

                //if(actN ==null)
                //{
                //    if (n.parentNodes.Count == 0)
                //        actN = n;
                //}

                if (actN != null)
                {
                    return e;
                }

                ActivatedNodes.Add(n);
            }
            
            return null;
        }

    }




}
