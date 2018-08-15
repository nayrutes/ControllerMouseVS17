using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public class Win32
{
    //Mouse
    [Flags]
    public enum MouseEventFlags
    {
        LeftDown = 0x00000002,
        LeftUp = 0x00000004,
        MiddleDown = 0x00000020,
        MiddleUp = 0x00000040,
        Move = 0x00000001,
        Absolute = 0x00008000,
        RightDown = 0x00000008,
        RightUp = 0x00000010,
        Wheel = 0x00000800,
        HWheel = 0x00001000
    }

    [DllImport("User32.Dll")]
    public static extern long SetCursorPos(int x, int y);

    [DllImport("User32.Dll")]
    public static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);

    [DllImport("User32.Dll")]
    public static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);


    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;

    }

    public static POINT GetCursorPosition()
    {
        POINT lpPoint;
        GetCursorPos(out lpPoint);
        //bool success = User32.GetCursorPos(out lpPoint);
        // if (!success)

        return lpPoint;
    }

    public static void CursorSetPosition(POINT p)
    {
        //p.x = Convert.ToInt16(txtMouseX.Text);
        //p.y = Convert.ToInt16(txtMouseY.Text);

        //Win32.ClientToScreen(this.Handle, ref p);
        SetCursorPos(p.x, p.y);
    }

    public static void MouseEvent(MouseEventFlags value)
    {
        POINT position = GetCursorPosition();

        mouse_event ((int)value, position.x, position.y, 0, 0);
    }

   /// <summary>
   /// Simulate scroll wheel event
   /// </summary>
   /// <param name="speed"> 120 or -120 advised</param>
   /// <param name="horizontal">use horizontal scroll instead of vertical</param>
    public static void MouseScroll(int speed, bool horizontal = false)
    {
        POINT position = GetCursorPosition();
        if(!horizontal)
            mouse_event((int)MouseEventFlags.Wheel, position.x, position.y, speed, 0);
        else
            mouse_event((int)MouseEventFlags.HWheel, position.x, position.y, speed, 0);
    }

    //Keyboard
    //https://www.pinvoke.net/default.aspx/user32.keybd_event

    public const byte VK_LSHIFT = 0xA0; // left shift key
    public const byte VK_TAB = 0x09;

    public const int KEYEVENTF_KEYDOWN = 0; //press
    public const int KEYEVENTF_EXTENDEDKEY = 0x01;
    public const int KEYEVENTF_KEYUP = 0x02; //release

    public enum Keys:byte
    {

        VK_SPACE = 0x20,
        VK_MENU = 0x12,
        VK_F4 = 0x73

    }

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

    public static void PressKeyUpOrEnd(Keys k)
    {
        keybd_event((byte)k, 0x45, KEYEVENTF_EXTENDEDKEY, 0);
        keybd_event((byte)k, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
    }

    public static void PressKeyDown(Keys k)
    {
        keybd_event((byte)k, 0x45, KEYEVENTF_KEYDOWN, 0);
    }
    public static void PressKeyUp(Keys k)
    {
        keybd_event((byte)k, 0x45, KEYEVENTF_KEYUP, 0);
    }
}
