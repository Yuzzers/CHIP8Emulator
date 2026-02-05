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
    [Fact]
    public void Execute_FX33_Stores_BCD_Correctly()
    {
        var memory = new Memory();
        var display = new Display();
        var input = new Input();

        var cpu = new CPU(memory, display, input);
        cpu.SetPc(0x200);
        cpu.SetRegister(3, 254);
        cpu.SetIndex(0x300);
        memory.Load(new byte[] { 0xF3, 0x33 }, 0x200);
        cpu.ExecuteCycle();
        Assert.Equal((byte)2, memory.Read(0x300));     // hundreds
        Assert.Equal((byte)5, memory.Read(0x301));     // tens
        Assert.Equal((byte)4, memory.Read(0x302));     // ones
        Assert.Equal((ushort)0x202, cpu.Pc);
        Assert.Equal((ushort)0x300, cpu.Index);
    }
    [Fact]
    public void Execute_FX33_Stores_BCD_Correctly_For_Small_Value()
    {
        var memory = new Memory();
        var display = new Display();
        var input = new Input();

        var cpu = new CPU(memory, display, input);
        cpu.SetPc(0x200);
        cpu.SetRegister(5, 42);
        cpu.SetIndex(0x300);
        memory.Load(new byte[] { 0xF5, 0x33 }, 0x200);
        cpu.ExecuteCycle();
        Assert.Equal((byte)0, memory.Read(0x300));     // hundreds
        Assert.Equal((byte)4, memory.Read(0x301));     // tens
        Assert.Equal((byte)2, memory.Read(0x302));     // ones

        Assert.Equal((ushort)0x202, cpu.Pc);

        Assert.Equal((ushort)0x300, cpu.Index);
    }
    [Fact]
    public void Fx33_UnitTest_Actually_Runs()
    {
        var memory = new Memory();
        var display = new Display();
        var input = new Input();
        var cpu = new CPU(memory, display, input);
        cpu.SetPc(0x200);
        cpu.SetRegister(3, 123);
        cpu.SetIndex(0x300);
        memory.Load(new byte[] { 0xF3, 0x33 }, 0x200);
        cpu.ExecuteCycle();
        byte hundreds = memory.Read(0x300);
        byte tens = memory.Read(0x301);
        byte ones = memory.Read(0x302);
        Assert.Equal(1, hundreds);  // 123 / 100 = 1
        Assert.Equal(2, tens);      // (123 / 10) % 10 = 2
        Assert.Equal(3, ones);      // 123 % 10 = 3
    }
    [Fact]
    public void Execute_8XY4_NoOverflow_BelowBoundary()
    {
        var memory = new Memory();
        var display = new Display();
        var input = new Input();
        var cpu = new CPU(memory, display, input);

        cpu.SetPc(0x200);
        cpu.SetRegister(1, 200);
        cpu.SetRegister(2, 54); // 200 + 54 = 254

        memory.Load(new byte[] { 0x81, 0x24 }, 0x200); // ADD V1, V2
        cpu.ExecuteCycle();

        Assert.Equal(254, cpu.GetRegister(1));
        Assert.Equal(0, cpu.GetRegister(0xF));
    }
    [Fact]
    public void Execute_8XY4_NoOverflow_AtBoundary()
    {
        var memory = new Memory();
        var display = new Display();
        var input = new Input();
        var cpu = new CPU(memory, display, input);

        cpu.SetPc(0x200);
        cpu.SetRegister(1, 200);
        cpu.SetRegister(2, 55); // 200 + 55 = 255

        memory.Load(new byte[] { 0x81, 0x24 }, 0x200);
        cpu.ExecuteCycle();

        Assert.Equal(255, cpu.GetRegister(1));
        Assert.Equal(0, cpu.GetRegister(0xF));
    }
    [Fact]
    public void Execute_8XY4_Overflow_AboveBoundary()
    {
        var memory = new Memory();
        var display = new Display();
        var input = new Input();
        var cpu = new CPU(memory, display, input);

        cpu.SetPc(0x200);
        cpu.SetRegister(1, 200);
        cpu.SetRegister(2, 56); // 200 + 56 = 256

        memory.Load(new byte[] { 0x81, 0x24 }, 0x200);
        cpu.ExecuteCycle();

        Assert.Equal(0, cpu.GetRegister(1)); // wraparound
        Assert.Equal(1, cpu.GetRegister(0xF)); // carry
    }

}

