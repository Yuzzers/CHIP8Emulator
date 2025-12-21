public class Display
{
      public const int Width = 64;
      public const int Height=32;
      public const int Size = Width*Height;
      private bool[] pixels = new bool[Size];



        // CLS opcode thingy
      public void Clear()
    {
        Array.Clear(pixels,0, pixels.Length);
    }  


            //2d into 1D array, modlus for wrapping
        private int GetIndex( int x, int y)
    {
        x %= Width;
        y %= Height;
        return x+y* Width;
    }

        //pixel true / off brug af XOR
        //true hvis pixel var slukket pga collisoion, falsk hvis tændt
    public bool TogglePixel(int x, int y)
    {
     int Index = GetIndex( x,  y);



     bool collision = pixels[Index];
     pixels[Index] ^= true;

     return collision;
    }


            //retunerer staten af en pixel på (x,y) 
    public bool getPixel(int x, int y)
    {
        return pixels[GetIndex(x,y)];
    }
        // GUI shit for senere
    public ReadOnlySpan<bool> Buffer => pixels;



}