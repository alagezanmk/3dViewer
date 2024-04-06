using _3DViewer.Model;
using SharpGL.SceneGraph;
using System;
using System.Xml;

namespace _3DViewer.File
{
    class STLXmlImport : STLFile
    {
        override public bool Read(string fileName)
        {
            try
            {
                this.stlData.name = "";
                this.stlData.facetList.Clear();

                using (XmlReader reader = XmlReader.Create(fileName))
                {
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                        case XmlNodeType.Element:
                            if ("Solid" == reader.Name)
                                this.readSolid(reader);
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                this.ProcessError = "XML Import read error";
                return false;
            }

            return true;
        }

        private void readSolid(XmlReader reader)
        {
            for (int a = 0; a < reader.AttributeCount; a++)
            {
                reader.MoveToAttribute(a);
                if ("name" == reader.Name)
                    this.stlData.name = reader.Value;
            }

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                case XmlNodeType.Element:
                    if ("Facet" == reader.Name)
                        this.readFacet(reader);
                    break;

                case XmlNodeType.EndElement:
                    return;
                }
            }
        }

        private void readFacet(XmlReader reader)
        {
            STLData.Facet facet = new STLData.Facet();
            int ivector = 0;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                case XmlNodeType.Element:
                    if ("Normal" == reader.Name)
                    {
                        this.readVertex(facet.normals[0], reader);
                        facet.normals[1] = facet.normals[2] = facet.normals[0];
                    }
                    else if ("Vertex" == reader.Name && ivector < 3)
                        this.readVertex(facet.vertexes[ivector++], reader);
                    break;

                case XmlNodeType.EndElement:
                    this.stlData.facetList.Add(facet);
                    return;
                }
            }            
        }

        #region "Helper methods"
        private void readVertex(Vertex vertex, XmlReader reader)
        {
            for (int a = 0; a < reader.AttributeCount; a++)
            {
                reader.MoveToAttribute(a);
                Single v = Single.Parse(reader.Value); 
                switch (reader.Name)
                {
                case "x":
                    vertex.X = v;
                    break;

                case "y":
                    vertex.Y = v;
                    break;

                case "z":
                    vertex.Z = v;
                    break;
                }
            }
        }
        #endregion "Helper methods"
    }
}
