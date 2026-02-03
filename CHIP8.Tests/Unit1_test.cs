using Xunit;

namespace CHIP8.Tests;

public class UnitTest1
{
    [Fact]
    public void Execute_8XY4_Sets_Carry_Flag_On_Overflow()
    {
        var memory = new Memory();
        var display = new Display();
        var input = new Input();

        var cpu = new CPU(memory, display, input);
        cpu.SetPc(0x200);
        cpu.SetRegister(1, 200);
        cpu.SetRegister(2, 100);

        // Load 8XY4 opcode: ADD V1, V2 at 0x200
        memory.Load(new byte[] { 0x81, 0x24 }, 0x200);

        cpu.ExecuteCycle();

        Assert.Equal((byte)44, cpu.GetRegister(1)); 
        Assert.Equal(1, cpu.GetRegister(0xF));
    }

    [Fact]
    public void Execute_6XKK_Sets_Register()
    {
        var memory = new Memory();
        var display = new Display();
        var input = new Input();

        var cpu = new CPU(memory, display, input);
        cpu.SetPc(0x200);

        memory.Load(new byte[] { 0x61, 0xAB }, 0x200); // LD V1, 0xAB

        cpu.ExecuteCycle();

        Assert.Equal(0xAB, cpu.GetRegister(1));
    }

    [Fact]
    public void Execute_7XKK_Adds_To_Register_Without_Carry()
    {
        var memory = new Memory();
        var display = new Display();
        var input = new Input();

        var cpu = new CPU(memory, display, input);
        cpu.SetPc(0x200);
        cpu.SetRegister(1, 50);

        memory.Load(new byte[] { 0x71, 0x0A }, 0x200); // ADD V1, 0x0A

        cpu.ExecuteCycle();

        Assert.Equal(60, cpu.GetRegister(1));
    }

    [Fact]
    public void Execute_1NNN_Jumps_To_Address()
    {
        var memory = new Memory();
        var display = new Display();
        var input = new Input();

        var cpu = new CPU(memory, display, input);
        cpu.SetPc(0x200);

        memory.Load(new byte[] { 0x12, 0x34 }, 0x200); // JMP 0x234

        cpu.ExecuteCycle();

        Assert.Equal((ushort)0x234, cpu.Pc);
    }
}
