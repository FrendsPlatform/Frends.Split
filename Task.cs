using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Frends.Split
{
    public class Task
    {
        public class Input
        {
            [DisplayFormat(DataFormatString = "Text")]
            [DefaultValue(@"F:\myfile.xml")]
            public String InputFilePath { get; set; }

            [DisplayFormat(DataFormatString = "Text")]
            [DefaultValue(@"Product")]
            public String SplitAtElementName { get; set; }

            [DisplayFormat(DataFormatString = "Text")]
            [DefaultValue(@"F:\myfile.xml")]
            public String OutputFilesDirectory { get; set; }
        }

        public class Options
        {
            [DefaultValue(50)]
            public Int32 ElementCountInEachFile { get; set; }

            [DisplayFormat(DataFormatString = "Text")]
            [DefaultValue("RootElement")]
            public String OutputFileRootNodeName { get; set; }
        }

        public static dynamic SplitXMLFile([PropertyTab]Input Input, [PropertyTab]Options Options)
        {
            Int32 seqNr = 0;
            Int32 loopSeqNr = 0;
            JArray returnArray = new JArray();

            FileInfo fileInfo = new FileInfo(Input.InputFilePath);
            DirectoryInfo dirInfo = new DirectoryInfo(Input.OutputFilesDirectory);
            
            XmlReader processDoc = XmlReader.Create(Input.InputFilePath, new XmlReaderSettings() { DtdProcessing = DtdProcessing.Ignore });
            XmlDocument newDoc = InitiateNewDocument(Options.OutputFileRootNodeName);

            while (processDoc.Read())
            {
                if (processDoc.Name == Input.SplitAtElementName)
                {
                    XmlElement rootNode = newDoc.CreateElement(processDoc.Name);

                    rootNode.InnerXml = processDoc.ReadInnerXml();
                    newDoc.LastChild.AppendChild(rootNode);

                    if (loopSeqNr++ >= Options.ElementCountInEachFile)
                    {
                        string strFileName = fileInfo.Name + "." + seqNr++ + ".part";
                        newDoc.Save(dirInfo.FullName + @"\" + strFileName);
                        JObject filePart = new JObject();
                        filePart.Add("FilePart", dirInfo.FullName + @"\" + strFileName);
                        returnArray.Add(filePart);
                        loopSeqNr = 0;
                        newDoc = InitiateNewDocument(Options.OutputFileRootNodeName);
                    }
                }
            }

            return returnArray;
        }

        private static XmlDocument InitiateNewDocument(String Rootname)
        {
            XmlDocument newDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = newDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = newDoc.DocumentElement;
            newDoc.InsertBefore(xmlDeclaration, root);

            XmlElement rootElement = newDoc.CreateElement(string.Empty, Rootname, string.Empty);
            newDoc.AppendChild(rootElement);

            return newDoc;
        }
    }
}
