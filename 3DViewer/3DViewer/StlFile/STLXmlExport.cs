using _3DViewer.Model;
using System;
using System.Xml;

namespace _3DViewer.File
{
    class STLXmlExport : STLFile
    {
        override public bool Write(string fileName)
        {
            if(null == this.model)
            {
                this.ProcessError = "No STLData to Export";
                return false;
            }

            try
            {
                using (XmlWriter writer = XmlWriter.Create(fileName))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Solid");
                    writer.WriteAttributeString("name", this.model.name);

                    foreach (Facet facet in this.model.facetList)
                    {
                        writer.WriteStartElement("Facet");

                        this.writeVector(writer, "Normal", facet.normals[0]);

                        foreach (Vector3 v in facet.vertexes)
                            this.writeVector(writer, "Vertex", v);

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
            catch (Exception)
            {
                this.ProcessError = "XML Export write error";
                return false;
            }

            return true;
        }

        void writeVector(XmlWriter writer, string nodeName, Vector3 vector)
        {
            writer.WriteStartElement(nodeName);
            writer.WriteAttributeString("x", vector.x.ToString());
            writer.WriteAttributeString("y", vector.y.ToString());
            writer.WriteAttributeString("z", vector.z.ToString());
            writer.WriteEndElement();
        }
    }
}
