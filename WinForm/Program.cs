using System;
using System.Drawing;
using System.Windows.Forms;

namespace CHIP8EmulatorGUI
{
    public class MainForm : Form
    {
        private Panel displayPanel;
        private Button loadRomButton;
        private Button settingsButton;

        private const int Scale = 10; // Each CHIP-8 pixel will be 10x10
        private const int WidthPixels = 64;
        private const int HeightPixels = 32;

        public MainForm()
        {
            // Form properties
            this.Text = "CHIP-8 Emulator";
            this.ClientSize = new Size(WidthPixels * Scale, HeightPixels * Scale + 40);

            // Initialize buttons
            loadRomButton = new Button { Text = "Load ROM", Location = new Point(10, 0), Size = new Size(100, 30) };
            settingsButton = new Button { Text = "Settings", Location = new Point(120, 0), Size = new Size(100, 30) };

            // Buttons currently do nothing
            loadRomButton.Click += (s, e) => MessageBox.Show("Load ROM clicked!");
            settingsButton.Click += (s, e) => MessageBox.Show("Settings clicked!");

            // Add buttons to the form
            this.Controls.Add(loadRomButton);
            this.Controls.Add(settingsButton);

            // Panel to draw CHIP-8 display
            displayPanel = new Panel
            {
                Location = new Point(0, 40),
                Size = new Size(WidthPixels * Scale, HeightPixels * Scale),
                BackColor = Color.Black
            };

            this.Controls.Add(displayPanel);

            // Optional: paint event for drawing
            displayPanel.Paint += DisplayPanel_Paint;
        }

        private void DisplayPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Example: fill a pixel at (0,0) for testing
            g.FillRectangle(Brushes.White, 0, 0, Scale, Scale);
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
