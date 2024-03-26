using _3DViewer.Model;
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

            if (null == this.model)
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
                    writer = new _BinaryWriter(this.model);
                else
                    writer = new ASCIIWriter(this.model);

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
            protected Model.GLModel model;

            public Writer(Model.GLModel model)
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
            public ASCIIWriter(Model.GLModel model) : base(model)
            { }

            override public bool Write(FileStream fileStream)
            {
                try
                {
                    StreamWriter fileWriter = new StreamWriter(fileStream);

                    string value = string.Format($"solid {this.model.name}");
                    fileWriter.WriteLine(value.Trim());
                    foreach(Facet facet in this.model.facetList)
                    {
                        Vector3 n = facet.normals[0];
                        value = string.Format($"  facet normal {n.x} {n.y} {n.z}");
                        fileWriter.WriteLine(value);

                        fileWriter.WriteLine("    outer loop");
                        foreach (Vector3 v in facet.vertexes)
                        {
                            value = string.Format($"      vertex {v.x} {v.y} {v.z}");
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
            public _BinaryWriter(Model.GLModel model) : base(model)
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

                    foreach (Facet facet in this.model.facetList)
                    {
                        this.writeVector(fileWriter, facet.normals[0]);

                        foreach (Vector3 v in facet.vertexes)
                            this.writeVector(fileWriter, v);

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

            void writeVector(BinaryWriter fileWriter, Vector3 v)
            {
                fileWriter.Write((Single)v.x);
                fileWriter.Write((Single)v.y);
                fileWriter.Write((Single)v.z);
            }
        }
    }
}
