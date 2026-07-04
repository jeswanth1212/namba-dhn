using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class ZKeySequenceWindow
{
    #region Windows API
    
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    
    // Virtual key codes
    private const int VK_Z = 0x5A;
    private const int VK_M = 0x4D;
    private const int VK_W = 0x57;
    private const int VK_1 = 0x31;
    
    #endregion
    
    #region Fields
    
    private IntPtr _hookID = IntPtr.Zero;
    private readonly LowLevelKeyboardProc _proc;
    private bool _zPressed = false;
    private DateTime _lastZPress = DateTime.MinValue;
    private readonly System.Threading.Timer _sequenceTimer;
    private const int SEQUENCE_TIMEOUT_MS = 1000;
    
    #endregion
    
    #region Events
    
    public event EventHandler<KeyPressedEventArgs>? HotkeyPressed;
    
    #endregion
    
    #region Constructor
    
    public ZKeySequenceWindow()
    {
        // Setup sequence timer
        _sequenceTimer = new System.Threading.Timer(OnSequenceTimeout, null, Timeout.Infinite, Timeout.Infinite);
        
        // Setup keyboard hook
        _proc = HookCallback;
        SetHook();
    }
    
    #endregion
    
    #region Keyboard Hook
    
    private void SetHook()
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule!)
        {
            _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, _proc,
                GetModuleHandle(curModule.ModuleName!), 0);
        }
    }
    
    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                HandleKeyPress(vkCode);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hook callback error: {ex.Message}");
        }
        
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }
    
    private void HandleKeyPress(int vkCode)
    {
        var now = DateTime.Now;
        
        if (vkCode == VK_Z)
        {
            _zPressed = true;
            _lastZPress = now;
            _sequenceTimer.Change(SEQUENCE_TIMEOUT_MS, Timeout.Infinite);
            Console.WriteLine("Z key pressed - waiting for sequence key");
        }
        else if (_zPressed && (now - _lastZPress).TotalMilliseconds <= SEQUENCE_TIMEOUT_MS)
        {
            // Two-key sequence: Z+Key
            var hotkeyId = GetHotkeyId(vkCode);
            if (hotkeyId > 0)
            {
                Console.WriteLine($"Z+{vkCode:X} sequence detected");
                OnHotkeyDetected(hotkeyId);
                ResetSequence();
                return;
            }
            ResetSequence();
        }
        else if (vkCode != VK_Z)
        {
            ResetSequence();
        }
    }
    
    private int GetHotkeyId(int vkCode)
    {
        int result = vkCode switch
        {
            VK_M => 1, // Z+M - Status
            VK_W => 2, // Z+W - Extract text and get AI response
            VK_1 => 11, // Z+1 - Self Destruct
            _ => 0
        };
        
        if (result > 0)
        {
            Console.WriteLine($"GetHotkeyId: {vkCode:X} => {result}");
        }
        
        return result;
    }
    
    private void OnSequenceTimeout(object? state)
    {
        ResetSequence();
    }
    
    private void ResetSequence()
    {
        _zPressed = false;
        _sequenceTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }
    
    private void OnHotkeyDetected(int hotkeyId)
    {
        HotkeyPressed?.Invoke(this, new KeyPressedEventArgs(hotkeyId));
    }
    
    #endregion
    
    #region Cleanup
    
    public void Dispose()
    {
        try
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
            _sequenceTimer?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Dispose error: {ex.Message}");
        }
    }
    
    #endregion
}

public class KeyPressedEventArgs : EventArgs
{
    public int HotkeyId { get; }
    
    public KeyPressedEventArgs(int hotkeyId)
    {
        HotkeyId = hotkeyId;
    }
}