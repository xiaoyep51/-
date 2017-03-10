using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CommonHelper
{
    public class XMLHelper : XmlDocument
    {
        public string XmlFileName { set; get; }

        public XMLHelper(string xmlFile)
        {
            XmlFileName = xmlFile;

            this.Load(xmlFile);
        }

        public XmlNode GetNode(string xPath)
        {
            XmlNode xmlNode = this.SelectSingleNode(xPath);
            return xmlNode;
        }

        public string GetNodeAttributesValue(string xPath, string attributeName)
        {
            XmlNode xmlNode = this.GetNode(xPath);
            if (xmlNode != null)
            {
                XmlAttribute attribute = xmlNode.Attributes[attributeName];
                if (attribute != null)
                {
                    return attribute.Value;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        public XmlNodeList GetNodeList(string xPath)
        {
            XmlNodeList nodeList = this.SelectSingleNode(xPath).ChildNodes;
            return nodeList;

        }
    }
}
