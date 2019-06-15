namespace chip8.core
{
    public interface IInput
    {
        bool KeyPressed(byte key);
        bool WaitForKey(out byte keyPressed);
    }
}