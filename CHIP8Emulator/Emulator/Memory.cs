using System.Reflection.Metadata;

public class Memory{
    
byte[] memory = new byte [4096];

public byte Read(int address)
    {
        return memory[address];
    }
    public void Write(int address, byte value)
{
    memory[address] = value;
}


    public void Load(byte[] data, int ProgramstartAddress)
    {
        for (int i = 0; i < data.Length; i++)
            memory[ProgramstartAddress + i] = data[i];
    }
}