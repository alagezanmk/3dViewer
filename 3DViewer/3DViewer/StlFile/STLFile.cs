using _3DViewer.Model;

namespace _3DViewer.File
{
    class STLFile
    {
        public STLData stlData = new STLData();

        public string ProcessError { get; set; }        

        virtual public bool Read(string fileName)
        {
            return false;
        }

        virtual public bool Write(string fileName)
        {
            return false;
        }
    }
}
