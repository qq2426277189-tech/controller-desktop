using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace ControllerDesktop.Input;

public sealed class XInputGamepad : IDisposable
{
    [StructLayout(LayoutKind.Sequential)]
    private struct XINPUT_STATE
    {
        public uint dwPacketNumber;
        public ushort wButtons;
        public byte bLeftTrigger;
        public byte bRightTrigger;
        public short sThumbLX;
        public short sThumbLY;
        public short sThumbRX;
        public short sThumbRY;
    }

    [DllImport("xinput1_4.dll")]
    private static extern uint XInputGetState(uint dwUserIndex, ref XINPUT_STATE pState);

    private Timer? _timer;
    private readonly object _lock = new();
    private GamepadState _last = new();
    public bool Connected { get; private set; }

    public event Action<GamepadState, GamepadState>? StateChanged;

    public void Start()
    {
        if (_timer != null) return;
        _timer = new Timer(_ => Poll(), null, 0, 16);
    }

    private void Poll()
    {
        lock (_lock)
        {
            var state = new XINPUT_STATE();
            var result = XInputGetState(0, ref state);
            Connected = result == 0;
            if (!Connected) return;

            var current = new GamepadState
            {
                Buttons = state.wButtons,
                LeftX = state.sThumbLX,
                LeftY = state.sThumbLY,
                RightX = state.sThumbRX,
                RightY = state.sThumbRY,
                LeftTrigger = state.bLeftTrigger,
                RightTrigger = state.bRightTrigger
            };

            if (current != _last)
            {
                var previous = _last;
                _last = current;
                StateChanged?.Invoke(previous, current);
            }
        }
    }

    public GamepadState Last => _last;

    public void Dispose()
    {
        _timer?.Dispose();
        _timer = null;
    }
}


