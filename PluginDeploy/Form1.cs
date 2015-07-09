using System;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;
using bche.SettingsManager;

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

        private readonly SettingsManager Settings;

        public Form1()
        {
            Settings= new SettingsManager();

            InitializeComponent();

            LoadSettings();
        }

        #region Save/Load Settings

        private void LoadSettings()
        {
            Settings.LoadSettings();
            txtInput.Text = Settings["directory"];
            txtOutput.Text = Settings["output"];
        }

        private void SaveSettings()
        {
            Settings["directory"] = txtInput.Text;
            Settings["output"] = txtOutput.Text;
            Settings.SaveSettings();
        }

        #endregion

        private void btnGo_Click(object sender, EventArgs e)
        {
            btnGo.Enabled = false;

            try
            {
                DirectoryInfo dir = new DirectoryInfo(txtInput.Text);

                if (!dir.Exists)
                {
                    MessageBox.Show("The input directory does not exist");
                    txtInput.Focus();
                    return;
                }

                DirectoryInfo output = new DirectoryInfo(outputDir);
                output.DeleteAll();
                output.Create();
                output.CreateSubdirectory("bin");

                foreach (var d in dir.GetDirectories())
                {
                    if (d.Name.ToLower().StartsWith(prefix) && !d.Name.ToLower().Contains("rdi.service.plugins.example"))
                    {
                        processDirectory(d);
                    }
                }

                SaveSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                btnGo.Enabled = true;
            }
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
