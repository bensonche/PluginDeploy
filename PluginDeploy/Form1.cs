using System;
using System.Windows.Forms;
using System.IO;

namespace PluginDeploy
{
    public partial class Form1 : Form
    {
        private const string outputDir = @"c:\temp\plugins";
        private const string prefix = @"RDI.Service.Plugins";

        public Form1()
        {
            InitializeComponent();
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            btnGo.Enabled = false;

            DirectoryInfo output = new DirectoryInfo(outputDir);
            if(output.Exists)
            {
                foreach (var f in output.GetFiles())
                    f.Delete();
                output.Delete();
            }
            output.Create();

            DirectoryInfo dir = new DirectoryInfo(txtInput.Text);

            foreach(var d in dir.GetDirectories())
            {
                if(d.Name.StartsWith(prefix))
                {
                    processDirectory(d);
                }
            }

            btnGo.Enabled = true;
        }

        private void processDirectory(DirectoryInfo dir)
        {
            DirectoryInfo bin = new DirectoryInfo(Path.Combine(dir.FullName, @"bin\Release"));
            if (bin.Exists)
            {
                foreach (var f in bin.GetFiles())
                {
                    File.Copy(f.FullName, Path.Combine(outputDir, f.Name), true);
                }
            }
        }
    }
}
