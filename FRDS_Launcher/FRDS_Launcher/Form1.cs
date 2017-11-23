using Open.Nat;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FRDS_Launcher
{
    public partial class Form1 : Form
    {
        public static string BasePath = AppDomain.CurrentDomain.BaseDirectory;
        public static string ModPath = BasePath + "firefightreloaded/";
        public static string EXEPath = BasePath + "fr.exe";
        public static string MapPath = ModPath + "maps/";

        public Form1()
        {
            InitializeComponent();
        }

        public async void StartUPNP()
        {
           var nat = new NatDiscoverer();

           var cts = new CancellationTokenSource(5000);
           var device = await nat.DiscoverDeviceAsync(PortMapper.Upnp, cts);
           await device.CreatePortMapAsync(new Mapping(Protocol.Udp, Convert.ToInt32(textBox2.Text), Convert.ToInt32(textBox2.Text), "FIREFIGHT RELOADED"));

           var ip = await device.GetExternalIPAsync();
        }

        public async void StopUPNP()
        {
            var nat = new NatDiscoverer();
            var cts = new CancellationTokenSource(5000);
            var device = await nat.DiscoverDeviceAsync(PortMapper.Upnp, cts);

            foreach (var mapping in await device.GetAllMappingsAsync())
            {
                if (mapping.Description.Contains("FIREFIGHT RELOADED"))
                {
                    await device.DeletePortMapAsync(mapping);
                }
            }

            var ip = await device.GetExternalIPAsync();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                StartUPNP();
            }

            string args = "-console -game firefightreloaded -steam -textmode -port " + textBox2.Text + " +maxplayers " + numericUpDown1.Value.ToString() + " +map " + listBox1.SelectedItem.ToString() + " +hostname " + textBox1.Text;

            Process client = new Process();
            client.StartInfo.FileName = EXEPath;
            client.StartInfo.Arguments = args;
            client.EnableRaisingEvents = true;
            client.Exited += new EventHandler(ServerExited);
            client.Start();
        }

        void ServerExited(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                StopUPNP();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string mapdir = MapPath;
            DirectoryInfo dinfo = new DirectoryInfo(mapdir);
            FileInfo[] Files = dinfo.GetFiles("*.bsp");
            foreach (FileInfo file in Files)
            {
                listBox1.Items.Add(file.Name);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (checkBox1.Checked == true)
            {
                StopUPNP();
            }

            Process[] fr = Process.GetProcessesByName("fr");
            if (fr != null)
            {
                foreach (var process in fr)
                {
                    process.Kill();
                }
            }
        }
    }
}
