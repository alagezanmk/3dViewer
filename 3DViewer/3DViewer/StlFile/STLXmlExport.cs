using _3DViewer.Model;
using SharpGL.SceneGraph;
using System;
using System.Xml;

namespace _3DViewer.File
{
    class STLXmlExport : STLFile
    {
        override public bool Write(string fileName)
        {
            if(null == this.stlData)
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
                    writer.WriteAttributeString("name", this.stlData.name);

                    foreach (STLData.Facet facet in this.stlData.facetList)
                    {
                        writer.WriteStartElement("Facet");

                        this.writeVertex(writer, "Normal", facet.normals[0]);

                        foreach (Vertex v in facet.vertexes)
                            this.writeVertex(writer, "Vertex", v);

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

        #region "Helper methods"
        void writeVertex(XmlWriter writer, string nodeName, Vertex vertex)
        {
            writer.WriteStartElement(nodeName);
            writer.WriteAttributeString("x", vertex.X.ToString());
            writer.WriteAttributeString("y", vertex.Y.ToString());
            writer.WriteAttributeString("z", vertex.Z.ToString());
            writer.WriteEndElement();
        }
        #endregion "Helper methods"
    }
}