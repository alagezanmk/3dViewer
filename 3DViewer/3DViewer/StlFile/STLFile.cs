using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using _3DViewer;
namespace _3DViewer.File
{
    class STLFile
    {
        public Model.GLModel model = new Model.GLModel();

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
