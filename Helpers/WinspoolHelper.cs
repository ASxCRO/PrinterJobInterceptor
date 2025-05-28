using System.Runtime.InteropServices;

namespace PrinterJobInterceptor.Helpers;
public static class WinspoolHelper
{
    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GetJob(IntPtr hPrinter, uint dwJobId, uint Level, IntPtr pJob, uint cbBuf, out uint pcbNeeded);

    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool SetJob(IntPtr hPrinter, uint dwJobId, uint Level, IntPtr pJob, uint Command);

    [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool EnumJobs(IntPtr hPrinter, uint FirstJob, uint NoJobs, uint Level, IntPtr pJob, uint cbBuf, out uint pcbNeeded, out uint pcReturned);

    private const uint JOB_INFO_2_CONST = 2;
    private const uint JOB_CONTROL_PAUSE = 1;
    private const uint JOB_CONTROL_RESUME = 2;
    private const uint JOB_CONTROL_CANCEL = 3;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct JOB_INFO_2
    {
        public uint JobId;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string pPrinterName;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string pMachineName;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string pUserName;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string pDocument;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string pNotifyName;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string pDatatype;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string pPrintProcessor;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string pParameters;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string pDriverName;
        public IntPtr pDevMode;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string pStatus;
        public IntPtr pSecurityDescriptor;
        public uint Status;
        public uint Priority;
        public uint Position;
        public uint StartTime;
        public uint UntilTime;
        public uint TotalPages;
        public uint Size;
        public SYSTEMTIME Submitted;
        public uint Time;
        public uint PagesPrinted;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEMTIME
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMilliseconds;
    }

    public static bool PauseJob(string printerName, uint jobId)
    {
        return ControlJob(printerName, jobId, JOB_CONTROL_PAUSE);
    }

    public static bool ResumeJob(string printerName, uint jobId)
    {
        return ControlJob(printerName, jobId, JOB_CONTROL_RESUME);
    }

    public static bool CancelJob(string printerName, uint jobId)
    {
        return ControlJob(printerName, jobId, JOB_CONTROL_CANCEL);
    }

    private static bool ControlJob(string printerName, uint jobId, uint command)
    {
        if (!OpenPrinter(printerName, out IntPtr hPrinter, IntPtr.Zero))
        {
            throw new Exception($"Failed to open printer: {Marshal.GetLastWin32Error()}");
        }

        try
        {
            return SetJob(hPrinter, jobId, 0, IntPtr.Zero, command);
        }
        finally
        {
            ClosePrinter(hPrinter);
        }
    }

    public static JOB_INFO_2? GetJobInfo(string printerName, uint jobId)
    {
        if (!OpenPrinter(printerName, out IntPtr hPrinter, IntPtr.Zero))
        {
            throw new Exception($"Failed to open printer: {Marshal.GetLastWin32Error()}");
        }

        try
        {
            uint needed = 0;
            GetJob(hPrinter, jobId, JOB_INFO_2_CONST, IntPtr.Zero, 0, out needed);

            if (needed == 0)
            {
                return null;
            }

            IntPtr buffer = Marshal.AllocHGlobal((int)needed);
            try
            {
                if (!GetJob(hPrinter, jobId, JOB_INFO_2_CONST, buffer, needed, out needed))
                {
                    return null;
                }

                return (JOB_INFO_2)Marshal.PtrToStructure(buffer, typeof(JOB_INFO_2));
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        finally
        {
            ClosePrinter(hPrinter);
        }
    }

    public static IEnumerable<uint> GetJobIds(string printerName)
    {
        if (!OpenPrinter(printerName, out IntPtr hPrinter, IntPtr.Zero))
        {
            throw new Exception($"Failed to open printer: {Marshal.GetLastWin32Error()}");
        }

        try
        {
            uint needed = 0;
            uint returned = 0;

            // First call to get the required buffer size
            EnumJobs(hPrinter, 0, uint.MaxValue, JOB_INFO_2_CONST, IntPtr.Zero, 0, out needed, out returned);

            if (needed == 0)
            {
                return Array.Empty<uint>();
            }

            IntPtr buffer = Marshal.AllocHGlobal((int)needed);
            try
            {
                if (!EnumJobs(hPrinter, 0, uint.MaxValue, JOB_INFO_2_CONST, buffer, needed, out needed, out returned))
                {
                    return Array.Empty<uint>();
                }

                var jobs = new List<uint>();
                var offset = 0;

                for (int i = 0; i < returned; i++)
                {
                    var jobInfo = (JOB_INFO_2)Marshal.PtrToStructure(IntPtr.Add(buffer, offset), typeof(JOB_INFO_2));
                    jobs.Add(jobInfo.JobId);
                    offset += Marshal.SizeOf(typeof(JOB_INFO_2));
                }

                return jobs;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        finally
        {
            ClosePrinter(hPrinter);
        }
    }
}