//public PredictionResults TracePredictionStepByStep2(Trace t)
//{
//    string eventTemplate = "Event:{0} has prob to occur {1}, with best next event {2}={3} and selection is correct={4}";
//    foreach (string nd in traceTree.distinctNodes.Keys)
//    {
//        ClearObservation(nd);
//    }

//    List<KeyValuePair<string, int>> result = new List<KeyValuePair<string, int>>();
//    PredictionResults res = new PredictionResults();
//    result.Add(new KeyValuePair<string, int>("Checking trace:" + string.Join(";", t.events.Select(x => x.name)), 0));

//    List<Node> ActivatedNodes = new List<Node>();

//    HashSet<Node> badNodes = new HashSet<Node>();

//    ActivatedNodes.Add(traceTree.distinctNodes["start_event"]);
//    ClearObservation("start_event");
//    ObserveVariable("start_event", "occured", "1");



//    for (int i = 0; i < t.events.Count - 1; i++)
//    {
//        Event e = t.events[i];

//        Node n = traceTree.distinctNodes[e.fullName];
//        ActivatedNodes.Add(n);

//        traceTree.orMatrix.GetNonEmptyRowColumns(n.name, 0).ForEach(x =>
//        {
//            if (!badNodes.Contains(traceTree.distinctNodes[x]))
//            {
//                badNodes.Add(traceTree.distinctNodes[x]);
//            }
//        });


//        CPT cpt = cpts[n];

//        double chance = cpts[n].ProbOfOccurence();

//        foreach (var eVal in e.EventVals)
//        {
//            ObserveVariable(n.name, eVal.Key, eVal.Value.value);
//        }
//        ObserveVariable(n.name, "occured", "1");


//        double bestCurrVal = 0;

//        Node bestNextChoice = null;

//        Node actual = traceTree.distinctNodes[t.events[i + 1].fullName];

//        double proboc = cpts[actual].ProbOfOccurence();//cpts[actual].WhatIf(null,null,null);

//        var ppp = new List<Tuple<string, string, string>>() {
//                        new Tuple<string,string,string>(n.name,"occured","1"),
//                        new Tuple<string,string,string>(n.name,"previous","1")
//                    };
//        var ppo = new List<Tuple<string, string, string>>() {
//                        new Tuple<string,string,string>(n.name,"occured","1"),
//                        new Tuple<string,string,string>(n.name,"previous","-2")
//                    };
//        double probprev = cpts[actual].WhatIf3(ppp, false, ppo, false) * proboc;
//        bool partial = false;
//        if (probprev > 0.8)
//        {
//            bestNextChoice = actual;
//            bestCurrVal = proboc > probprev ? proboc : probprev;

//        }
//        else if (proboc > 0.8)
//        {
//            bestNextChoice = actual;
//            bestCurrVal = proboc > probprev ? proboc : probprev;
//            partial = true;


//        }
//        else
//        {

//            var possibleNodes = n.childNodes.Union(from k in ActivatedNodes
//                                                   from j in k.childNodes
//                                                   where
//                                                       !ActivatedNodes.Contains(j)
//                                                       && !badNodes.Contains(j)
//                                                       && !n.childNodes.Contains(j)
//                                                   select j).ToList();
//            var chs = possibleNodes.Select(x =>
//            {
//                return new { prob = cpts[x].WhatIf3(ppp, false, ppo, false), node = x };// cpts[x].ProbOfOccurence(), node = x };
//            }).OrderByDescending(x => x.prob).ToList();

//            int indx = 0;
//            for (int k = 0; k < chs.Count; k++)
//            {
//                if (chs[k].node == actual)
//                {
//                    indx = k;
//                    break;
//                }
//            }
//            if (((indx == 0 /*&& chs.Count <=2) || (indx <= chs.Count / 10 && chs.Count>2))*/ && chs[indx].prob > 0 && !double.IsNaN(chs[indx].prob) && !double.IsInfinity(chs[indx].prob))))
//            {
//                bestNextChoice = actual;
//                bestCurrVal = chs[indx].prob;
//            }
//            else
//            {
//                chs = possibleNodes.Select(x =>
//                {
//                    return new { prob = cpts[x].ProbOfOccurence(), node = x };// cpts[x].ProbOfOccurence(), node = x };
//                }).OrderByDescending(x => x.prob).ToList();

//                indx = 0;
//                for (int k = 0; k < chs.Count; k++)
//                {
//                    if (chs[k].node == actual)
//                    {
//                        indx = k;
//                        break;
//                    }
//                }
//                if (((indx == 0/* && chs.Count <= 2) || (indx <= chs.Count / 10 && chs.Count > 2))*/ && chs[indx].prob > 0 && !double.IsNaN(chs[indx].prob) && !double.IsInfinity(chs[indx].prob))))
//                {
//                    bestNextChoice = actual;
//                    bestCurrVal = chs[indx].prob;
//                    partial = true;
//                }
//            }
//        }



//        bool correctChoice = false;
//        if (bestNextChoice != null)
//        {
//            if (i < t.events.Count - 1 && bestNextChoice.name == t.events[i + 1].fullName)
//            {
//                correctChoice = true;
//            }
//        }

//        bool partialCorrect = false;
//        if (!correctChoice && bestNextChoice != null)
//        {
//            if (t.events.Where(x => x.fullName == bestNextChoice.name).FirstOrDefault() != null)
//            {
//                partialCorrect = true;
//            }
//        }

//        res.nodes.Add(n);
//        Node nextNode = null;
//        if (traceTree.distinctNodes.TryGetValue(t.events[i + 1].fullName, out nextNode))
//        { }
//        res.results.Add(new KeyValuePair<Node, PredictionResult>(n,
//            new PredictionResult()
//            {
//                bestNextChoice = bestNextChoice,
//                current = n,
//                next = nextNode,
//                realNextProb = nextNode == null ? 0 : cpts[nextNode].ProbOfOccurence(),
//                currentProb = cpts[n].ProbOfOccurence(),
//                nextProb = bestNextChoice != null ? cpts[bestNextChoice].ProbOfOccurence() : double.NaN,
//                correct = correctChoice ^ partial,
//                failedPredictionWithOccurence = partialCorrect || partial
//            }));

//        if (i > 0 && i < t.events.Count - 1)
//        {
//            result.Add(
//                new KeyValuePair<string, int>(String.Format(eventTemplate, e.fullName, chance.ToString(), bestNextChoice == null ? "None" : bestNextChoice.name, bestCurrVal, correctChoice), correctChoice ? 1 : 0));
//        }
//    }
//    return res;// result;
//}

//public PredictionResults TracePredictionStepByStep1(Trace t)
//{
//    #region prediction initialisation
//    string eventTemplate = "Event:{0} has prob to occur {1}, with best next event {2}={3} and selection is correct={4}";
//    //clear current observations since this is a new observation
//    foreach (string nd in traceTree.distinctNodes.Keys)
//    {
//        ClearObservation(nd);
//    }

//    List<KeyValuePair<string, int>> result = new List<KeyValuePair<string, int>>();
//    PredictionResults res = new PredictionResults();
//    result.Add(new KeyValuePair<string, int>("Checking trace:" + string.Join(";", t.events.Select(x => x.name)), 0));

//    List<Node> ActivatedNodes = new List<Node>();

//    HashSet<Node> badNodes = new HashSet<Node>();

//    //initialise to notice start_event
//    ActivatedNodes.Add(traceTree.distinctNodes["start_event"]);
//    ClearObservation("start_event");
//    ObserveVariable("start_event", "occured", "1");

//    #endregion

//    var tetra = 0;
//    for (int i = 0; i < t.events.Count - 1; i++)
//    {
//        Event e = t.events[i];

//        //mark this event as activated
//        Node n = traceTree.distinctNodes[e.fullName];
//        ActivatedNodes.Add(n);

//        //get all nodes that cannot occur together with this node
//        traceTree.orMatrix.GetNonEmptyRowColumns(n.name, 0).ForEach(x =>
//        {
//            if (!badNodes.Contains(traceTree.distinctNodes[x]))
//            {
//                badNodes.Add(traceTree.distinctNodes[x]);
//            }
//        });


//        //mark this node as observed
//        CPT cpt = cpts[n];
//        double chance = cpts[n].ProbOfOccurence();
//        foreach (var eVal in e.EventVals)
//        {
//            ObserveVariable(n.name, eVal.Key, eVal.Value.value);
//        }
//        ObserveVariable(n.name, "occured", "1");

//        double bestCurrVal = 0;
//        Node bestNextChoice = null;
//        //select possible further nodes 
//        var possibleNodes = n.childNodes.Union(from k in ActivatedNodes
//                                               from j in k.childNodes
//                                               where
//                                                   !ActivatedNodes.Contains(j)
//                                                   && !badNodes.Contains(j)
//                                                   && !n.childNodes.Contains(j)
//                                               select j);


//        //for each of the possible nodes
//        foreach (Node childNode in possibleNodes)
//        {

//            var cnCpt = cpts[childNode];

//            double p = cnCpt.ProbOfOccurence();
//            //var pnc = 

//            var nPrev = cnCpt.WhatIf(n.name, "previous", "1", true);
//            var nnPrev = cnCpt.WhatIf(n.name, "previous", "-1", true);
//            if (double.IsNaN(nPrev) && !double.IsNaN(nnPrev))
//            {
//                nPrev = 0;
//            }
//            else if (!double.IsNaN(nPrev) && double.IsNaN(nnPrev))
//            {
//                nPrev = 1;
//            }
//            else if (double.IsNaN(nPrev) && double.IsNaN(nnPrev))
//            {
//                nPrev = cnCpt.NonCausalProbOfOccurence();
//            }
//            else
//            {
//                nPrev = nPrev / (nPrev + nnPrev);
//            }
//            //nPrev = double.IsInfinity(nPrev) ? 1 : (double.IsNaN(nPrev) ? p: nPrev);
//            if (p > 0 && double.IsNaN(nPrev))
//            {
//                p = cnCpt.NonCausalProbOfOccurence();
//            }
//            else
//            {
//                p *= nPrev;
//            }

//            //List<Tuple<string, string, string>> val = new List<Tuple<string, string, string>>();
//            //val.Add(new Tuple<string, string, string>(n.name, "previous", "1"));
//            //childNode.childNodes.ForEach(x =>
//            //{
//            //    if (childNode.parentNodes.Contains(x))
//            //    {
//            //        val.Add(new Tuple<string, string, string>(x.name, "occured", "1"));
//            //    }
//            //});
//            //p *= cpts[childNode].WhatIf2(val);

//            if (p > bestCurrVal)
//            {
//                bestNextChoice = childNode;
//                bestCurrVal = p;
//            }


//        }

//        if (i == 0 && bestNextChoice != traceTree.distinctNodes[t.events[1].fullName])
//        {
//            bestNextChoice = null;
//            bestCurrVal = 0;
//        }


//        #region add results
//        bool correctChoice = false;
//        if (bestNextChoice != null)
//        {
//            if (bestNextChoice.name == t.events[i + 1].fullName)
//            {
//                correctChoice = true;
//            }

//        }


//        bool partialCorrect = false;
//        if (!correctChoice && bestNextChoice != null)
//        {
//            if (t.events.Where(x => x.fullName == bestNextChoice.name).FirstOrDefault() != null)
//            {
//                partialCorrect = true;
//            }
//        }

//        res.nodes.Add(n);
//        Node nextNode = null;
//        if (traceTree.distinctNodes.TryGetValue(t.events[i + 1].fullName, out nextNode))
//        { }
//        res.results.Add(new KeyValuePair<Node, PredictionResult>(n,
//            new PredictionResult()
//            {
//                bestNextChoice = bestNextChoice,
//                current = n,
//                next = nextNode,
//                realNextProb = nextNode == null ? 0 : cpts[nextNode].ProbOfOccurence(),
//                currentProb = cpts[n].ProbOfOccurence(),
//                nextProb = bestNextChoice != null ? cpts[bestNextChoice].ProbOfOccurence() : double.NaN,
//                correct = correctChoice,
//                failedPredictionWithOccurence = partialCorrect
//            }));

//        if (i > 0 && i < t.events.Count - 1)
//        {
//            result.Add(
//                new KeyValuePair<string, int>(String.Format(eventTemplate, e.fullName, chance.ToString(), bestNextChoice == null ? "None" : bestNextChoice.name, bestCurrVal, correctChoice), correctChoice ? 1 : 0));
//        }


//        #endregion
//    }
//    return res;// result;
//}

//public PredictionResults TracePredictionStepByStep3(Trace t)
//{
//    string eventTemplate = "Event:{0} has prob to occur {1}, with best next event {2}={3} and selection is correct={4}";
//    //clear current observations since this is a new observation
//    foreach (string nd in traceTree.distinctNodes.Keys)
//    {
//        ClearObservation(nd);
//    }

//    List<KeyValuePair<string, int>> result = new List<KeyValuePair<string, int>>();
//    PredictionResults res = new PredictionResults();
//    result.Add(new KeyValuePair<string, int>("Checking trace:" + string.Join(";", t.events.Select(x => x.name)), 0));

//    List<Node> ActivatedNodes = new List<Node>();

//    HashSet<Node> badNodes = new HashSet<Node>();

//    //initialise to notice start_event
//    ActivatedNodes.Add(traceTree.distinctNodes["start_event"]);
//    ClearObservation("start_event");
//    ObserveVariable("start_event", "occured", "1");


//    for (int i = 0; i < t.events.Count - 1; i++)
//    {
//        Event e = t.events[i];

//        //mark this event as activated
//        Node n = traceTree.distinctNodes[e.fullName];
//        ActivatedNodes.Add(n);

//        //get all nodes that cannot occur together with this node
//        traceTree.orMatrix.GetNonEmptyRowColumns(n.name, 0).ForEach(x =>
//        {
//            if (!badNodes.Contains(traceTree.distinctNodes[x]))
//            {
//                badNodes.Add(traceTree.distinctNodes[x]);
//            }
//        });


//        //mark this node as observed
//        CPT cpt = cpts[n];
//        double chance = cpts[n].ProbOfOccurence();
//        foreach (var eVal in e.EventVals)
//        {
//            ObserveVariable(n.name, eVal.Key, eVal.Value.value);
//        }
//        ObserveVariable(n.name, "occured", "1");

//        double bestCurrVal = 0;

//        Node bestNextChoice = null;
//        //select possible further nodes 
//        var possibleNodes = n.childNodes.Union(from k in ActivatedNodes
//                                               from j in k.childNodes
//                                               where
//                                                   !ActivatedNodes.Contains(j)
//                                                   && !badNodes.Contains(j)
//                                                   && !n.childNodes.Contains(j)
//                                               select j);
//        List<KeyValuePair<Node, double>> choices = new List<KeyValuePair<Node, double>>();


//        Dictionary<Node, KeyValuePair<double, double>> probs = new Dictionary<Node, KeyValuePair<double, double>>();

//        //for each of the possible nodes
//        foreach (Node childNode in possibleNodes)
//        {

//            double nodeChance = cpts[childNode].WhatIf(n.name, "previous", "1", false);
//            double bestVal = 0;

//            //find the max P(cnn |n[occured],possibleNode[occured,previous])
//            foreach (Node cnn in childNode.childNodes)
//            {
//                double nnch = 0;
//                List<Tuple<string, string, string>> val = new List<Tuple<string, string, string>>();
//                val.Add(new Tuple<string, string, string>(childNode.name, "previous", "1"));
//                ActivatedNodes.ForEach(x =>
//                {
//                    if (cnn.parentNodes.Contains(x))
//                    {
//                        val.Add(new Tuple<string, string, string>(x.name, "occured", "1"));
//                        val.Add(new Tuple<string, string, string>(childNode.name, "previous", "-1"));
//                    }
//                });
//                nnch = cpts[cnn].WhatIf2(val);

//                if (nnch > bestVal)
//                {
//                    bestVal = nnch;
//                }
//            }

//            //alternative calculation - check the probability for this node being the next one if it happens

//            double alt = nodeChance * cpts[childNode].WhatIf(n.name, "previous", "1", true) / cpts[childNode].WhatIf(null, null, null, true);
//            if (childNode.childNodes.Count == 0)//|| alt > bestVal)
//            {
//                bestVal = alt;
//            }
//            //double nodeChance = cpts[childNode].WhatIf(n.name, "previous", "1",false);
//            double notChance = cpts[childNode].WhatIf(n.name, "previous", "-1", true) / (cpts[childNode].Total);
//            if (double.IsInfinity(notChance) && !childNode.name.Contains("register"))
//            {
//                notChance = 1;
//                nodeChance = 0;
//            }
//            //nodeChance = nodeChance * (1 - notChance);

//            probs.Add(childNode, new KeyValuePair<double, double>(nodeChance, notChance));

//        }
//        double bestCurrNotVal = 0;
//        foreach (var val in probs)
//        {
//            double totalnot = 1;
//            totalnot = val.Value.Key;
//            if (double.IsNaN(totalnot))
//            {
//                totalnot = cpts[val.Key].NonCausalProbOfOccurence();
//            }

//            if (totalnot > bestCurrVal)
//            {
//                bestNextChoice = val.Key;
//                bestCurrVal = totalnot;
//                bestCurrNotVal = val.Value.Value;
//            }
//            else if (totalnot == bestCurrVal)
//            {
//                if (val.Value.Value < bestCurrNotVal)// && !double.IsInfinity(val.Value.Value))
//                {
//                    bestNextChoice = val.Key;
//                    bestCurrVal = totalnot;
//                    bestCurrNotVal = val.Value.Value;
//                }
//            }
//            choices.Add(new KeyValuePair<Node, double>(val.Key, totalnot));
//        }


//        //Dictionary<Node, KeyValuePair<double, double>> chances = new Dictionary<Node, KeyValuePair<double, double>>();
//        //double currProb = 0;
//        //foreach (Node bn in possibleNodes)
//        //{
//        //    double prob = cpts[bn].NonCausalProbOfOccurence();
//        //    double nprob = 1 - prob;
//        //    double p = (prob * 100);
//        //    chances.Add(bn, new KeyValuePair<double, double>(currProb, currProb + p));
//        //    currProb += p;
//        //}
//        //double chosen = new Random().Next(1, (int)(currProb));
//        //bestNextChoice = chances.Where(x => x.Value.Key < chosen && x.Value.Value >= chosen).Select(x => x.Key).First();
//        //bestCurrVal = chosen / 100;


//        bool correctChoice = false;
//        if (bestNextChoice != null)
//        {
//            if (bestNextChoice.name == t.events[i + 1].fullName)
//            {
//                correctChoice = true;
//            }

//        }


//        bool partialCorrect = false;
//        if (!correctChoice && bestNextChoice != null)
//        {
//            if (t.events.Where(x => x.fullName == bestNextChoice.name).FirstOrDefault() != null)
//            {
//                partialCorrect = true;
//            }
//        }

//        res.nodes.Add(n);
//        Node nextNode = null;
//        if (traceTree.distinctNodes.TryGetValue(t.events[i + 1].fullName, out nextNode))
//        { }
//        res.results.Add(new KeyValuePair<Node, PredictionResult>(n,
//            new PredictionResult()
//            {
//                bestNextChoice = bestNextChoice,
//                current = n,
//                next = nextNode,
//                realNextProb = nextNode == null ? 0 : cpts[nextNode].ProbOfOccurence(),
//                currentProb = cpts[n].ProbOfOccurence(),
//                nextProb = bestNextChoice != null ? cpts[bestNextChoice].ProbOfOccurence() : double.NaN,
//                correct = correctChoice,
//                failedPredictionWithOccurence = partialCorrect
//            }));

//        if (i > 0 && i < t.events.Count - 1)
//        {
//            result.Add(
//                new KeyValuePair<string, int>(String.Format(eventTemplate, e.fullName, chance.ToString(), bestNextChoice == null ? "None" : bestNextChoice.name, bestCurrVal, correctChoice), correctChoice ? 1 : 0));
//        }
//    }
//    return res;// result;
//}


//public PredictionResults TracePredictionStepByStep(Trace t)
//{
//    string eventTemplate = "Event:{0} has prob to occur {1}, with best next event {2}={3} and selection is correct={4}";
//    foreach (string nd in traceTree.distinctNodes.Keys)
//    {
//        ClearObservation(nd);
//    }

//    List<KeyValuePair<string, int>> result = new List<KeyValuePair<string, int>>();
//    PredictionResults res = new PredictionResults();
//    result.Add(new KeyValuePair<string, int>("Checking trace:" + string.Join(";", t.events.Select(x => x.name)), 0));

//    List<Node> ActivatedNodes = new List<Node>();

//    HashSet<Node> badNodes = new HashSet<Node>();

//    ActivatedNodes.Add(traceTree.distinctNodes["start_event"]);
//    ClearObservation("start_event");
//    ObserveVariable("start_event", "occured", "1");

//    for (int i = 0; i < t.events.Count - 1; i++)
//    {
//        Event e = t.events[i];

//        Node n = traceTree.distinctNodes[e.fullName];
//        ActivatedNodes.Add(n);

//        traceTree.orMatrix.GetNonEmptyRowColumns(n.name, 0).ForEach(x =>
//        {
//            if (!badNodes.Contains(traceTree.distinctNodes[x]))
//            {
//                badNodes.Add(traceTree.distinctNodes[x]);
//            }
//        });


//        CPT cpt = cpts[n];

//        double chance = cpts[n].ProbOfOccurence();

//        foreach (var eVal in e.EventVals)
//        {
//            ObserveVariable(n.name, eVal.Key, eVal.Value.value);
//        }
//        ObserveVariable(n.name, "occured", "1");


//        double bestCurrVal = 0;

//        Node bestNextChoice = null;
//        var possibleNodes = n.childNodes.Union(from k in ActivatedNodes
//                                               from j in k.childNodes
//                                               where
//                                                   !ActivatedNodes.Contains(j)
//                                                   && !badNodes.Contains(j)
//                                                   && !n.childNodes.Contains(j)
//                                               select j);
//        List<KeyValuePair<Node, double>> choices = new List<KeyValuePair<Node, double>>();
//        foreach (Node childNode in possibleNodes.AsParallel())
//        {
//            var nodeCpt = cpts[childNode];

//            var ppp = new List<Tuple<string, string, string>>() { 
//                        new Tuple<string,string,string>(n.name,"occured","1"),                                
//                        new Tuple<string,string,string>(n.name,"previous","1") 
//                    };
//            var ppo = new List<Tuple<string, string, string>>() {
//                        new Tuple<string,string,string>(n.name,"occured","1"),                                
//                        new Tuple<string,string,string>(n.name,"previous","-2") 
//                    };

//            double prevp = 0;// ctivatedNodes.Where(x => childNode.parentNodes.Contains(x)).Max(x => { return traceTree.frequencyMatrix.GetValue(x.name, childNode.name) / x.NodeObjects.Count; });
//            double t1 = nodeCpt.WhatIf3(ppp, false, ppo, false);
//            double t2 = traceTree.frequencyMatrix.GetValue(n.name, childNode.name) / n.NodeObjects.Count;
//            if (t1 > 0.8)
//            {
//                prevp = t2;
//            }
//            else
//            {
//                if (double.IsInfinity(t1))
//                {
//                    prevp = t1;
//                }
//                else if (t1 >= 0 && t1 <= 1)
//                {
//                    prevp = t1;
//                }
//                else
//                {
//                    prevp = 0;
//                }
//            }

//            double poc = nodeCpt.ProbOfOccurence() * ActivatedNodes.Max(x => nodeCpt.WhatIf(x.name, "previous", "1")); ;// nodeCpt.WhatIf(n.name, "previous", "1");
//            double p = poc * prevp;// *tmp; //prevpfreq *


//            double c = p;// *pc;



//            #region random choice

//            //childNode.childNodes.ForEach(ccN =>
//            //    {
//            //        if (ActivatedNodes.Contains(ccN) || badNodes.Contains(ccN))
//            //        {
//            //            return;
//            //        }

//            //        //var t2 = new List<Tuple<string, string, string>>() { 
//            //        //    new Tuple<string,string,string>(childNode.name,"occured","1"),                                
//            //        //    new Tuple<string,string,string>(childNode.name,"previous","1") 
//            //        //};
//            //        //var t3 = new List<Tuple<string, string, string>>() {
//            //        //    new Tuple<string,string,string>(childNode.name,"occured","1")
//            //        //};

//            //        //double previousChance = nodeCpt.WhatIf3(t2, false, t3, false);


//            //        double c3 = cpts[ccN].WhatIf(childNode.name, "previous", "1");


//            //        if (c3 > c2 && !double.IsNaN(c3))
//            //        {
//            //            c2 = c3;
//            //        }
//            //    });

//            //c = childNode.childNodes.Count==0?c:c2;
//            #endregion

//            lock (choices)
//            {
//                choices.Add(new KeyValuePair<Node, double>(childNode, c));
//            }
//        }

//        for (int k = 0; k < choices.Count; k++)
//        {
//            double c = choices[k].Value;
//            var childNode = choices[k].Key;
//            if (c > bestCurrVal && !double.IsNaN(c))
//            {
//                bestNextChoice = childNode;
//                bestCurrVal = c;
//            }
//        }


//        //double sum = choices.Sum(x => x.Value);
//        //Random r = new Random();
//        //double rr = (double)(r.Next(0, (int)(sum * 100))) / 100;
//        //double partialSum = 0;
//        //Node chosen = null;
//        //for (int idx = 0; idx < choices.Count - 1; idx++)
//        //{
//        //    double tmp = partialSum + choices[idx].Value;
//        //    if (tmp >= rr && rr < tmp + choices[idx + 1].Value)
//        //    {
//        //        chosen = choices[idx].Key;
//        //    }
//        //}
//        //if (chosen != null)
//        //{
//        //    bestNextChoice = chosen;
//        //    bestCurrVal = cpts[chosen].ProbOfOccurence();
//        //}
//        //if (bestCurrVal == 1)
//        //{
//        //    bestCurrVal = 1;
//        //}`
//        bool correctChoice = false;
//        if (bestNextChoice != null)
//        {
//            if (i < t.events.Count - 1 && bestNextChoice.name == t.events[i + 1].fullName)
//            {
//                correctChoice = true;
//            }
//            //else
//            //{
//            //    guess = guess;
//            //    if(bestCurrVal > 0)
//            //    {
//            //        bestCurrVal = bestCurrVal;
//            //    }

//            //    if (choices.Count > 1)
//            //    {
//            //        var secondBest = choices.OrderByDescending(x => x.Value).Take(2).ElementAt(1);
//            //        Node sb = secondBest.Key;
//            //        if (i < t.events.Count - 1 && sb.name == t.events[i + 1].fullName)
//            //        {
//            //            bestNextChoice = sb;
//            //            bestCurrVal = secondBest.Value;
//            //            correctChoice = true;
//            //        }
//            //    }

//            //}
//        }


//        bool partialCorrect = false;
//        if (!correctChoice && bestNextChoice != null)
//        {
//            if (t.events.Where(x => x.fullName == bestNextChoice.name).FirstOrDefault() != null)
//            {
//                partialCorrect = true;
//            }
//        }

//        res.nodes.Add(n);
//        Node nextNode = null;
//        if (traceTree.distinctNodes.TryGetValue(t.events[i + 1].fullName, out nextNode))
//        { }
//        res.results.Add(new KeyValuePair<Node, PredictionResult>(n,
//            new PredictionResult()
//            {
//                bestNextChoice = bestNextChoice,
//                current = n,
//                next = nextNode,
//                realNextProb = nextNode == null ? 0 : cpts[nextNode].ProbOfOccurence(),
//                currentProb = cpts[n].ProbOfOccurence(),
//                nextProb = bestNextChoice != null ? cpts[bestNextChoice].ProbOfOccurence() : double.NaN,
//                correct = correctChoice,
//                failedPredictionWithOccurence = partialCorrect
//            }));

//        if (i > 0 && i < t.events.Count - 1)
//        {
//            result.Add(
//                new KeyValuePair<string, int>(String.Format(eventTemplate, e.fullName, chance.ToString(), bestNextChoice == null ? "None" : bestNextChoice.name, bestCurrVal, correctChoice), correctChoice ? 1 : 0));
//        }
//    }
//    return res;// result;
//}



#region CPT


//private long count = 0;


//public double WhatIf2(List<Tuple<string, string, string>> variables, bool allowNonExact = true)
//{
//    return Infer2(variables, allowNonExact, false);
//}

//bool cache = false;
//private double Infer2(List<Tuple<string, string, string>> variables, bool allowNonExact, bool returnNumber)
//{
//    Dictionary<int, int> _infVar = new Dictionary<int, int>();
//    if (aggregatedCPT.Length == 0)
//    {
//        return 1;
//    }
//    foreach (var iv in inferenceVariables)
//    {
//        _infVar.Add(iv.Key, iv.Value);
//    }

//    NPROB prob = (from i in columnIndexes where i.Key.Name == this.CPTMainNode.name select i.Key).FirstOrDefault();
//    int innerIdx = prob.columnsCount;



//    double r = double.NaN;

//    List<int> mustMatchIdx = new List<int>();
//    #region variable selection
//    foreach (var tuple in variables)
//    {
//        string node = tuple.Item1;
//        string variable = tuple.Item2;
//        string value = tuple.Item3;
//        if (!string.IsNullOrEmpty(node) && !string.IsNullOrEmpty(variable) && !string.IsNullOrEmpty(value))
//        {
//            prob = (from i in columnIndexes where i.Key.Name == node select i.Key).FirstOrDefault();
//            bool add = true;
//            if (prob != null)
//            {
//                innerIdx = default(int);
//                int valueIdx = default(int);
//                if (variable == "occured")
//                {
//                    innerIdx = prob.columnsCount + 1;
//                    valueIdx = Int32.Parse(value);
//                }
//                else if (variable == "No data")
//                {
//                    innerIdx = prob.columnsCount + 1;
//                    valueIdx = Int32.Parse(value);
//                }
//                else if (variable == "previous")
//                {

//                    innerIdx = prob.columnsCount;
//                    if (innerIdx == -1)
//                    {
//                        add = false;
//                    }
//                    valueIdx = Int32.Parse(value);
//                }
//                else
//                {
//                    innerIdx = prob.GetColumnIndex(variable);
//                    valueIdx = prob.values[innerIdx][value];
//                }

//                if (add)
//                {
//                    int currIdx = columnIndexes[prob];


//                    if (valueIdx == 0 && innerIdx == prob.columnsCount - 1)
//                    {
//                        throw new Exception("Why?");
//                    }

//                    if (_infVar.ContainsKey(currIdx + innerIdx))
//                    {

//                        if (valueIdx < -1)
//                        {
//                            _infVar.Remove(currIdx + innerIdx);
//                        }
//                        else
//                        {
//                            mustMatchIdx.Add(currIdx + innerIdx);
//                            _infVar[currIdx + innerIdx] = valueIdx;
//                        }
//                    }
//                    else
//                    {
//                        if (valueIdx >= -1)
//                        {
//                            mustMatchIdx.Add(currIdx + innerIdx);
//                            _infVar.Add(currIdx + innerIdx, valueIdx);
//                        }
//                    }
//                }
//            }
//        }
//    }

//    #endregion
//    string conc = "";
//    var ivs = (from i in _infVar orderby i.Key select i);
//    foreach (var iv in ivs)
//    {
//        conc += iv.Key + iv.Value;
//    }
//    if (cache && allowNonExact && cachedwhatifs.TryGetValue(conc, out r))
//    {
//        return r;
//    }
//    else if (cache && cachedwhatifstotals.TryGetValue(conc, out r))
//    {
//        return r;
//    }


//    var result = MakeInference2(_infVar, mustMatchIdx);
//    int colC = aggregatedCPT[0].Length;
//    int inferredTotal = 0;
//    int inferredObservedTotal = 0;

//    double refVal = 1;
//    if (_infVar.ContainsKey(colC - 2))
//    {
//        refVal = inferenceVariables[colC - 2];
//    }
//    foreach (var y in result.Key)
//    {
//        int val = y[colC - 1];
//        inferredTotal += val;
//        if (y[colC - 2] == refVal)
//        {
//            inferredObservedTotal += val;
//        }
//    }
//    r = (double)inferredObservedTotal / (double)inferredTotal;
//    if ((double.IsNaN(r) /*|| r == 0*/) && allowNonExact)
//    {
//        r = result.Value;
//    }
//    if (!cachedwhatifs.ContainsKey(conc) && cachedwhatifs.Count < 100000)
//    {
//        cachedwhatifs.Add(conc, r);
//    }


//    if (returnNumber)
//    {
//        return inferredObservedTotal;
//    }
//    return r;
//}
//private double GetConditionedProbability(bool conditionedByTotal, bool occuranceChance, Dictionary<int, int> _infVar)
//{
//    int colC = aggregatedCPT[0].Length;

//    int total = 0;
//    int totalObserved = 0;
//    int totalObservedConditionally = 0;
//    int totalConditionally = 0;


//    for (int i = 0; i < aggregatedCPT.Length; i++)
//    {
//        var y = aggregatedCPT[i];
//        bool suitable = true;
//        var ocVal = y[colC - 1];
//        var oc = y[colC - 2] > 0;
//        foreach (var val in _infVar)
//        {
//            if (y[val.Key] != val.Value)
//            {
//                suitable = false;
//            }
//        }
//        total += ocVal;
//        if (oc) { totalObserved += ocVal; }
//        if (suitable)
//        {
//            totalConditionally += ocVal;
//            if (oc)
//            {
//                totalObservedConditionally += ocVal;
//            }
//        }
//    }
//    double res = 1;


//    var pEH = (occuranceChance ? (double)totalConditionally : (double)totalObservedConditionally) / (conditionedByTotal ? total : totalConditionally);
//    var pH = 1;// (double)totalConditionally / total;
//    var pE = 1;// (double)totalObserved / total;
//    res = pEH * pH / pE;
//    return res;
//}

//internal double WhatIf3(List<Tuple<string, string, string>> variables1, bool allowNonExact, List<Tuple<string, string, string>> variables2, bool allowNonExact2)
//{
//    double v = Infer2(variables1, allowNonExact, true);
//    double x = Infer2(variables2, allowNonExact2, true);
//    //if(v>x && x != 0)
//    //{
//    //    throw new Exception("WTF");
//    //}
//    return v / x;
//}


//private KeyValuePair<List<int[]>, double> MakeInference2(Dictionary<int, int> _infVar, List<int> mustMatchIdx = null)
//{
//    mustMatchIdx = mustMatchIdx ?? new List<int>();
//    if (_infVar.Count == 0)
//    {
//        return new KeyValuePair<List<int[]>, double>(aggregatedCPT.AsParallel().ToList(), NonCausalProbOfOccurence()); //aggregatedCPT.AsParallel().ToList();
//    }
//    Dictionary<int, InferenceResult> foundVars = new Dictionary<int, InferenceResult>();

//    Dictionary<int, InferenceResult> totals = new Dictionary<int, InferenceResult>();
//    foreach (int x in _infVar.Keys)
//    {
//        foundVars.Add(x, new InferenceResult());
//        totals.Add(x, new InferenceResult());
//    }


//    List<int[]> result = aggregatedCPT.AsParallel().Where(y =>
//    {
//        bool found = true;
//        foreach (var val in _infVar)
//        {
//            if (y[val.Key] == val.Value)
//            {
//                if (y[y.Length - 1] > 0)
//                {
//                    System.Threading.Interlocked.Increment(ref foundVars[val.Key].res);
//                }
//                System.Threading.Interlocked.Increment(ref totals[val.Key].res);
//            }
//            else
//            {
//                found = false;
//            }
//        }
//        return found;

//    }).ToList();

//    double res = 1;
//    foreach (var r in foundVars)
//    {
//        if (totals[r.Key].res > 0 || mustMatchIdx.Contains(r.Key))
//        {
//            res *= ((double)r.Value.res / (double)totals[r.Key].res);
//            if (r.Value.res > totals[r.Key].res)
//            {
//                Console.WriteLine("something went wrong");
//            }
//        }
//    }
//    return new KeyValuePair<List<int[]>, double>(result, res);
//}

#endregion