using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Memory_Management
{
    public class Memory
    {
        public static IntPtr GetProcessHandle(Process process)
        {
            IntPtr handle = OpenProcess(ProcessAccessFlags.All, false, process.Id);
            if (handle == IntPtr.Zero)
            {
                throw new ApplicationException("Failed to open process.");
            }

            return handle;
        }

        public static IntPtr AllocateMemory(int sizeInBytes)
        {
            IntPtr memoryAddress = Marshal.AllocHGlobal(sizeInBytes);
            if (memoryAddress == IntPtr.Zero)
            {
                throw new OutOfMemoryException("Failed to allocate memory.");
            }
            return memoryAddress;
        }

        public static void FreeMemory(IntPtr memoryAddress)
        {
            Marshal.FreeHGlobal(memoryAddress);
        }

        public static T ReadFromMemory<T>(IntPtr address) where T : struct
        {
            byte[] buffer = new byte[Marshal.SizeOf<T>()];
            Marshal.Copy(address, buffer, 0, buffer.Length);

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T result = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();

            return result;
        }

        public static void WriteToMemory<T>(IntPtr address, T value) where T : struct
        {
            byte[] buffer = new byte[Marshal.SizeOf<T>()];
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
            handle.Free();

            Marshal.Copy(buffer, 0, address, buffer.Length);
        }

        public static T ReadProcessMemory<T>(IntPtr processHandle, IntPtr address) where T : struct
        {
            if (processHandle == IntPtr.Zero)
            {
                throw new ObjectDisposedException("MemoryReaderWriter");
            }

            byte[] buffer = new byte[Marshal.SizeOf<T>()];
            ReadProcessMemory(processHandle, address, buffer, buffer.Length, out _);

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T result = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();
            CloseHandle(processHandle);

            return result;
        }

        public static void WriteProcessMemory<T>(IntPtr processHandle, IntPtr address, T value) where T : struct
        {
            if (processHandle == IntPtr.Zero)
            {
                throw new ObjectDisposedException("MemoryReaderWriter");
            }

            byte[] buffer = new byte[Marshal.SizeOf<T>()];
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
            handle.Free();
            CloseHandle(processHandle);

            WriteProcessMemory(processHandle, address, buffer, buffer.Length, out _);
        }

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);
    }
}
