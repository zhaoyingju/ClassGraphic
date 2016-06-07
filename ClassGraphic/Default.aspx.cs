using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ClassGraphic
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var re = new ClassTool.ClassRelation();
            //var dllPath = typeof(AssemblyDll.ClassC).Assembly.Location;
            var dllPath = Assembly.GetExecutingAssembly().Location;
            re.LoadFromAssemblyFile(dllPath);
            var image = re.Draw(re.FindClass("AssemblyDll.ClassAB"));
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            Response.ClearContent();
            Response.ContentType = "image/Jpeg";
            Response.BinaryWrite(ms.GetBuffer());
            ms.Close();
            ms = null;
            image.Dispose();
            image = null;
        }
    }
}