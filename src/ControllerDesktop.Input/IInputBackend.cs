namespace ControllerDesktop.Input;

public interface IInputBackend
{
    string Name { get; }
    void Initialize();
    void MoveMouse(int deltaX, int deltaY);
    void MouseLeft(bool down);
    void MouseRight(bool down);
    void Wheel(int delta);
    void KeyTap(string key);
    void Shutdown();
}
