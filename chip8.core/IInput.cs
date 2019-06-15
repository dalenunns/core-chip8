public interface IInput
{
    bool KeyPressed (byte key);
    bool WaitForKey(out byte keyPressed);
}