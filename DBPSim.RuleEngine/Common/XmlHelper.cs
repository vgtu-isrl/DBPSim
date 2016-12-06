using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Xml;

namespace DBPSim.RuleEngine
{
    public class XmlHelper
    {

        public static XmlElement CreateXmlElement(XmlNode document, string elementName)
        {
            XmlDocument xmlDocument = null;
            if (document is XmlDocument)
            {
                xmlDocument = (XmlDocument)document;
            }
            else
            {
                xmlDocument = document.OwnerDocument;
            }

            XmlElement createdElement = xmlDocument.CreateElement(elementName);

            document.AppendChild(createdElement);

            return createdElement;
        }


        public static XmlElement SaveValue(XmlNode element, string elementName, string value)
        {
            if (value == null)
            {
                return null;
            }

            XmlElement createdElement = XmlHelper.CreateXmlElement(element, elementName);

            XmlNode elementValue = null;

            if (value.Contains("<") || value.Contains('\r') || value.Contains('\n') || value.Contains('\t'))
            {
                elementValue = element.OwnerDocument.CreateCDataSection(value);
            }
            else
            {
                elementValue = element.OwnerDocument.CreateTextNode(value);
            }

            createdElement.AppendChild(elementValue);

            return createdElement;
        }


        public static List<XmlElement> FindElements(XmlNode element, string elementName)
        {
            List<XmlElement> elementsToReturn = new List<XmlElement>();

            foreach (XmlNode node in element.ChildNodes)
            {
                if (node is XmlElement)
                {
                    if (node.Name.Equals(elementName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        elementsToReturn.Add((XmlElement)node);
                    }
                }
            }

            return elementsToReturn;
        }


        public static XmlElement FindElement(XmlNode element, string elementName)
        {
            XmlElement elementToReturn = null;

            foreach (XmlNode childNode in element.ChildNodes)
            {
                if (childNode is XmlElement)
                {
                    if (childNode.Name.Equals(elementName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        elementToReturn = (XmlElement)childNode;
                        break;
                    }
                }
            }

            return elementToReturn;
        }


        public static string GetElementString(XmlNode element, string elementName)
        {
            XmlElement strElement = XmlHelper.FindElement(element, elementName);

            return XmlHelper.ParseStringFromElement(strElement);

        }


        public static XmlElement SaveElement(XmlNode element, string name, object value)
        {
            if (value != null)
            {
                if (value is DateTime)
                {
                    return SaveValue(element, name, ((DateTime)value).ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    return SaveValue(element, name, value.ToString());
                }
            }
            else
            {
                return SaveValue(element, name, (string)null);
            }
        }


        public static bool? GetElementAsBoolean(XmlNode element, string elementName)
        {
            string stringValue = XmlHelper.GetElementString(element, elementName);

            bool value;
            if (bool.TryParse(stringValue, out value))
            {
                return value;
            }

            return null;
        }


        public static int? GetElementAsInteger(XmlNode element, string elementName)
        {
            string stringValue = XmlHelper.GetElementString(element, elementName);

            int value;
            if (int.TryParse(stringValue, out value))
            {
                return value;
            }

            return null;
        }


        public static DateTime? GetElementAsDate(XmlNode element, string elementName)
        {
            string stringValue = XmlHelper.GetElementString(element, elementName);

            if (!string.IsNullOrEmpty(stringValue))
            {
                return Convert.ToDateTime(stringValue, CultureInfo.InvariantCulture);
            }

            return null;
        }


        public static string ParseStringFromElement(XmlNode element)
        {
            if (element == null)
            {
                return null;
            }

            if (element.ChildNodes.Count > 0)
            {
                XmlNode elementNode = element.ChildNodes.Item(0);

                if (elementNode is XmlText)
                {
                    XmlText textNode = (XmlText)elementNode;
                    return textNode.Value;
                }
                else if (elementNode is XmlCDataSection)
                {
                    XmlCDataSection cDataNode = (XmlCDataSection)elementNode;
                    return cDataNode.Value;
                }
            }

            return string.Empty;
        }

    }
}
