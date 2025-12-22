public class Input{ 
    
bool[] keypad = new bool[16];

public void SetKey(int key, bool isPressed)
    {
        keypad[key] = isPressed;
    }

public bool IsKeyPressed(int key)
    {
        return keypad[key];
    }

    public int GetPressedKey()
    {
        for (int i = 0; i < 16; i++)
        {
            if (keypad[i])
                return i;
        }
        return -1; // ingen tast trykket
    }

}