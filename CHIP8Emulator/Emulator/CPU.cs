using System.Data;
using System.Runtime.Intrinsics.X86;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
[assembly: InternalsVisibleTo("CHIP8.Tests")]


public class CPU
{

    private Memory memory;
    private Display display;
    private Input input;
    public delegate void OpcodeHandler(ushort opcode);
    private Dictionary<int, OpcodeHandler> table;
    private byte[] Registers = new byte[16];
    private ushort[] Stack = new ushort[16];
    private byte Sp;
    public ushort Pc { get; private set; }
    public ushort Index { get; set; }
    public byte DelayTimer { get; private set; }
    public byte SoundTimer { get; private set; }


    public void DecrementTimers()
    {
        if (DelayTimer > 0) DelayTimer--;
        if (SoundTimer > 0) SoundTimer--;
    }



    internal byte GetRegister(int i) => Registers[i];
    internal void SetRegister(int i, byte value) => Registers[i] = value;
    internal ushort GetStackPointer() => Sp;
    internal ushort GetIndex() => Index;
    internal void SetIndex(ushort value) => Index = value;





    public CPU(Memory memory, Display display, Input input)
    {
        this.memory = memory;
        this.display = display;
        this.input = input;

        table = new Dictionary<int, OpcodeHandler>
        {
        { 0x0000, Execute0Group },
        { 0x1000, ExecuteJP },
        { 0x2000, ExecuteCALL },
        { 0x3000, ExecuteSE_Vx_Byte },
        { 0x4000, ExecuteSNE_Vx_Byte },
        { 0x5000, ExecuteSE_Vx_Vy },
        { 0x6000, ExecuteLD_Vx_Byte },
        { 0x7000, ExecuteADD_Vx_Byte },
        { 0x8000, Execute8Group },
        { 0x9000, ExecuteSNE_Vx_Vy },
        { 0xA000, ExecuteLD_I_Addr },
        { 0xB000, ExecuteJP_V0_Addr },
        { 0xC000, ExecuteRND_Vx_Byte },
        { 0xD000, ExecuteDRW_Vx_Vy_Nibble },
        { 0xE000, ExecuteEGroup },
        { 0xF000, ExecuteFGroup }
        };

    }

    public ushort FetchOpcode()
    {
        byte leftByte = memory.Read(Pc);
        byte rightByte = memory.Read(Pc + 1);
        return (ushort)((leftByte << 8) | rightByte);
    }

    public void SetPc(ushort address)
    {
        Pc = address;
    }
    public void ExecuteCycle()
    {
        ushort opcode = FetchOpcode(); // Fetchies
        OpcodeHandler handler = table[opcode & 0xF000];
        handler(opcode);
    }
    //GROUP HANDLERS
    private void Execute0Group(ushort opcode)
    {
        switch (opcode & 0x00FF)
        {
            case 0xE0:
                display.Clear();
                Pc += 2;
                break;

            case 0xEE:
                Pc = Stack[--Sp];
                break;

            default: // 0nnn or unknown
                Console.WriteLine($"Unknown 0x0 group opcode: 0x{opcode:X4}");
                Pc += 2;
                break;
        }
    }
    private void Execute8Group(ushort opcode)
    {
        int x = (opcode & 0x0F00) >> 8;
        int y = (opcode & 0x00F0) >> 4;
        byte vx = Registers[x];
        byte vy = Registers[y];
        switch (opcode & 0x000F)
        {

            case 0x0: Registers[x] = Registers[y]; break; //  LD Vx, Vy - Sætter værdien Vx= Vy
            case 0x1: Registers[x] |= Registers[y]; break; //  OR Vx, Vy -  Vx= Vx OR Vy
            case 0x2: Registers[x] &= Registers[y]; break; //  AND Vx, Vy - Vx= Vx AND Vy
            case 0x3: Registers[x] ^= Registers[y]; break; //  XOR Vx, Vy - Vx= Vx XOR Vy
            case 0x4: // ADD Vx, Vy with carry
                int sum = vx + vy;
                Registers[0xF] = (byte)(sum > 255 ? 1 : 0);
                Registers[x] = (byte)sum;
                break;
            case 0x5: // SUB Vx, By

                Registers[0xF] = (byte)(vx >= vy ? 1 : 0);
                Registers[x] = (byte)(vx - vy);
                break;
            case 0x6: // SHR Vx {, Vy}
                Registers[0xF] = (byte)(vx & 0X1);
                Registers[x] >>= 1;
                break;
            case 0x7: // SUBN Vx, Vy

                Registers[0xF] = (byte)(vy >= vx ? 1 : 0);
                Registers[x] = (byte)(vy - vx);
                break;
            case 0xE: // SHL Vx {, Vy}
                Registers[0xF] = (byte)((vx & 0X80) >> 7);
                Registers[x] <<= 1;
                break;
            default:
                Console.WriteLine($" unknows 0x800 opcode 0x{opcode:X4}");
                break;
        }
        Pc += 2;
    }

    private void ExecuteEGroup(ushort opcode)
    {
        int vx = (opcode & 0x0F00) >> 8;
        int key = Registers[vx];
        switch (opcode & 0x00FF)
        {
            case 0x9E: // SKP Vx
                if (input.IsKeyPressed(key))
                    Pc += 4;
                else
                    Pc += 2;
                break;
            case 0xA1: // SKNP Vx
                if (!input.IsKeyPressed(key))
                    Pc += 4;
                else
                    Pc += 2;
                break;
            default:
                Console.WriteLine($"unknown 0xE000 opcode: 0x{opcode:X4}");
                Pc += 2;
                break;
        }
    }




    private void ExecuteFGroup(ushort opcode)
    {
        int vx = (opcode & 0x0F00) >> 8;

        switch (opcode & 0x00FF)
        {
            case 0x07: // Fx07 - LD Vx, DT
                Registers[vx] = DelayTimer;
                Pc += 2;
                break;

            case 0x0A: // Fx0A - LD Vx, K
                {
                    int key = input.GetPressedKey();
                    if (key == -1)
                    {
                        return; // repeat this instruction (PC unchanged)
                    }

                    Registers[vx] = (byte)key;
                    Pc += 2;
                    break;
                }

            case 0x15: // Fx15 - LD DT, Vx
                DelayTimer = Registers[vx];
                Pc += 2;
                break;

            case 0x18: // Fx18 - LD ST, Vx
                SoundTimer = Registers[vx];
                Pc += 2;
                break;

            case 0x1E: // Fx1E - ADD I, Vx
                Index += Registers[vx];
                Pc += 2;
                break;

            case 0x29: // Fx29 - LD F, Vx
                Index = (ushort)(0x050 + Registers[vx] * 5);
                Pc += 2;
                break;

            case 0x33: // Fx33 - LD B, Vx
                byte vxs = Registers[vx];
                memory.Write(Index, (byte)(vxs / 100));
                memory.Write(Index + 1, (byte)((vxs / 10) % 10));
                memory.Write(Index + 2, (byte)(vxs % 10));
                Pc += 2;
                break;

            case 0x55: // Fx55 - LD [I], Vx
                       // Memory dump –
                for (int i = 0; i <= vx; i++)
                {
                    memory.Write(Index + i, Registers[i]);

                }
                Pc += 2;


                // Has quirk where index is incremented by 1 afterwards
                break;

            case 0x65: // Fx65 - LD Vx, [I]
                       // Memory load – 
                for (int i = 0; i <= vx; i++)
                {
                    Registers[i] = memory.Read(Index + i);

                }
                Pc += 2;
                // Has quirk where index is incremented by 1 afterwards
                break;
            default:
                Console.WriteLine($"Unknown 0xF000 opcode: 0x{opcode:X4}");
                Pc += 2;
                break;



        }
    }
    //individual opcodes
    private void ExecuteJP(ushort opcode) { Pc = (ushort)(opcode & 0x0FFF); }
    private void ExecuteCALL(ushort opcode)
    {
        Stack[Sp++] = (ushort)(Pc + 2);
        Pc = (ushort)(opcode & 0x0FFF);
    }
    private void ExecuteSE_Vx_Byte(ushort opcode)
    {
        if (Registers[(opcode & 0x0F00) >> 8] == (byte)(opcode & 0x00FF))
            Pc += 4;
        else
            Pc += 2;
    }
    private void ExecuteSNE_Vx_Byte(ushort opcode)
    {
        if (Registers[(opcode & 0x0F00) >> 8] != (byte)(opcode & 0x00FF))
            Pc += 4;
        else
            Pc += 2;
    }
    private void ExecuteSE_Vx_Vy(ushort opcode)
    {
        if (Registers[(opcode & 0x0F00) >> 8] == Registers[(opcode & 0x00F0) >> 4])
            Pc += 4;
        else
            Pc += 2;
    }
    private void ExecuteLD_Vx_Byte(ushort opcode)
    {
        Registers[(opcode & 0x0F00) >> 8] = (byte)(opcode & 0x00FF);
        Pc += 2;
    }
    private void ExecuteADD_Vx_Byte(ushort opcode)
    {
        Registers[(opcode & 0x0F00) >> 8] += (byte)(opcode & 0x00FF);
        Pc += 2;
    }
    private void ExecuteSNE_Vx_Vy(ushort opcode)
    {
        if (Registers[(opcode & 0x0F00) >> 8] != Registers[(opcode & 0x00F0) >> 4])
            Pc += 4;
        else
            Pc += 2;
    }
    private void ExecuteLD_I_Addr(ushort opcode)
    {
        Index = (ushort)(opcode & 0x0FFF);
        Pc += 2;
    }
    private void ExecuteJP_V0_Addr(ushort opcode)
    {
        Pc = (ushort)((opcode & 0x0FFF) + Registers[0]);
    }
    private void ExecuteRND_Vx_Byte(ushort opcode)
    {
        Random random = new();
        Registers[(opcode & 0x0F00) >> 8] = (byte)(random.Next(0, 256) & (opcode & 0x00FF));
        Pc += 2;
    }
    //DXYN
    private void ExecuteDRW_Vx_Vy_Nibble(ushort opcode)
    {
        byte x = Registers[(opcode & 0x0F00) >> 8];
        byte y = Registers[(opcode & 0x00F0) >> 4];
        byte height = (byte)(opcode & 0x00F);
        Registers[0xF] = 0; // collision flag


        //loop gennem være byte (row) af spriten
        for (int row = 0; row < height; row++)
        {

            //læser sprite fra memory starter på index
            byte spriteByte = memory.Read(Index + row);

            for (int collision = 0; collision < 8; collision++)
            {

                //tjekker hvis nuværende pixels i sprite er tænxdt
                bool spritePixel = (spriteByte & (0x80 >> collision)) != 0;
                if (spritePixel)
                {
                    bool _collision = display.TogglePixel(x + collision, y + row);

                    //hvis collision skær VF = 1
                    if (_collision) Registers[0xF] = 1;
                }
            }

        }
        Pc += 2;

    }







}