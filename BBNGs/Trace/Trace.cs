using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace BBNGs.TraceLog
{
    [Serializable]
    public class Trace
    {
        /// <summary>
        /// concept:name of the trace
        /// </summary>
        public string name;
        /// <summary>
        /// Start time of the first event in the trace
        /// </summary>
        public DateTime startTime;
        /// <summary>
        /// List of all event types in the trace
        /// </summary>
        public Dictionary<string, int> EventTypes = new Dictionary<string, int>();
        /// <summary>
        /// List of events in the trace
        /// </summary>
        public List<Event> events = new List<Event>();
        /// <summary>
        /// XML element of the trace
        /// </summary>
        [NonSerialized]
        internal XElement trace;

        /// <summary>
        /// Constructor of the trace 
        /// </summary>
        /// <param name="trace">XES format "LOG" node of the file.</param>
        /// <param name="xes">Namespace of the main namespace</param>
        internal Trace(XElement trace, XNamespace xes, List<string> attributesToIgnore = null, List<string>eventsToIgnore=null)
        {
            this.trace = trace;
            var nameNode = trace.Elements(xes + "string").First();
            this.name = nameNode.Attribute("value").Value;

            var eventai = trace.Elements(xes + "event");
            foreach (XElement e in eventai)
            {
                var evName = Event.EventName(e);
                if (eventsToIgnore.Contains(evName) || evName.StartsWith("w_"))
                    continue;
                
                events.Add(new Event(e, xes, this, attributesToIgnore));
            }

            events = events.OrderBy(x => x.timestamp).ToList();
            for(int i = 0; i < events.Count; i++)
            {
                events[i].AddEventIndex(i);   
            }
            startTime = events[0].timestamp;

            for (int i = 0; i < events.Count; i++)
            {
                Event ev = events[i];
                if (EventTypes.Keys.Contains(ev.name + ev.transition))
                {
                    //if (EventTypes[ev.name + ev.transition] > 1)
                    //{
                    //ev.e.Remove();
                    //events.Remove(ev);
                    //if (i > 0)
                    //    i--;
                    //}
                    //else if (ev.name != "Completed")
                    //{
                    EventTypes[ev.name + ev.transition] += 1;
                   // ev.name += EventTypes[ev.name + ev.transition];
                    // (from k in ev.e.Elements(xes + "string") where k.Attribute("key").Value == "concept:name" select k).First().Attribute("value").SetValue(ev.name);

                    //}
                }
                else
                    EventTypes.Add(ev.name + ev.transition, 0);
            }

            for (int i = 0; i < events.Count; i++)
            {
                var ev = events[i];
                for (int j = i+1; j < events.Count; j++)
                {
                    if (events[j].fullName == ev.fullName)
                    {
                        events.Remove(events[j]);
                    }
                }

            }


        }



        public bool TryToFind(string eventName, out Event val)
        {
            int result;
            if (!this.EventTypes.TryGetValue(eventName, out result))
            {
                val = null;
                return false;
            }

            foreach (Event e in this.events)
                if (e.fullName == eventName)
                {
                    val = e;
                    return true;
                }
            val = null;
            return false;
        }


        public bool IsSequential(string firstEvent, string secondEvent)
        {
            int firstIdx = -1;
            int secondIdx = -1;

            for (int i = 0; i < events.Count; i++)
            {
                if (events[i].fullName == firstEvent)
                    firstIdx = i;
                else if (events[i].fullName == secondEvent)
                    secondIdx = i;
            }

            if (firstIdx < 0 || secondIdx < 0 || firstIdx == secondIdx)
                return false;

            int idx = 0;
            int idx2 = 0;

            if (firstIdx < secondIdx)
            {
                idx = firstIdx;
                idx2 = secondIdx;
            }
            else
            {
                idx = secondIdx;
                idx2 = firstIdx;
            }

            if (idx2 - idx == 1)
                return true;

            bool suits = true;
            for (int i = idx + 1; i < idx2; i++)
            {
                if (events[i].fullName != firstEvent || events[i].fullName != secondEvent)
                {
                    suits = false;
                    break;
                }
            }
            return suits;
        }

    }

    /// <summary>
    /// Implements trace equality comparer.
    /// </summary>
    class TraceComparer : IEqualityComparer<Trace>
    {
        /// <summary>
        /// Checks if all event sequences are identical.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>True if equal event sequences, otherwise false.</returns>
        public bool Equals(Trace x, Trace y)
        {
            //unequal if not the same amount of events
            if (x.EventTypes.Count != y.EventTypes.Count)
                return false;

            //order evens for comparison
            List<Event> xEvents = x.events.OrderBy(e => e.timestamp).ToList();
            List<Event> yEvents = y.events.OrderBy(e => e.timestamp).ToList();

            //checks if corresponding events are equal
            for (int i = 0; i < xEvents.Count; i++)
            {
                if (xEvents[i].fullName != yEvents[i].fullName)
                    return false;
            }

            return true;
        }

        public int GetHashCode(Trace x)
        {
            return base.GetHashCode();
        }

    }
}
