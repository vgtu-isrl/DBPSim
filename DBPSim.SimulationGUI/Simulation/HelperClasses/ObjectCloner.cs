using DBPSim.RuleEngine.Memory;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace DBPSim.SimulationGUI.HelperClasses
{
    class ObjectCloner
    {

        public static WorkingMemory CloneWorkingMemory(WorkingMemory workingMemoryInstance)
        {
            WorkingMemory workingMemoryCopy = new WorkingMemory();

            foreach (KeyValuePair<string, object> workingMemoryObject in workingMemoryInstance)
            {
                ExpandoObject expandoObject = workingMemoryObject.Value as ExpandoObject;
                if (expandoObject != null)
                {
                    workingMemoryCopy.Add(workingMemoryObject.Key, CloneExpandoObject(expandoObject));
                }
            }

            return workingMemoryCopy;
        }


        public static ExpandoObject CloneExpandoObject(ExpandoObject expandoObject)
        {
            ExpandoObject expandoObjectCopy = new ExpandoObject();

            IDictionary<string, object> expandoObjectProperties = (IDictionary<string, object>)expandoObject;

            foreach (KeyValuePair<string, object> expandoObjectPropertyValue in expandoObjectProperties)
            {
                ((IDictionary<string, object>)expandoObjectCopy).Add(expandoObjectPropertyValue.Key, expandoObjectPropertyValue.Value);
            }

            return expandoObjectCopy;
        }

    }
}
