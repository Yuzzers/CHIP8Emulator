using System;
using System.Drawing;
using System.Windows.Forms;
using CHIP8Emulator;
using System.IO;
using System.Media;

namespace CHIP8EmulatorGUI
{
    public class MainForm : Form
    {
        private Chip8 emulator;
        private Panel displayPanel;
        private const int Scale = 10;
        private int MapKey(Keys key)
        {
            switch (key)
            {
                case Keys.D1: return 0x1;
                case Keys.D2: return 0x2;
                case Keys.D3: return 0x3;
                case Keys.D4: return 0xC;
                case Keys.Q: return 0x4;
                case Keys.W: return 0x5;
                case Keys.E: return 0x6;
                case Keys.R: return 0xD;
                case Keys.A: return 0x7;
                case Keys.S: return 0x8;
                case Keys.D: return 0x9;
                case Keys.F: return 0xE;
                case Keys.Z: return 0xA;
                case Keys.X: return 0x0;
                case Keys.C: return 0xB;
                case Keys.V: return 0xF;
                default: return -1;
            }
        }

        public MainForm()
        {
            this.Text = "CHIP-8 Emulator";
            this.ClientSize = new Size(64 * Scale, 32 * Scale);

            this.KeyDown += KeyDownForm;
            this.KeyUp += KeyUpForm;
            this.KeyPreview = true;

            emulator = new Chip8();

            displayPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(64 * Scale, 32 * Scale),
                BackColor = Color.Black
            };
            displayPanel.Paint += DisplayPanel_Paint;
            this.Controls.Add(displayPanel);
            this.Focus();

            // Load ROM at startup
            LoadRomAtStartup();

            // Timer for emulator + redraw
            var timer = new System.Windows.Forms.Timer();

            timer.Interval = 1000 / 60; // 60Hz
            timer.Tick += (s, e) =>
            {
                // Run multiple CPU cycles per tick
                for (int i = 0; i < 10; i++)
                    emulator.ExecuteCycle();

                // Decrement timers once per tick
                emulator.DecrementTimers();

                if (emulator.ShouldBeep())
                {
                    Console.Beep(440, 1000 / 60);
                }



                if (emulator.DisplayChanged)
                {
                    displayPanel.Invalidate();
                    emulator.DisplayChanged = false;
                }

            };
            timer.Start();
        }

        private void LoadRomAtStartup()
        {
            using OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Select CHIP-8 ROM",
                Filter = "CHIP-8 ROM (*.ch8;*.rom)|*.ch8;*.rom|All files (*.*)|*.*"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    byte[] rom = Romloader.LoadRom(ofd.FileName);
                    emulator.LoadRom(rom);
                }
                catch (FileNotFoundException ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close(); // Exit app if ROM not found
                }
            }
            else
            {
                // No file selected
                MessageBox.Show("No ROM selected, exiting.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
        }

        private void DisplayPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.Black);

            var pixels = emulator.Pixels;
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    if (pixels[y * 64 + x])
                        g.FillRectangle(Brushes.White, x * Scale, y * Scale, Scale, Scale);
                }
            }
        }
        private void KeyDownForm(object sender, KeyEventArgs e)
        {
            int key = MapKey(e.KeyCode);
            if (key != -1)
                emulator.input.SetKey(key, true);
        }
        private void KeyUpForm(object sender, KeyEventArgs e)
        {
            int key = MapKey(e.KeyCode);
            if (key != -1)
                emulator.input.SetKey(key, false);
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}




