using System;
using System.Drawing;
using System.Windows.Forms;
using CHIP8Emulator;
using System.IO;

namespace CHIP8EmulatorGUI
{
    public class MainForm : Form
    {
        private Chip8 emulator;
        private Panel displayPanel;
        private const int Scale = 10;

        public MainForm()
        {
            this.Text = "CHIP-8 Emulator";
            this.ClientSize = new Size(64 * Scale, 32 * Scale);

            emulator = new Chip8();

            displayPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(64 * Scale, 32 * Scale),
                BackColor = Color.Black
            };
            displayPanel.Paint += DisplayPanel_Paint;
            this.Controls.Add(displayPanel);

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


                displayPanel.Invalidate();
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

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}




