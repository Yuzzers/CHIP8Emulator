namespace CHIP8Emulator
{


    public class Chip8
    {

        private readonly CPU cpu;
        private readonly Memory memory;
        private readonly Display display;
        private readonly Input input;

        private const ushort ProgramstartAddress = 0x200;
        private const ushort FontStartAddress = 0x050;
        public static readonly byte[] FontSet = new byte[]

        {

            0xF0,0x90,0x90,0x90,0xF0, // 0
            0x20,0x60,0x20,0x20,0x70, // 1
            0xF0,0x10,0xF0,0x80,0xF0, // 2
            0xF0,0x10,0xF0,0x10,0xF0, // 3
            0x90,0x90,0xF0,0x10,0x10, // 4
            0xF0,0x80,0xF0,0x10,0xF0, // 5
            0xF0,0x80,0xF0,0x90,0xF0, // 6
            0xF0,0x10,0x20,0x40,0x40, // 7
            0xF0,0x90,0xF0,0x90,0xF0, // 8
            0xF0,0x90,0xF0,0x10,0xF0, // 9
            0xF0,0x90,0xF0,0x90,0x90, // A
            0xE0,0x90,0xE0,0x90,0xE0, // B
            0xF0,0x80,0x80,0x80,0xF0, // C
            0xE0,0x90,0x90,0x90,0xE0, // D
            0xF0,0x80,0xF0,0x80,0xF0, // E
            0xF0,0x80,0xF0,0x80,0x80, // F    

        };
        public byte DelayTimer => cpu.DelayTimer;
        public byte SoundTimer => cpu.SoundTimer;
        public Chip8()
        {

            memory = new Memory();
            display = new Display();
            input = new Input();
            cpu = new CPU(memory, display, input);

            LoadFontSet();

        }

        public void ExecuteCycle()
        {
            cpu.ExecuteCycle();
        }

        public void DecrementTimers()
        {
            cpu.DecrementTimers();
        }

        private void LoadFontSet()
        {
            memory.Load(FontSet, FontStartAddress);
        }


        public void LoadRom(byte[] rom)
        {
            memory.Load(rom, ProgramstartAddress);
            cpu.SetPc(ProgramstartAddress);

        }


        public ReadOnlySpan<bool> Pixels => display.Buffer;

        public int ScreenWidth => Display.Width;
        public int ScreenHeight => Display.Height;


    }
}








