using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace DBPSim.RuleEngine.DataSource
{
    public class XmlDataSourceProvider : DataSourceProvider
    {

        private string _xml;


        public XmlDataSourceProvider()
        {
        }


        public XmlDataSourceProvider(string xml)
        {
            this._xml = xml;
        }


        public string ExportedXml
        {
            get
            {
                return this._xml;
            }
        }


        public override RuleCollection Load()
        {
            RuleCollection ruleCollection = null;
            bool xmlLoaded = false;
            XmlDocument document = new XmlDocument();
            try
            {
                document.LoadXml(this._xml);
                xmlLoaded = true;
            }
            catch (XmlException)
            {
            }

            if (xmlLoaded)
            {
                XmlElement documentElement = document.DocumentElement;
                List<XmlElement> ruleElements = XmlHelper.FindElements(documentElement, "Rule");
                if (ruleElements.Count > 0)
                {
                    ruleCollection = new RuleCollection();
                }
                foreach (XmlElement ruleElement in ruleElements)
                {
                    string id = XmlHelper.GetElementString(ruleElement, "Id");
                    bool enabled = XmlHelper.GetElementAsBoolean(ruleElement, "Enabled") ?? true;
                    string title = XmlHelper.GetElementString(ruleElement, "Title");
                    int? priority = XmlHelper.GetElementAsInteger(ruleElement, "Priority");
                    string condition = XmlHelper.GetElementString(ruleElement, "Condition");
                    string body = XmlHelper.GetElementString(ruleElement, "Body");
                    
                    RuleConditional rule = new RuleConditional();                   
                    rule.Id = id;
                    rule.Enabled = enabled;
                    rule.Title = title;
                    rule.Priority = priority;
                    rule.Condition = condition;
                    rule.Body = body;

                    ruleCollection.Add(rule);
                }
            }
            return ruleCollection;
        }


        public override void Save(RuleCollection ruleCollection)
        {
            XmlDocument document = new XmlDocument();
            XmlElement documentElement = document.CreateElement("RuleCollection");
            document.AppendChild(documentElement);
            foreach (RuleConditional rule in ruleCollection)
            {
                XmlElement ruleElement = XmlHelper.CreateXmlElement(documentElement, "Rule");
                XmlHelper.SaveValue(ruleElement, "Id", rule.Id);
                XmlHelper.SaveElement(ruleElement, "Enabled", rule.Enabled);
                XmlHelper.SaveValue(ruleElement, "Title", rule.Title);
                XmlHelper.SaveElement(ruleElement, "Priority", rule.Priority);
                XmlHelper.SaveValue(ruleElement, "Condition", rule.Condition);
                XmlHelper.SaveValue(ruleElement, "Body", rule.Body);
            }
            this._xml = document.OuterXml;
        }

    }
}
