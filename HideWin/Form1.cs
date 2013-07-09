using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HideWin
{
    public partial class Form1 : Form
    {
        private const int SW_HIDE = 0;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_SHOWNOACTIVATE = 4;
        private const int SW_RESTORE = 9;
        private const int SW_SHOWDEFAULT = 10;

        Timer timer = new Timer();
        NotifyIcon notificationIcon;
        MenuItem exitItem = new MenuItem();
        ContextMenu contextMenu = new ContextMenu();
        List<string> hiddenAppNames = new List<string>();
        Dictionary<String, IntPtr> processDictionary = new Dictionary<string, IntPtr>();
        
        public Form1()
        {
            InitializeComponent();
            this.components = new System.ComponentModel.Container();
            notificationIcon = new NotifyIcon(this.components);
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;

            exitItem.Click += new EventHandler(exitItem_Click);
            exitItem.Text = "Exit";
            Bitmap bmp = HideWin.Properties.Resources.arrow_incident_blue_24_ns;
            notificationIcon.Icon = Icon.FromHandle(bmp.GetHicon());
            notificationIcon.Visible = true;
            notificationIcon.ContextMenu = new System.Windows.Forms.ContextMenu();
            RefreshProcesses();
            
            timer.Interval = 100;
            timer.Enabled = true;
            timer.Tick += new EventHandler(t_Tick);
            timer.Start();
            
        }

        void exitItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        void t_Tick(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcesses();
            List<String> oldList = new List<String>();
            List<String> newList=new List<String>();
            oldList.AddRange(processDictionary.Keys);

            foreach (Process item in processes)
            {
                if (item.MainWindowTitle.Length != 0||hiddenAppNames.Contains(item.ProcessName))
                {
                    newList.Add(item.ProcessName);
                }
            }

            var s1 = newList.Except(oldList);
            var s2 = oldList.Except(newList);
            
            if(s1.Count()!=0||s2.Count()!=0)
            {
                lock (processDictionary)
                {
                    RefreshProcesses();
                }
            }
        }
        
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        private void RefreshProcesses() 
        {
            notificationIcon.ContextMenu.MenuItems.Clear();
            processDictionary.Clear();
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (process.MainWindowTitle.Length != 0 || hiddenAppNames.Contains(process.ProcessName))
                {
                    try 
	                {
                        processDictionary.Add(process.ProcessName,process.MainWindowHandle);
                        MenuItem menuItem = new MenuItem();
                        menuItem.Text = process.ProcessName;
                        menuItem.Click += new EventHandler(m_Click);
                        notificationIcon.ContextMenu.MenuItems.Add(menuItem);
	                }
	                catch (Exception)
	                {
		                
	                }
                }
            }
            notificationIcon.ContextMenu.MenuItems.Add(exitItem);
        }

        void m_Click(object sender, EventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem.Checked == false)
            {
                ShowWindow(processDictionary[menuItem.Text], SW_HIDE);
                hiddenAppNames.Add(menuItem.Text);
                menuItem.Checked = true;
            }
            else 
            {
                ShowWindow(processDictionary[menuItem.Text], SW_SHOWNORMAL);
                hiddenAppNames.Remove(menuItem.Text);
                menuItem.Checked = false;
            }
        }
    }
}
