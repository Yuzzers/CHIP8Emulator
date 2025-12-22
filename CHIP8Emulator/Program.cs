var Chip8 = new Chip8();
byte[] rom = Romloader.LoadRom(@"C:\Users\Nexsy\CHIP8Emulator\ROMs\IBM Logo.ch8");
Chip8.LoadRom(rom);

Console.WriteLine(" 16 bytes in memory at 0x200");

    for (int i =0; i <16; i++)
{
    Console.Write($"{Chip8.memory.Read(0x200 + i):X2} ");
}
Console.WriteLine($"\nCurrent PC: 0x{Chip8.Cpu.Pc:X3}");



        int characterToCheck = 2; // 0–F (hex) 

        Console.WriteLine($"Checking font for character {characterToCheck:X}:");

        // Each char is 5 bytes, stored sequentially starts at 0x050
        int startAddress = 0x050 + characterToCheck * 5;

        for (int i = 0; i < 5; i++)
        {
            byte b = Chip8.memory.Read(startAddress + i);
            Console.WriteLine($"Byte {i}: 0x{b:X2}");
        }


Chip8.Cpu.Reg[0] = 0;
Chip8.Cpu.Reg[1] = 0;
Chip8.Cpu.Index = (ushort)startAddress;

ushort DrawOIpcode = 0xD015;
Chip8.Cpu.ExecuteDxyn(DrawOIpcode);

Console.WriteLine("\nDrawing font in console:");
for (int y = 0; y < Display.Height; y++)
{
    for (int x = 0; x < Display.Width; x++)
    {
        Console.Write(Chip8.Display.getPixel(x, y) ? "#" : " ");
    }
    Console.WriteLine();
}

Chip8.Input.SetKey(0x5, true);
Chip8.Cpu.ExecuteCycle();
Chip8.Input.SetKey(0x5, false);
