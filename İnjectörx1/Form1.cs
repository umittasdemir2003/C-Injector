using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

namespace İnjectörx1
{
    public partial class Form1 : Form
    {
        static readonly IntPtr INTPTR_ZERO = (IntPtr)0;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInherİtHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr Hmodule, string lpProcName);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwsize, uint flAllocationType, uint flProtect);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int WriteProcessMemory(IntPtr hProcces, IntPtr lpBaseAddres, byte[] buffer, uint size, int lpNumberOfBytesWritten);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddres,
            IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);
       

        public Form1()
        {
            InitializeComponent();
        }

        public enum DllInjectionResult
        {
            DllNotFound,
            GameProcessNotFound,
            InjectionFailed,
            Success
        }

        public DllInjectionResult Inject(String sProcName,string sDLLPath)
        {
            if (!File.Exists(sDLLPath))
            {
                return DllInjectionResult.DllNotFound;
            }

            uint _procId = 0;

            Process[] _procs = Process.GetProcesses();
            for (int i = 0; i < _procs.Length; i++)
            {
                if (_procs[i].ProcessName == sProcName)
                {
                    _procId = (uint)_procs[i].Id;
                    break;
                }

            }

            if (_procId == 0)
            {
                return DllInjectionResult.GameProcessNotFound;
            }

            if (bInject(_procId, sDLLPath))
            {
                return DllInjectionResult.InjectionFailed;
            }

            return DllInjectionResult.Success;

        }

        bool bInject(uint pToBeInjected, string sDllPath)
        {
            IntPtr hnPro = OpenProcess((0x2 | 0x8 | 0x10 | 0x20 | 0x400), 1, pToBeInjected);

            if (hnPro == INTPTR_ZERO)
            {
                return false;
            }

            IntPtr lpAddress = VirtualAllocEx(hnPro, (IntPtr)null, (IntPtr)sDllPath.Length, (0x1000 | 0x2000), 0x40);

            if (lpAddress == INTPTR_ZERO)
            {
                return false;
            }

            byte[] bytes = Encoding.ASCII.GetBytes(sDllPath);

            if (WriteProcessMemory(hnPro, lpAddress, bytes, (uint)bytes.Length, 0)== 0)
            {
                return false;
            }

            if (CreateRemoteThread(hnPro, (IntPtr)null, INTPTR_ZERO, lpAddress, lpAddress, 0, (IntPtr)null)== INTPTR_ZERO)
            {
                return false;
            }

            CloseHandle(hnPro);
            return true;
        }

        private void injectMode()
        {
            if (checkBox1.Checked)
            {
                Process[] ProcessCollection = Process.GetProcesses();
                foreach (Process p in ProcessCollection)
                {
                    if (p.ProcessName == PnameT.Text.ToString() && p.MainWindowTitle != "")
                    {
                        Inject(PnameT.Text, DllPathT.Text);
                        Console.Beep();
                        chAutoInject.Checked = false;
                        timer1.Stop();
                        break;

                    }
                } 
            }
            else
            {
                Process[] processCollection = Process.GetProcesses();
                foreach(Process p in processCollection)
                {
                    if (p.ProcessName == PnameT.Text.ToString())
                    {
                        Inject(PnameT.Text, DllPathT.Text);
                        Console.Beep();
                        chAutoInject.Checked = false;
                        timer1.Stop();
                        break;
                    }
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            injectMode();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (chAutoInject.Checked)
            {
                timer1.Start();
                return;
            }
            timer1.Stop();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.Items.Count !=0)
            {
                PnameT.Text = listBox1.SelectedItem.ToString();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            statusT.Clear();
            injectMode();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            Process[] processes = Process.GetProcesses();
            for (int i = 0; i < processes.Length; i ++)
            {
                Process p = processes[i];
                listBox1.Items.Add(p.ProcessName);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                DllPathT.Text = openFileDialog1.FileName;
            }
        }
    }
}
