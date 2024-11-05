using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Win32;

namespace NUMPUD
{
    public partial class Form1 : Form
    {
        private Dictionary<Keys, string> keyBindings = new Dictionary<Keys, string>();
        private Dictionary<int, Keys> registeredHotkeys = new Dictionary<int, Keys>();
        private const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public Form1()
        {
            InitializeComponent();
            LoadBindingsFromFile();
            SetupSystemTray();
            RegisterHotkeys();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                if (registeredHotkeys.ContainsKey(id))
                {
                    Keys key = registeredHotkeys[id];
                    if (keyBindings.ContainsKey(key))
                    {
                        LaunchApplication(keyBindings[key]);
                    }
                }
            }
        }

        private void RegisterHotkeys()
        {
            int id = 0;
            foreach (var binding in keyBindings)
            {
                RegisterHotKey(this.Handle, id, 0, (uint)binding.Key);
                registeredHotkeys[id] = binding.Key;
                id++;
            }
        }

        private void UnregisterHotkeys()
        {
            foreach (var id in registeredHotkeys.Keys)
            {
                UnregisterHotKey(this.Handle, id);
            }
            registeredHotkeys.Clear();
        }

        private void LaunchApplication(string appPath)
        {
            try
            {
                if (Uri.IsWellFormedUriString(appPath, UriKind.Absolute))
                {
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {appPath}") { CreateNoWindow = true });
                }
                else
                {
                    Process.Start(appPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при запуске приложения: " + ex.Message);
            }
        }

        private void SaveBinding(Keys key, string appPath)
        {
            if (string.IsNullOrWhiteSpace(appPath))
            {
                keyBindings.Remove(key);
            }
            else
            {
                keyBindings[key] = appPath;
            }
            UnregisterHotkeys();
            SaveBindingsToFile();
            UpdateTextBoxes();
            RegisterHotkeys();
            MessageBox.Show($"Привязка сохранена: {key} -> {appPath}");
        }


        private void SaveBindingsToFile()
        {
            var doc = new XmlDocument();
            var root = doc.CreateElement("KeyBindings");
            doc.AppendChild(root);

            foreach (var binding in keyBindings)
            {
                var keyBindingElement = doc.CreateElement("KeyBinding");
                keyBindingElement.SetAttribute("Key", binding.Key.ToString());
                keyBindingElement.SetAttribute("Path", binding.Value);
                root.AppendChild(keyBindingElement);
            }

            doc.Save("keybindings.xml");
        }

        private void LoadBindingsFromFile()
        {
            if (!System.IO.File.Exists("keybindings.xml"))
                return;

            var doc = new XmlDocument();
            doc.Load("keybindings.xml");

            foreach (XmlNode node in doc.SelectNodes("//KeyBinding"))
            {
                var key = (Keys)Enum.Parse(typeof(Keys), node.Attributes["Key"].Value);
                var path = node.Attributes["Path"].Value;
                keyBindings[key] = path;
            }

            UpdateTextBoxes();
        }

        private void UpdateTextBoxes()
        {
            textBox1.Text = keyBindings.ContainsKey(Keys.F2) ? keyBindings[Keys.F2] : string.Empty;
            textBox2.Text = keyBindings.ContainsKey(Keys.F3) ? keyBindings[Keys.F3] : string.Empty;
            textBox3.Text = keyBindings.ContainsKey(Keys.F4) ? keyBindings[Keys.F4] : string.Empty;
            textBox4.Text = keyBindings.ContainsKey(Keys.F5) ? keyBindings[Keys.F5] : string.Empty;
            textBox5.Text = keyBindings.ContainsKey(Keys.F6) ? keyBindings[Keys.F6] : string.Empty;
            textBox6.Text = keyBindings.ContainsKey(Keys.F7) ? keyBindings[Keys.F7] : string.Empty;
            textBox7.Text = keyBindings.ContainsKey(Keys.F8) ? keyBindings[Keys.F8] : string.Empty;
            textBox8.Text = keyBindings.ContainsKey(Keys.F9) ? keyBindings[Keys.F9] : string.Empty;
            textBox9.Text = keyBindings.ContainsKey(Keys.F10) ? keyBindings[Keys.F10] : string.Empty;
            textBox10.Text = keyBindings.ContainsKey(Keys.F11) ? keyBindings[Keys.F11] : string.Empty;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveBinding(Keys.F2 , textBox1.Text);
        }


        private void button2_Click(object sender, EventArgs e)
        {
            SaveBinding(Keys.F3, textBox2.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveBinding(Keys.F4, textBox3.Text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveBinding(Keys.F5, textBox4.Text);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SaveBinding(Keys.F6, textBox5.Text);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            SaveBinding(Keys.F7, textBox6.Text);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            SaveBinding(Keys.F8, textBox7.Text);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            SaveBinding(Keys.F9, textBox8.Text);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            SaveBinding(Keys.F10, textBox9.Text);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            SaveBinding(Keys.F11, textBox10.Text);
        }


        private void SetupSystemTray()
        {
            notifyIcon1.Text = "NUMPUD";
            notifyIcon1.Icon = SystemIcons.Application;
            notifyIcon1.Visible = true;
            notifyIcon1.DoubleClick += (s, e) => {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            };

            this.Resize += (s, e) => {
                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.Hide();
                }
            };

            var contextMenu = new ContextMenuStrip();
            var exitMenuItem = new ToolStripMenuItem("Выход");
            exitMenuItem.Click += (s, e) => {
                notifyIcon1.Visible = false;
                Application.Exit();
            };
            contextMenu.Items.Add(exitMenuItem);
            notifyIcon1.ContextMenuStrip = contextMenu;
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox11_TextChanged(object sender, EventArgs e)
        {

        }
        private void textBox_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
