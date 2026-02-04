using System.Data;
using System.Runtime.Intrinsics.X86;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("CHIP8.Tests")]


public class CPU
{

    private Memory memory;
    private Display display;
    private Input input;


    private byte[] Registers = new byte[16];

    private ushort[] Stack = new ushort[16];
    private byte Sp;
    public ushort Pc{get; private set;} 
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
        this.display=display;
        this.input=input;

    }

    public ushort FetchOpcode()
    {
        byte leftByte= memory.Read(Pc);
        byte rightByte= memory.Read(Pc+1);
        return (ushort)((leftByte << 8) | rightByte);
    }



    public void SetPc(ushort address)
    {
        Pc = address;
    }
// dxyn opcode, tegner en sprite på koordinearne (vx, vy)
// sætter VF til 1 hvis en pixel er ændret fra tændt til utent (collision)
    public void ExecuteDxyn(ushort opcode)
    {
        byte x = Registers[(opcode & 0x0F00) >>8];
        byte y = Registers[(opcode & 0x00F0) >>4];
        byte height = (byte) (opcode & 0x00F);
        Registers[0xF] =0; // collision flag


        //loop gennem være byte (row) af spriten
        for(int row=0; row < height; row++)
        {

            //læser sprite fra memory starter på index
            byte spriteByte = memory.Read(Index+row);

            for (int collision =0; collision <8;collision++)
            {

                //tjekker hvis nuværende pixels i sprite er tænxdt
                bool spritePixel = (spriteByte & (0x80 >> collision)) !=0;
                if (spritePixel)
                {
                    bool _collision = display.TogglePixel(x+collision, y+row);

                    //hvis collision skær VF = 1
                    if (_collision) Registers[0xF]=1;
                }
            }

        }
        Pc +=2;

    }

    public void ExecuteCycle()
    {
        ushort Opcode = FetchOpcode(); // Fetchies

        //motherload of switches for Executie and decoding
        // ORs the first nibble to identify instruction type
            switch (Opcode & 0xF000)
        {
           case 0x0000:
                switch (Opcode & 0x00FF)//look at the last byte for E0 and EE
                {
                    case 0xE0: // CLS - Clear Display
                    display.Clear();
                    Pc +=2;
                    break;

                    case 0xEE: // RET - Return from subroutine
                    Pc = Stack[--Sp];
                    break;
                    default:
                    Console.WriteLine($"unknoiwing 0x0000 opcode: 0x{Opcode:X4}");
                    Pc +=2;
                    break;
                } 
                break;
            case 0x1000: // 1nnn - Jump to adress nnn
                Pc = (ushort)(Opcode & 0x0FFF);
                break;
            case 0x2000: // 2nnn - Call nnn
                Stack[Sp++] = (ushort)(Pc+2);
                Pc = (ushort)(Opcode & 0x0FFF);
                break;
            case 0x3000: //3xkk - Skip næste insturktion hvis Vx ==kk
                if (Registers[(Opcode & 0x0F00) >> 8] ==(byte)(Opcode & 0x00FF))
                Pc+=4;
                else
                Pc +=2;
                break;
            case 0x4000: // 4xkk - Skip næste insturktiun hvis Vx != kk
                    if (Registers[(Opcode & 0x0F00) >> 8] !=(byte)(Opcode & 0x00FF))
                Pc+=4;
                else
                Pc +=2;
                break;
            case 0x5000: // 5xy0 - Skip næstese instruktion hvis Vx == VY
                    if (Registers[(Opcode & 0x0F00) >> 8] == Registers[(Opcode & 0x00F0) >> 4])
                Pc+=4;
                else
                Pc +=2;
                break;
            case 0x6000:// 6xkk -Sætter værdien kk ind i register Vx
                Registers[(Opcode & 0x0F00) >> 8] = (byte)(Opcode & 0x00FF);
                Pc+=2;
                break;
            case 0x7000: // 7xkk - sætter Vx=Vx+kk
                Registers[(Opcode & 0x0F00) >>8] += (byte)(Opcode & 0x00FF);
                Pc +=2;
                break;
            case 0x8000: // 0xy? matematik pis
                byte x = (byte)((Opcode & 0x0F00) >>8);
                byte y = (byte)((Opcode & 0x00F0) >>4);
                switch (Opcode & 0x000F)
                {
                    case 0x0:Registers[x] = Registers[y]; break; //  LD Vx, Vy - Sætter værdien Vx= Vy
                    case 0x1:Registers[x] |= Registers[y]; break; //  OR Vx, Vy -  Vx= Vx OR Vy
                    case 0x2:Registers[x] &= Registers[y]; break; //  AND Vx, Vy - Vx= Vx AND Vy
                    case 0x3:Registers[x] ^= Registers[y]; break; //  XOR Vx, Vy - Vx= Vx XOR Vy
                    case 0x4: // ADD Vx, Vy with carry
                        int sum = Registers[x] + Registers[y];
                        Registers[0xF] = (byte)(sum > 255 ? 1:0);
                        Registers[x] = (byte)sum;
                        break;
                    case 0x5: // SUB Vx, By
                        Registers[0XF] = (byte)(Registers[x] > Registers[y] ? 1 : 0);
                        Registers[x] -= Registers[y];
                        break;
                    case 0x6: // SHR Vx {, Vy}
                        Registers[0xF] = (byte)(Registers[x] & 0X1);
                        Registers[x] >>= 1;
                        break;
                    case 0x7: // SUBN Vx, Vy
                        Registers[0xF] = (byte)(Registers[y] > Registers[x] ? 1:0);
                        Registers[x] = (byte)(Registers[y] - Registers[x]);
                        break;
                    case 0xE: // SHL Vx {, Vy}
                        Registers[0xF] = (byte)((Registers[x] & 0X80) >> 7);
                        Registers[x] <<= 1;
                        break;
                    default:
                        Console.WriteLine($" unknows 0x800 opcode 0x{Opcode:X4}");
                        break;
                }
                Pc +=2;
                break;
            case 0x9000: //9xy0
                if(Registers[(Opcode& 0x0F00) >> 8] != Registers[(Opcode & 0x00F0) >> 4])
                Pc +=4;
                else
                Pc+=2;
                break;
            case 0xA000://Annn
                Index = (ushort)(Opcode & 0x0FFF);
                Pc += 2;
                break;
            case 0xB000://Bnnn
            Pc = (ushort)((Opcode & 0x0FFF) + Registers[0]);
            break;
            case 0xC000://cxkk
            Random random = new();
            Registers[(Opcode & 0x0F00) >> 8] = (byte)(random.Next(0, 256) & (Opcode & 0x00FF));
            Pc +=2;
            break;
            case 0xD000://dxyn
            ExecuteDxyn(Opcode);
            break;
            case 0xE000:
                {
                    int vx = (Opcode & 0x0F00) >> 8;
                    int key = Registers[vx];
                    switch (Opcode & 0x00FF)
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
                            Console.WriteLine($"unknown 0xE000 opcode: 0x{Opcode:X4}");
                            Pc += 2;
                            break;
                    }
                }
            break;
            case 0xF000:
                {
                int vx = (Opcode & 0x0F00) >> 8;
                switch (Opcode & 0x00FF)
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
                    byte xs = (byte)((Opcode & 0x0F00) >> 8);
                    byte vxs = Registers[xs];
                    memory.Write(Index,      (byte)(vxs / 100));
                    memory.Write(Index + 1,  (byte)((vxs / 10) % 10));
                    memory.Write(Index + 2,  (byte)(vxs % 10));
                    Pc += 2;
                    break;

                case 0x55: // Fx55 - LD [I], Vx
                    // Memory dump –
                    Pc += 2;
                    break;

                case 0x65: // Fx65 - LD Vx, [I]
                    // Memory load – 
                    Pc += 2;
                    break;

                default:
                    Console.WriteLine($"Unknown 0xF000 opcode: 0x{Opcode:X4}");
                    Pc += 2;
                    break;
                    }
                break;
                }   
        }
    }
}



