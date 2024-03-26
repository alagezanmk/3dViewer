using _3DViewer.Model;
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
                this.model.name = "";
                this.model.facetList.Clear();

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
                    this.model.name = reader.Value;
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
            Facet facet = new Facet();
            int ivector = 0;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                case XmlNodeType.Element:
                    if ("Normal" == reader.Name)
                    {
                        this.readVector(facet.normals[0], reader);
                        facet.normals[1] = facet.normals[2] = facet.normals[0];
                    }
                    else if ("Vertex" == reader.Name && ivector < 3)
                        this.readVector(facet.vertexes[ivector++], reader);
                    break;

                case XmlNodeType.EndElement:
                    this.model.facetList.Add(facet);
                    return;
                }
            }            
        }
        private void readVector(Vector3 vector, XmlReader reader)
        {
            for (int a = 0; a < reader.AttributeCount; a++)
            {
                reader.MoveToAttribute(a);
                Single v = Single.Parse(reader.Value); 
                switch (reader.Name)
                {
                case "x":
                    vector.x = v;
                    break;

                case "y":
                    vector.y = v;
                    break;

                case "z":
                    vector.z = v;
                    break;
                }
            }
        }
    }
}
