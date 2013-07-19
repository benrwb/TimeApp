using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TimeApp2
{
    class Desktops
    {
        // http://stackoverflow.com/questions/509733/win-api-and-c-sharp-desktops

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetUserObjectInformation(IntPtr hObj, int nIndex, StringBuilder pvInfo, uint nLength, ref uint lpnLengthNeeded);

        private const int UOI_NAME = 2;
        private const uint READ_CONTROL = 0x00020000;

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr OpenInputDesktop(uint dwFlags, bool fInherit, uint dwDesiredAccess);

        private static string GetDesktopName(IntPtr hDesktop)
        {
            StringBuilder name = new StringBuilder();
            uint length = 0;
            GetUserObjectInformation(hDesktop, UOI_NAME, name, 0, ref length);
            GetUserObjectInformation(hDesktop, UOI_NAME, name, length, ref length);
            return name.ToString();
        }

        public static string GetCurrentDesktopName()
        {
            IntPtr hDesktop = OpenInputDesktop(0, false, READ_CONTROL);
            return GetDesktopName(hDesktop);
        }


        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetThreadDesktop(uint dwThreadId);

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        public static string GetProcessDesktopName()
        {
            var hDesktop = GetThreadDesktop(GetCurrentThreadId());
            return GetDesktopName(hDesktop);
        }



















        [StructLayout(LayoutKind.Sequential/*, CharSet = CharSet.Unicode*/)]
        // Had to remove "CharSet = CharSet.Unicode", otherwise
        // CreateProcess fails and the following error message is displayed:
        //    |---------------------------------------------------------------|
        //    |  The application was unable to start correctly (0xc0000142).  |
        //    |  Click OK to close the application.                           |
        //    |---------------------------------------------------------------|
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [DllImport("kernel32.dll")]
        private static extern bool CreateProcess(
           string lpApplicationName,
           string lpCommandLine,
           IntPtr lpProcessAttributes,
           IntPtr lpThreadAttributes,
           bool bInheritHandles,
           int dwCreationFlags,
           IntPtr lpEnvironment,
           string lpCurrentDirectory,
           ref STARTUPINFO lpStartupInfo,
           ref PROCESS_INFORMATION lpProcessInformation);

        const int NORMAL_PRIORITY_CLASS = 0x00000020;

        // http://www.codeproject.com/Articles/7666/Desktop-Switching

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        public static void StartProcessOnDesktop(string cmdLine, string desktopName)
        {
            // set startup parameters.
            STARTUPINFO si = new STARTUPINFO();
            si.cb = Marshal.SizeOf(si);
            si.lpDesktop = desktopName;

            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();

            // start the process.
            bool result = CreateProcess(null, cmdLine, IntPtr.Zero, IntPtr.Zero, true,
                NORMAL_PRIORITY_CLASS, IntPtr.Zero, null, ref si, ref pi);

            CloseHandle(pi.hProcess); // don't
            CloseHandle(pi.hThread);   // leak!
        }
    }
}
