using _3DViewer.Model;
using SharpGL.SceneGraph;
using System;
using System.IO;

namespace _3DViewer.File
{
    class STLFileWriter : STLFile
    {
        public bool binaryFormat = true;
        override public bool Write(string fileName)
        {
            bool success = false;

            if (null == this.stlData)
            {
                this.ProcessError = "No STLData to Export";
                return false;
            }

            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(fileName, FileMode.OpenOrCreate);
                Writer writer = null;
                if (this.binaryFormat)
                    writer = new _BinaryWriter(this.stlData);
                else
                    writer = new ASCIIWriter(this.stlData);

                success = writer.Write(fileStream);
                this.ProcessError = writer.error;
            }
            catch (Exception)
            {
                this.ProcessError = "XML Export write error";
                return false;
            }

            if(null != fileStream)
                fileStream.Close();

            return success;
        }

        // ///////////////////////////////////////////////
        class Writer
        {
            public string error = "";
            protected Model.STLData model;

            public Writer(Model.STLData model)
            {
                this.model = model;
            }

            virtual public bool Write(FileStream fileStream)
            {
                return false;
            }
        }

        //---------------------------------------------
        class ASCIIWriter : Writer
        {
            public ASCIIWriter(Model.STLData model) : base(model)
            { }

            override public bool Write(FileStream fileStream)
            {
                try
                {
                    StreamWriter fileWriter = new StreamWriter(fileStream);

                    string value = string.Format($"solid {this.model.name}");
                    fileWriter.WriteLine(value.Trim());
                    foreach(STLData.Facet facet in this.model.facetList)
                    {
                        Vertex n = facet.normals[0];
                        value = string.Format($"  facet normal {n.X} {n.Y} {n.Z}");
                        fileWriter.WriteLine(value);

                        fileWriter.WriteLine("    outer loop");
                        foreach (Vertex v in facet.vertexes)
                        {
                            value = string.Format($"      vertex {v.X} {v.Y} {v.Z}");
                            fileWriter.WriteLine(value);
                        }

                        fileWriter.WriteLine("    outerloop");
                        fileWriter.WriteLine("  endfacet");
                    }

                    fileWriter.WriteLine("endsolid");
                    fileWriter.Flush();
                    fileWriter.Close();
                }
                catch (Exception)
                {
                    this.error = "STL File write error";
                    return false;
                }

                return true;
            }
        }

        //------------------------------
        class _BinaryWriter : Writer
        {
            public _BinaryWriter(Model.STLData model) : base(model)
            { }
       
            override public bool Write(FileStream fileStream)
            {
                try
                { 
                    BinaryWriter fileWriter = new BinaryWriter(fileStream);
                    
                    var name = this.model.name.ToCharArray();
                    fileWriter.Write(name, 0, name.Length);
                    for(int n = name.Length; n < 80; n++)
                        fileWriter.Write('\0');                    

                    fileWriter.Write((UInt32)this.model.facetList.Count);

                    foreach (STLData.Facet facet in this.model.facetList)
                    {
                        this.writeVertex(fileWriter, facet.normals[0]);

                        foreach (Vertex v in facet.vertexes)
                            this.writeVertex(fileWriter, v);

                        fileWriter.Write((UInt16)0);
                    }

                    fileWriter.Flush();
                    fileWriter.Close();
                }
                catch (Exception)
                {
                    this.error = "Write STL File error";
                    return false;
                }

                return true;
            }

            #region "Helper methods"
            void writeVertex(BinaryWriter fileWriter, Vertex v)
            {
                fileWriter.Write((Single)v.X);
                fileWriter.Write((Single)v.Y);
                fileWriter.Write((Single)v.Z);
            }
            #endregion "Helper methods"
        }
    }
}
