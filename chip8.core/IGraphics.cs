namespace chip8.core
{
    public interface IGraphics
    {
        void ClearScreen();
        bool DrawSprite(byte x, byte y, byte height, byte[] sprite);
    }
}