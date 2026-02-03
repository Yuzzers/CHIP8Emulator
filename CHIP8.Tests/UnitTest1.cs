namespace CHIP8.Tests;

public class UnitTest1
{
[Fact]
public void Execute_8XY4_Sets_Carry_Flag_On_Overflow()
{
    var mem = new FakeMemory();
    var cpu = CreateCpu(mem);

    cpu.SetPc(0x200);
    cpu.SetRegister(1, 200);
    cpu.SetRegister(2, 100);

    LoadOpcode(mem, 0x200, 0x8124); // ADD V1, V2

    cpu.ExecuteCycle();

    Assert.Equal((byte)44, cpu.GetRegister(1)); // overflow wrap
    Assert.Equal(1, cpu.GetRegister(0xF));
}

}
