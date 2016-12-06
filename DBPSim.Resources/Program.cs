using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBPSim.Resources
{
    public class ManageResource
    {
        static void Main(string[] args)
        {
        }
        public static IEnumerable<Resource> FindFreeResource()
        {
            using (var db = new ResDBDataContext())
            {
                var x = from r in db.Resources where r.ResourceAllocations.First() == null select r;
                return x;
            }
        }
        public int Amount(string title, string propertie)
        {
            using (var db = new ResDBDataContext())
            {
                var res = db.Resources.Where(r => r.Title.ToString() == title).Select(r => r).FirstOrDefault();
                if (res != null)
                {
                    var atributeID = db.Attributes.Where(a => a.Title.ToString() == propertie).Select(a => a).FirstOrDefault();
                    var resat = db.ResourceAttributes.Where(ra => ra.Resource_ID == res.ID && ra.Attribute_ID == atributeID.ID).Select(ra => ra).FirstOrDefault();
                    if (resat.Value > 0)
                    {
                        return resat.Value;
                    }
                    else
                        return 0;

                }
                else
                    return 0;
            }
        }
        public void ResourceRelease(string title, int duration)
        {
            using (var db = new ResDBDataContext())
            {
                var findResource= db.Resources.Where(r=>r.Title.ToString()==title).Select(r=>r).FirstOrDefault();
                var allocatedResource = db.ResourceAllocations.Where(ra => ra.Resource_ID == findResource.ID).Select(ra => ra);
                foreach (ResourceAllocation res in allocatedResource)
                {
                    res.Occupied_Untill = DateTime.Now.AddHours(duration);
                }
                try
                {
                    db.SubmitChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    // Provide for exceptions.
                }
            }
        }


        public int ResourceAllocation(string title, int amount, int duration, string ActivityName)
        //int resourceID, int AllocatedResourceID, int ActivityID, DateTime From, DateTime Untill)
        {
            using (var db = new ResDBDataContext())
            {
                if (amount > 0)
                {
                    var activityID = ActivityID(ActivityName);
                    var a = Amount(title, "Amount");
                    if (a > 0)
                    {
                        var ID = Allocate(title, duration, activityID, amount);
                        UpdateCount(title, "Amount", amount);
                        return ID;
                    }
                    else
                    {
                        return -1;
                    }
                }
                else
                {
                    var activityID = ActivityID(ActivityName);
                    var ID = Allocate(title, duration, activityID);
                    return ID;
                }
            }
        }
        static int ActivityID(string ActivityName)
        {
            using (var db = new ResDBDataContext())
            {
                var FindActivity = db.Activities.Where(a => a.Title.ToString() == ActivityName).Select(a => a).FirstOrDefault();
                int activityID;
                if (FindActivity == null)
                {
                    var activity = new Activity()
                    {
                        Title = ActivityName,
                        Process_ID = 1
                    };
                    db.Activities.InsertOnSubmit(activity);
                    db.SubmitChanges();
                    activityID = activity.ID;
                    return activityID;
                }
                else
                {
                    activityID = FindActivity.ID;
                    return activityID;
                }
            }
        }
        static int Allocate(string title, int duration, int activityID, int amount)
        {
            using (var db = new ResDBDataContext())
            {
                var res = db.Resources.Where(r => r.Title.ToString() == title).Select(r => r).FirstOrDefault();
                var resAllocation = new ResourceAllocation()
                {
                    Resource_ID = res.ID,
                    Res_Resource_ID = null,
                    Activity_ID = activityID,
                    Occupied_From = DateTime.Now.AddHours(duration),
                    Occupied_Untill = null
                };
                db.ResourceAllocations.InsertOnSubmit(resAllocation);
                db.SubmitChanges();

                var consumeres = new ConsumedResourceAttribute()
                {
                    ResourceAllocation_ID = resAllocation.ID,
                    Amount = amount,
                    ConsumeType_ID = 1
                };
                db.ConsumedResourceAttributes.InsertOnSubmit(consumeres);
                db.SubmitChanges();
                return resAllocation.ID;
            }
        }
        static bool CheckIfAllocated(int resID)
        {
            using (var db = new ResDBDataContext())
            {

                var allocation = db.ResourceAllocations.Where(r => r.Resource_ID == resID).OrderByDescending(r => r.ID).Select(r => r).First();
                if (allocation.Occupied_Untill == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool CheckAlocation(string title)
        {
            using (var db = new ResDBDataContext())
            {
                var res = db.Resources.Where(r => r.Title.ToString() == title).Select(r => r).FirstOrDefault();
                var allocate = CheckIfAllocated(res.ID);
                if(allocate == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        static int Allocate(string title, int duration, int activityID)
        {
            using (var db = new ResDBDataContext())
            {
                var res = db.Resources.Where(r => r.Title.ToString() == title).Select(r => r).FirstOrDefault();
                var alocated = CheckIfAllocated(res.ID);
                if(alocated == true)
                {
                    return -1;
                }
                else
                {
                    var resAllocation = new ResourceAllocation()
                    {
                        Resource_ID = res.ID,
                        Res_Resource_ID = null,
                        Activity_ID = activityID,
                        Occupied_From = DateTime.Now.AddMinutes(duration),
                        Occupied_Untill = null
                    };
                    db.ResourceAllocations.InsertOnSubmit(resAllocation);
                    db.SubmitChanges();
                    return resAllocation.ID;
                }
               

                /*var consumeres = new ConsumedResourceAttribute()
                {
                    ResourceAllocation_ID = resAllocation.ID,
                    Amount = 1,
                    ConsumeType_ID = 1
                };
                db.ConsumedResourceAttributes.InsertOnSubmit(consumeres);
                db.SubmitChanges();*/
                
            }
        }
        static void UpdateCount(string title, string propertie, int amount)
        {
            using (var db = new ResDBDataContext())
            {
                var res = db.Resources.Where(r => r.Title.ToString() == title).Select(r => r).FirstOrDefault();
                if (res != null)
                {
                    var atributeID = db.Attributes.Where(a => a.Title.ToString() == propertie).Select(a => a).FirstOrDefault();
                    var resat = db.ResourceAttributes.Where(ra => ra.Resource_ID == res.ID && ra.Attribute_ID == atributeID.ID).Select(ra => ra).FirstOrDefault();
                    resat.Value = resat.Value - amount;
                }
                db.SubmitChanges();
            }
        }
        public bool AddResources(string title, string properties, int amount)
        {
            using (var db = new ResDBDataContext())
            {
                var res = db.Resources.Where(r => r.Title.ToString() == title).Select(r => r).FirstOrDefault();
                if (res != null)
                {
                    var consumeres = new ConsumedResourceAttribute()
                    {
                        ResourceAllocation_ID = null,
                        Amount = amount,
                        ConsumeType_ID = 2
                    };
                    db.ConsumedResourceAttributes.InsertOnSubmit(consumeres);
                    db.SubmitChanges();
                    UpdateCount(title, properties, amount);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public void Cancel(string title, int ID, int amount, int duration)
        {
            using (var db = new ResDBDataContext())
            {
                var resal = db.ResourceAllocations.Where(ra => ra.ID == ID).Select(ra => ra).FirstOrDefault();
                resal.Occupied_Untill = DateTime.Now.AddHours(duration);
                db.SubmitChanges();
                var consumeRA = db.ConsumedResourceAttributes.Where(cra => cra.ResourceAllocation_ID == resal.ID).Select(cra => cra).FirstOrDefault();
                var conType = db.ConsumeTypes.Where(ct=>ct.Title.ToString()=="Cancel").Select(ct=>ct).FirstOrDefault();
                consumeRA.ConsumeType_ID = conType.ID;
                db.SubmitChanges();
                UpdateCount(title, "Amount", -amount);
            }
        }
    }
}
