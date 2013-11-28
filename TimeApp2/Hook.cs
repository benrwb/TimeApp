using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


// read this: http://stackoverflow.com/questions/872677/what-are-all-the-differences-between-wh-mouse-and-wh-mouse-ll-hooks/872720#872720


namespace TimeApp2
{
    class Hook
    {
        // http://support.microsoft.com/kb/318804
        delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        //Declare the hook handle as an int.
        static int hHook = 0;

        //Declare the mouse hook constant.
        //For other hook types, you can obtain these values from Winuser.h in the Microsoft SDK.
        const int WH_MOUSE = 7;
        const int WH_MOUSE_LL = 14;

        
        //Declare the wrapper managed POINT class.
        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
        }

        ////Declare the wrapper managed MouseHookStruct class.
        //[StructLayout(LayoutKind.Sequential)]
        //public class MouseHookStruct
        //{
        //    public POINT pt;
        //    public int hwnd;
        //    public int wHitTestCode;
        //    public int dwExtraInfo;
        //}
        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public int mouseData; // be careful, this must be ints, not uints (was wrong before I changed it...). regards, cmew.
            public int flags;
            public int time;
            public UIntPtr dwExtraInfo;
        }


        //This is the Import for the SetWindowsHookEx function.
        //Use this function to install a thread-specific hook.
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        //This is the Import for the UnhookWindowsHookEx function.
        //Call this function to uninstall the hook.
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        static extern bool UnhookWindowsHookEx(int idHook);

        //This is the Import for the CallNextHookEx function.
        //Use this function to pass the hook information to the next hook procedure in chain.
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

        //Declare MouseHookProcedure as a HookProc type.
        private static HookProc MouseHookProcedure;

        private static Action<MSLLHOOKSTRUCT> THE_CALLBACK;

        public static void SetWindowsHook(Action<MSLLHOOKSTRUCT> callback)
        {
            if (hHook == 0)
            {
                THE_CALLBACK = callback;

                // Create an instance of HookProc.
                MouseHookProcedure = new HookProc(MouseHookProc);

                hHook = SetWindowsHookEx(WH_MOUSE_LL,
                            MouseHookProcedure,
                            (IntPtr)0,
                            0);//AppDomain.GetCurrentThreadId());
                //If the SetWindowsHookEx function fails.
                if (hHook == 0)
                {
                    throw new Exception("SetWindowsHookEx Failed");
                }

            }
        }
        public static void UnhookWindowsHook()
        {
            if (hHook != 0) 
            {
                bool ret = UnhookWindowsHookEx(hHook);
                //If the UnhookWindowsHookEx function fails.
                if (ret == false)
                {
                    throw new Exception("UnhookWindowsHookEx Failed");
                }
                hHook = 0;

                THE_CALLBACK = null;
            }
        }


        static int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //Marshall the data from the callback.
            var MyMouseHookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

            if (nCode < 0)
            {
                return CallNextHookEx(hHook, nCode, wParam, lParam);
            }
            else
            {
                THE_CALLBACK(MyMouseHookStruct);
                ////Create a string variable that shows the current mouse coordinates.
                //String strCaption = "x = " +
                //        MyMouseHookStruct.pt.x.ToString("d") +
                //            "  y = " +
                //MyMouseHookStruct.pt.y.ToString("d");
                ////You must get the active form because it is a static function.
                //Form tempForm = Form.ActiveForm;

                ////Set the caption of the form.
                //tempForm.Text = strCaption;



                return CallNextHookEx(hHook, nCode, wParam, lParam);
            }
        }
    }
}
