using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BBNGs.TraceLog;

namespace BBNGs.Graph
{
    [Serializable]
    public class Node
    {
        private static List<string> keys = new List<string>() { "lifecycle:transition" };
        private static Random r = new Random(2);

        /// <summary>
        /// Parent nodes of the node
        /// </summary>
        public List<Node> parentNodes;
        /// <summary>
        /// Child nodes of the node
        /// </summary>
        public List<Node> childNodes = new List<Node>();

        public Dictionary<string, Node> blockedPaths = new Dictionary<string, Node>();
        /// <summary>
        /// Nodes level in the tree conting from the root node.
        /// </summary>
        public int level = 0;
        /// <summary>
        /// Name of the node.
        /// </summary>
        public string name;
        /// <summary>
        /// Associated object's node.
        /// </summary>
        public List<Event> NodeObjects = new List<Event>();
        /// <summary>
        /// Nodes associated traces.
        /// </summary>
        public List<Trace> traces = new List<Trace>();
        /// <summary>
        /// Xml attribute that has attribute that we are interested in
        /// </summary>
        public string chosenExtractionAttribute = "lifecycle:transition";
        /// <summary>
        /// Value that we are interested in the extracted bayesian network
        /// </summary>
        public string chosenExtractionAttributeValue;
        /// <summary>
        /// Creates new node.
        /// </summary>
        /// <param name="parent">Parent node.</param>
        /// <param name="nameOfNode">Name of the node.</param>
        /// <param name="objectOfNode">Object describing this node.</param>         
        public Node(Node parent, string nameOfNode, Event objectOfNode, Trace t)
        {
            if (parent != null)
                parentNodes = new List<Node>() { parent };
            else
                parentNodes = new List<Node>();
            if (t != null)
                traces.Add(t);
            if (objectOfNode != null)
                NodeObjects.Add(objectOfNode);
            name = nameOfNode;
            if (parent != null)
                level = parent.level++;
            chosenExtractionAttribute = keys[r.Next(0, keys.Count)];
        }
        
        private Node(Node parent, string nameOfNode)
        {
            if (parent != null)
                parentNodes = new List<Node>() { parent };
            else
                parentNodes = new List<Node>();
            name = nameOfNode;
            chosenExtractionAttribute = keys[r.Next(0, keys.Count)];
        }

        public static Node CreateDummyNode(Node parentNode, string nameOfNode)
        {
            return new Node(parentNode, nameOfNode);
        }

        

        public void AppendNodeInfo(Trace t, Event nodeObject)
        {
            lock(traces)
            //    if (!traces.Contains(t))
                    traces.Add(t);
            lock(NodeObjects)
           //     if (!NodeObjects.Contains(nodeObject))
                    NodeObjects.Add(nodeObject);
        }

        /// <summary>
        /// Returns string describing the object.
        /// </summary>
        /// <returns>Returns Node.name</returns>
        public override string ToString()
        {
            return this.name;
        }
        /// <summary>
        /// Compares Node's name to another.
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>True if obj is Node and Node.name matches. False if obj is not Node. Returns base.Equals(obj) otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Node))
                return false;
            else if ((obj as Node).name == this.name)
                return true;
            else
                return base.Equals(obj);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>base.GetHashCode()</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// Gets edge list.
        /// </summary>
        /// <returns>List of directed edges in format "x,y"</returns>
        public List<string> GetEdges()
        {
            List<string> edges = new List<string>();
            foreach (Node n in parentNodes)
            {
                edges.Add(n.name + "##" + name);
            }
            return edges;
        }
        /// <summary>
        /// Checks if the node would loop in it's tree.
        /// </summary>
        /// <param name="nod">Node to compare</param>
        /// <returns>False if exists loop. Otherwise, false.</returns>
        public Node WouldLoop(Node nod, Trace t)
        {
            Node rastas = null;
            if (this.name == nod.name && this.traces.Contains(t))
                return this;
            var tevai = this.parentNodes.Where(x => x.traces.Contains(t));
            foreach (Node n in tevai)
            {
                rastas = n.WouldLoop(nod, t);
                if (rastas != null)
                {
                    return rastas;
                }

            }
            return rastas;
        }

        public Node WouldFullLoop(Node nod)
        {

            Node rastas = null;
            if (this.name == nod.name)
                return this;
            Stack<Node> nodes = new Stack<Node>();
            this.parentNodes.ForEach(x => nodes.Push(x));
            List<Node> completed = new List<Node>();

            while (nodes.Count > 0)
            {
                Node n = nodes.Pop();
                if (n.name == nod.name)
                    return n;
                if (completed.Contains(n))
                    continue;

                n.parentNodes.ForEach(x =>
                {
                    if (!nodes.Contains(x))
                        nodes.Push(x);
                });
            }
            return null;


            //Node rastas = null;
            //if (this.name == nod.name)
            //    return this;

            //    var tevai = this.parentNodes;
            //    foreach (Node n in tevai)
            //    {
            //        rastas = n.WouldFullLoop(nod);
            //        if (rastas != null)
            //        {
            //            return rastas;  
            //        }                  
            //    }
            //return rastas;
        }

        public List<string> GetFilteredParents(List<string> path)
        {
            List<string> parents = (from i in this.parentNodes where path.Contains(i.name) select i.name).ToList();

            if (parents.Count == 0)
                foreach (Node i in this.parentNodes)
                {
                    parents.AddRange(i.GetFilteredParents(path));
                }
            return parents;
        }

        public List<string> GetDirectParents(List<string> path)
        {

            var parents = (from i in this.parentNodes where path.Contains(i.name) select i.name);

            if (parents == null)
                parents = GetFilteredParents(path);

            if (parents == null)
                return new List<string>();
            else return parents.ToList();
        }

        public List<Node> GetAllParents()
        {
            if (this.parentNodes.Count == 0)
                return null;

            List<Node> parents = this.GetAllParentsRecursive(new List<Node>());

            return parents.Distinct().ToList();
        }

        public List<string> GetAllChildren()
        {

            List<string> names = new List<string>();
            names.Add(this.name);

            Queue<Node> cn = new Queue<Node>();
            this.childNodes.ForEach(x => cn.Enqueue(x));
            while (cn.Count > 0)
            {
                Node n = cn.Dequeue();
                if (names.Contains(n.name))
                {
                    continue;
                }
                else
                    names.Add(n.name);

                try
                {
                    n.childNodes.ForEach(x => cn.Enqueue(x));
                }
                catch (Exception exc) { Console.WriteLine(exc); }
            }
            return names;
        }

        public List<Node> GetAllChildrenNodes()
        {
            List<Node> nodes = new List<Node>();
            nodes.Add(this);

            Queue<Node> cn = new Queue<Node>();
            this.childNodes.ForEach(x => cn.Enqueue(x));
            while (cn.Count > 0)
            {
                Node n = cn.Dequeue();
                if (nodes.Contains(n))
                {
                    continue;
                }
                else
                    nodes.Add(n);

                try
                {
                    n.childNodes.ForEach(x => cn.Enqueue(x));
                }
                catch (Exception exc) { Console.WriteLine(exc); }
            }
            return nodes;
        }

        public List<Node> GetAllParentNodes()
        {
            List<Node> nodes = new List<Node>();
            nodes.Add(this);

            Queue<Node> cn = new Queue<Node>();
            this.parentNodes.ForEach(x => cn.Enqueue(x));
            while (cn.Count > 0)
            {
                Node n = cn.Dequeue();
                if (nodes.Contains(n))
                {
                    continue;
                }
                else
                    nodes.Add(n);

                try
                {
                    n.parentNodes.ForEach(x => cn.Enqueue(x));
                }
                catch (Exception exc) { Console.WriteLine(exc); }
            }
            return nodes;
        }

        private List<Node> GetAllParentsRecursive(List<Node> currentParentList)
        {
            if (this.parentNodes.Count == 0)
                return currentParentList;

            List<Node> parents = new List<Node>();
            parents.AddRange(this.parentNodes);
            foreach (Node parent in this.parentNodes)
                if (!currentParentList.Contains(parent))
                    parents.AddRange(parent.GetAllParentsRecursive(currentParentList));


            return parents.Distinct().ToList();
        }

        /// <summary>
        /// Gets the loop of the node.
        /// </summary>
        /// <param name="nod"></param>
        /// <returns>empty list if no loop found. Otherwise list of nodes for the loop path.</returns>
        public List<Node> GetLoop(Node nod, Trace t)
        {
            List<Node> path = new List<Node>();
            if (this.name == nod.name && this.traces.Contains(t))
                return new List<Node>() { this };

            foreach (Node n in this.parentNodes)
                if (n != null)
                {
                    List<Node> l = n.GetLoop(nod, t);
                    if (l.Count > 0)
                    {
                        path.Add(this);
                        path.AddRange(l);
                        break;
                    }
                }

            return path;
        }

        public List<Node> GetLoop(Node nod)
        {
            List<Node> path = new List<Node>();
            if (this.name == nod.name)
                return new List<Node>() { this };

            foreach (Node n in this.parentNodes)
                if (n != null)
                {
                    List<Node> l = n.GetLoop(nod);
                    if (l.Count > 0)
                    {
                        path.Add(this);
                        path.AddRange(l);
                        break;
                    }
                }

            return path;

        }

        public bool Equals(Node n)
        {
            if (this.name != n.name)
                return false;

            return true;
        }

        public void AddChildWithParentRelationship(Node n)
        {
            if (!this.childNodes.Contains(n))
                this.childNodes.Add(n);
            if (!n.parentNodes.Contains(this))
                n.parentNodes.Add(this);
        }

        public void RemoveChildParentRelationship(Node n)
        {
            this.parentNodes.Remove(n);
            n.childNodes.Remove(this);
            this.childNodes.Remove(n);
            n.parentNodes.Remove(this);
        }

        public void SkipNodeAsChild(Node n)
        {
            if (this == n)
                return;
            this.childNodes.Remove(n);

            n.parentNodes.Remove(this);
            while (n.childNodes.Count > 0)
            {
                Node x = n.childNodes[0];
                if (!this.childNodes.Contains(x))
                    this.childNodes.Add(x);
                if (!x.parentNodes.Contains(this))
                    x.parentNodes.Add(this);
                n.childNodes.Remove(x);
            }
        }

        public void SkipNodeAsParent(Node n)
        {
            if (this == n)
                return;
            this.parentNodes.Remove(n);
            n.childNodes.Remove(this);
            while (n.parentNodes.Count > 0)
            {
                Node x = n.parentNodes[0];
                if (!this.parentNodes.Contains(x))
                    this.parentNodes.Add(x);
                if (!x.childNodes.Contains(this))
                    x.childNodes.Add(this);
                n.parentNodes.Remove(x);
            }
        }

        public Node GetShortestPathToParent(List<Node> parents, List<Node> inPath)
        {
            foreach (Node p in parents)
                foreach (Node pp in this.parentNodes)
                    if (pp.name == p.name)
                        return this;


            foreach (Node p in this.parentNodes.Where(x => inPath.Contains(x)))
            {
                Node parent = p.GetShortestPathToParent(parents, inPath);
                if (parent != null)
                    return parent;
            }
            return null;

        }

        public Node GetOldestParent()
        {
            if (this.parentNodes.Count == 0)
                return this;
            else
                return this.parentNodes[0].GetOldestParent();
        }

        public bool CheckIfLoops(Tree t)
        {

            List<Node> foundNodes = new List<Node>();
            Stack<Node> n = new Stack<Node>();
            this.parentNodes.ForEach(x => n.Push(x));



            while (n.Count > 0)
            {
                Node nod = n.Pop();
                if (this == nod)
                    return true;

                if (!foundNodes.Contains(nod))
                {
                    nod.parentNodes.ForEach(x =>
                    {
                        if (!n.Contains(x)) n.Push(x);
                    });
                    foundNodes.Add(nod);
                }
            }
            return false;
        }

        public List<string> FindLoop()
        {
            List<string> resultingLoop = new List<string>();
            List<Node> foundNodes = new List<Node>();
            Stack<Node> n = new Stack<Node>();
            Stack<Node> path = new Stack<Node>();
            this.parentNodes.ForEach(x => n.Push(x));


            int hist = n.Count;
            Node nod = null;
            while (n.Count > 0)
            {
                nod = n.Pop();

                if (nod.parentNodes.Contains(this))
                {
                    path.Push(this);
                    break;
                }

                bool added = false;
                if (!foundNodes.Contains(nod))
                {
                    nod.parentNodes.ForEach(x =>
                    {
                        if (!n.Contains(x)) { n.Push(x); added = true; }
                    });
                    foundNodes.Add(nod);
                }
                if (added)
                    path.Push(nod);
                else if (n.Count < hist && path.Count > 0)
                    path.Pop();
            }

            if (n.Count > 0)
            {
                resultingLoop.Add(nod.name);
                while (path.Count > 0)
                    resultingLoop.Add(path.Pop().name);
                return resultingLoop;
            }

            return null;
        }

        public List<string> GetAttributeList()
        {
            return (from i in this.NodeObjects from j in i.EventVals.Keys select j).Distinct().ToList();
        }

        public Dictionary<string, int> GetValues()
        {
            if (this.name == "start_event")
            {
                return new Dictionary<string, int>() { { "No data", 0 }, { "data", 1 } };
            }

            List<string> vals = new List<string>();
            if (this.traces != null)
                vals.AddRange((from i in this.traces from j in i.events where j.fullName == this.name select j.Find(chosenExtractionAttribute)).Distinct().ToList());
            while (vals.Contains(null))
                vals.Remove(null);
            ;

            Dictionary<string, int> ats = new Dictionary<string, int>();

            // if (ats.Count == 0)
            ats.Add("No data", 0);
            for (int i = 1; i < vals.Count + 1; i++)
                ats.Add(vals[i - 1], i);


            return ats;
        }

    }
}
