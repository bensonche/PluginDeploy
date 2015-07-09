using System;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;

namespace PluginDeploy
{
    public partial class Form1 : Form
    {
        private string outputDir
        {
            get { return txtOutput.Text; }
        }

        private const string prefix = @"rdi.service.plugins";

        private string configuration
        {
            get { return rdbRelease.Checked ? @"bin\Release" : @"bin\Debug"; }
        }

        public Form1()
        {
            InitializeComponent();
            LoadSettings();
        }

        #region Save/Load Settings

        private const string settingsFileName = "settings.xml";

        private void LoadSettings()
        {
            try
            {
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

                if (!isoStore.FileExists(settingsFileName))
                    return;

                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(settingsFileName, FileMode.Open, FileAccess.Read, isoStore))
                using (StreamReader sr = new StreamReader(isoStream))
                {
                    string settings = sr.ReadToEnd();

                    XElement element = XElement.Parse(settings);
                    txtInput.Text = (from field in element.Elements("appSettings").Elements("directory")
                                     select field.Value).FirstOrDefault() ?? "";
                    txtOutput.Text = (from field in element.Elements("appSettings").Elements("output")
                                     select field.Value).FirstOrDefault() ?? "";
                }
            }
            catch
            {
            }
        }

        private void SaveSettings()
        {
            try
            {
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(settingsFileName, FileMode.OpenOrCreate, FileAccess.Write, isoStore))
                using (StreamWriter sw = new StreamWriter(isoStream))
                {
                    XElement element =
                        new XElement("config",
                            new XElement("appSettings",
                                new XElement("directory", txtInput.Text),
                                new XElement("output", txtOutput.Text)
                                )
                            );

                    sw.Write(element.ToString());
                }
            }
            catch
            {
            }
        }

        #endregion

        private void btnGo_Click(object sender, EventArgs e)
        {
            btnGo.Enabled = false;

            DirectoryInfo output = new DirectoryInfo(outputDir);
            output.DeleteAll();
            output.Create();
            output.CreateSubdirectory("bin");

            DirectoryInfo dir = new DirectoryInfo(txtInput.Text);

            foreach(var d in dir.GetDirectories())
            {
                if(d.Name.ToLower().StartsWith(prefix) && !d.Name.ToLower().Contains("rdi.service.plugins.example"))
                {
                    processDirectory(d);
                }
            }

            btnGo.Enabled = true;

            SaveSettings();
        }

        private void processDirectory(DirectoryInfo dir)
        {
            DirectoryInfo bin = new DirectoryInfo(Path.Combine(dir.FullName, configuration));
            if (bin.Exists)
            {
                foreach (var f in bin.GetFiles())
                {
                    if (f.Name.ToLower().StartsWith(prefix) && f.Name.ToLower() != "rdi.service.plugins.shared.dll")
                        File.Copy(f.FullName, Path.Combine(Path.Combine(outputDir, "bin"), f.Name), true);
                    else
                        File.Copy(f.FullName, Path.Combine(outputDir, f.Name), true);
                }
            }
        }
    }

    internal static class PluginDeployExtension
    {
        public static void DeleteAll(this DirectoryInfo dir)
        {
            if (!dir.Exists)
                return;

            foreach (var f in dir.GetFiles())
            {
                f.Delete();
            }

            foreach (var d in dir.GetDirectories())
            {
                d.DeleteAll();
            }

            dir.Delete();
        }
    }
}
