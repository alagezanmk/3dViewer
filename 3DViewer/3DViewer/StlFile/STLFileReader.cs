using System;
using System.Collections.Generic;
using System.IO;

using SharpGL.SceneGraph;

using _3DViewer.Model;

namespace _3DViewer.File
{
    class STLFileReader : STLFile
    {
        override public bool Read(string fileName)
        {
            bool success = false;

            do
            {
                FileStream fileStream = null;
                try
                {
                    fileStream = new FileStream(fileName, FileMode.Open);

                    Parser parser = null;
                    if (this.IsAscii(fileStream))
                        parser = new ASCIIParser(this.stlData);
                    else
                        parser = new BinaryParser(this.stlData);

                    fileStream.Seek(0, 0);
                    success = parser.Parse(fileStream);
                    this.ProcessError = parser.error;
                }
                catch (Exception)
                { }

                if (null != fileStream)
                {
                    fileStream.Close();
                    fileStream = null;
                }

            } while (false);

            return success;
        }

        private bool IsAscii(FileStream fileStream)
        {
            StreamReader fileReader = new StreamReader(fileStream);
            string line = fileReader.ReadLine();
            string[] tokens = line.Split(' ');
            if (tokens.Length > 0 && "solid" == tokens[0].Trim())
                return true;

            return false;
        }

        ///////////////////////////////////////// 
        class Parser
        {
            public string error = "";
            protected Model.STLData stlData;

            public Parser(Model.STLData stlData)
            {
                this.stlData = stlData;
            }

            virtual public bool Parse(FileStream fileStream)
            {
                return false;
            }

        }

        //------------------------------
        class ASCIIParser : Parser
        {
            public ASCIIParser(Model.STLData model) : base(model)
            { }

            override public bool Parse(FileStream fileStream)
            {
                bool success = true;
                bool completed = false;

                try
                {
                    StreamReader fileReader = new StreamReader(fileStream);
                    
                    this.error = "File-Start 'solid' is not found";

                    string line;
                    while (success && false == completed && false == fileReader.EndOfStream)
                    {
                        line = fileReader.ReadLine().Trim();
                        string[] tokens = line.Split(' ');

                        switch (tokens[0].Trim())
                        {
                        case "solid":
                            this.stlData.name = tokens[1].Trim();
                            this.error = "File-End 'endsolid' is not found";
                            continue;

                        case "facet":
                            success = this.ParseFacet(fileReader, tokens);
                            break;

                        case "endsolid":
                            completed = true;
                            this.error = "";
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    this.error = "STL Read file error";
                    return false;
                }

                return true;
            }

            private bool checkTokens(ref string[] tokens, int minCount, string[] tokenValues, string errorToken = null)
            {
                if (null == errorToken)
                    errorToken = tokenValues[0];

                List<string> _tokens = new List<string>();
                foreach(var token in tokens)
                {
                    var _token = token.Trim();
                    if (!string.IsNullOrEmpty(_token))
                        _tokens.Add(_token);
                }

                int t = 0;
                tokens = new string[_tokens.Count];
                foreach (var token in _tokens)
                    tokens[t++] = token;

                if (tokenValues.Length > tokens.Length)
                {
                    this.error = $"'{errorToken}' is not found";
                    return false;
                }

                for (t = 0; t < tokenValues.Length; t++)
                {
                    if (tokenValues[t].Trim() != tokens[t].Trim())
                    {
                        this.error = $"'{errorToken.Trim()}' is not found";
                        return false;
                    }
                }

                return true;
            }

            private void paseVertex(ref Vertex vertex, string[] tokens, int index)
            {
                vertex.X = float.Parse(tokens[index++]);
                vertex.Y = float.Parse(tokens[index++]);
                vertex.Z = float.Parse(tokens[index]);
            }

            private bool ParseFacet(StreamReader fileReader, string[] tokens)
            {
                STLData.Facet facet = new STLData.Facet();

                bool success = false;
                do
                {
                    if (!this.checkTokens(ref tokens, 5, new string[] { "facet", "normal" }))
                        break;

                    this.paseVertex(ref facet.normals[0], tokens, 2);
                    facet.normals[1] = facet.normals[2] = facet.normals[0];

                    string line = fileReader.ReadLine();
                    tokens = line.Split(' ');

                    if (!this.checkTokens(ref tokens, 2, new string[] { "outer", "loop" }))
                        break;

                    for (int v = 0; v < 3; v++)
                    {
                        line = fileReader.ReadLine();
                        tokens = line.Split(' ');
                        if (!this.checkTokens(ref tokens, 4, new string[] { "vertex" }, $"vertext[{v}]"))
                            break;

                        this.paseVertex(ref facet.vertexes[v], tokens, 1);
                    }

                    line = fileReader.ReadLine();
                    tokens = line.Split(' ');
                    if (!this.checkTokens(ref tokens, 4, new string[] { "endloop" }))
                        break;

                    line = fileReader.ReadLine();
                    tokens = line.Split(' ');
                    if (!this.checkTokens(ref tokens, 4, new string[] { "endfacet" }))
                        break;

                    success = true;
                } while (false);

                this.stlData.facetList.Add(facet);

                return success;
            }
        }

        //------------------------------
        class BinaryParser : Parser
        {
            public BinaryParser(Model.STLData model) : base(model)
            { }

            private void paseVertex(ref Vertex vertex, BinaryReader binaryReader)
            {
                vertex.X = binaryReader.ReadSingle();
                vertex.Y = binaryReader.ReadSingle();
                vertex.Z = binaryReader.ReadSingle();
            }

            override public bool Parse(FileStream fileStream)
            {
                const int minLength = 80 + 4 + 12 * 4 + 2;
                if (fileStream.Length < minLength)
                {
                    this.error = "Too short length of binary data";
                    return false;
                }

                bool success = true;
                try
                {
                    BinaryReader binaryReader = new BinaryReader(fileStream);

                    byte[] attributes;
                    byte[] header = binaryReader.ReadBytes(80);
                    this.stlData.name = System.Text.Encoding.UTF8.GetString(header, 0, header.Length);

                    // transcate at \0
                    for(int i = 0; i < this.stlData.name.Length; i++)
                    {
                        if ('\0' == this.stlData.name[i])
                        {
                            this.stlData.name = this.stlData.name.Substring(0, i);
                            break;
                        }
                    }

                    uint facetCount = binaryReader.ReadUInt32();

                    for (int f = 0; f < facetCount; f++)
                    {
                        STLData.Facet facet = new STLData.Facet();

                        this.paseVertex(ref facet.normals[0], binaryReader);
                        facet.normals[1] = facet.normals[2] = facet.normals[0];

                        for (int v = 0; v < 3; v++)
                            this.paseVertex(ref facet.vertexes[v], binaryReader);

                        UInt16 attrCount = binaryReader.ReadUInt16(); // attribute size count
                        if (attrCount > 0) // skip attribute bytes
                        {
                            attributes = binaryReader.ReadBytes(attrCount);
                            string name = System.Text.Encoding.UTF8.GetString(attributes, 0, attributes.Length);
                        }

                        stlData.facetList.Add(facet);
                    }

                    return true;
                }
                catch (Exception)
                {
                    success = false;
                }

                return success;
            }
        }
    }
}
