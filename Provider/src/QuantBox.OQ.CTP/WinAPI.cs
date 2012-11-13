using System;
using System.Runtime.InteropServices;

namespace QuantBox.OQ.CTP
{
    class WinAPI
    {
        //imports SetLocalTime function from kernel32.dll 
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int SetLocalTime(ref SystemTime lpSystemTime);

        //struct for date/time apis
        [StructLayout(LayoutKind.Sequential)]
        public struct SystemTime
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
        }

        public static int SetLocalTime(DateTime datetime)
        {
            SystemTime systNew = new SystemTime();

            // 设置属性
            systNew.wDay = (short)datetime.Day;
            systNew.wMonth = (short)datetime.Month;
            systNew.wYear = (short)datetime.Year;
            systNew.wHour = (short)datetime.Hour;
            systNew.wMinute = (short)datetime.Minute;
            systNew.wSecond = (short)datetime.Second;
            systNew.wMilliseconds = (short)datetime.Millisecond;

            // 调用API，更新系统时间
            return SetLocalTime(ref systNew);
        }

        [DllImport("Advapi32.dll")]
        public static extern bool LogonUser(
               string sUserName,
               string sDomain,
               string sUserPassword,
               uint dwLogonType,
               uint dwLogonProvider,
               out System.IntPtr token);

        [DllImport("Kernel32.dll")]
        public static extern void CloseHandle(System.IntPtr token);
    }
}
