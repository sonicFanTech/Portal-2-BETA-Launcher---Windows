using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.ComponentModel;

namespace Portal2BetaLauncher.Services;

public sealed class DebugEventInfo : EventArgs
{
    public string Message { get; }
    public bool IsError { get; }
    public DebugEventInfo(string message, bool isError=false) { Message = message; IsError = isError; }
}

public sealed class WindowsDebugger : IDisposable
{
    public event EventHandler<DebugEventInfo>? Output;
    private CancellationTokenSource? cts;
    private Task? worker;
    private uint attachedPid;

    public bool IsRunning => worker is { IsCompleted: false };

    public void DebugProcess(Process process)
    {
        Stop();
        attachedPid = (uint)process.Id;
        worker = Task.Run(() => DebugLoop(attachedPid));
    }

    public void Attach(int pid)
    {
        Stop();
        if (pid <= 0)
        {
            Emit("Invalid PID.", true);
            return;
        }
        attachedPid = (uint)pid;
        cts = new CancellationTokenSource();
        worker = Task.Run(() => DebugLoop((uint)pid));
    }

    public void Stop()
    {
        cts?.Cancel();
        try { if (attachedPid != 0) DebugActiveProcessStop(attachedPid); } catch { }
        attachedPid = 0;
    }

    private void DebugLoop(uint pid)
    {
        try
        {
            if (!DebugActiveProcess((uint)pid))
            {
                var error = Marshal.GetLastWin32Error();
                Emit($"Could not attach to PID {pid}. Windows error {error}: {new Win32Exception(error).Message}. Try running Portal 2 Beta Launcher as Administrator if the process belongs to another account or is protected.", true);
                attachedPid = 0;
                return;
            }
            DebugSetProcessKillOnExit(false);
            Emit($"Attached to PID {pid}. Debugger is active.");
            AttachLoop(pid);
        }
        catch (Exception ex) { Emit(ex.Message, true); }
    }

    private void AttachLoop(uint pid)
    {
        var de = new DEBUG_EVENT();
        while (cts is null || !cts.IsCancellationRequested)
        {
            if (!WaitForDebugEvent(ref de, 1000)) continue;
            var continueStatus = ContinueStatus.DBG_CONTINUE;
            try
            {
                switch (de.dwDebugEventCode)
                {
                    case DebugEventType.CREATE_PROCESS_DEBUG_EVENT:
                        Emit($"CREATE_PROCESS pid={de.dwProcessId} tid={de.dwThreadId} base=0x{de.u.CreateProcessInfo.lpBaseOfImage.ToInt64():X}");
                        CloseHandle(de.u.CreateProcessInfo.hFile); break;
                    case DebugEventType.LOAD_DLL_DEBUG_EVENT:
                        Emit($"LOAD_DLL base=0x{de.u.LoadDll.lpBaseOfDll.ToInt64():X}"); CloseHandle(de.u.LoadDll.hFile); break;
                    case DebugEventType.UNLOAD_DLL_DEBUG_EVENT:
                        Emit($"UNLOAD_DLL base=0x{de.u.UnloadDll.lpBaseOfDll.ToInt64():X}"); break;
                    case DebugEventType.CREATE_THREAD_DEBUG_EVENT:
                        Emit($"CREATE_THREAD tid={de.dwThreadId}"); break;
                    case DebugEventType.EXIT_THREAD_DEBUG_EVENT:
                        Emit($"EXIT_THREAD tid={de.dwThreadId} code=0x{de.u.ExitThread.dwExitCode:X}"); break;
                    case DebugEventType.OUTPUT_DEBUG_STRING_EVENT:
                        Emit(ReadDebugString(de.u.DebugString)); break;
                    case DebugEventType.EXCEPTION_DEBUG_EVENT:
                        var code = de.u.Exception.ExceptionRecord.ExceptionCode;
                        var firstChance = de.u.Exception.dwFirstChance != 0;
                        Emit($"EXCEPTION 0x{code:X8} at 0x{de.u.Exception.ExceptionRecord.ExceptionAddress.ToInt64():X} ({(firstChance ? "first chance" : "second chance")})", !firstChance);
                        if (!firstChance) continueStatus = ContinueStatus.DBG_EXCEPTION_NOT_HANDLED;
                        break;
                    case DebugEventType.EXIT_PROCESS_DEBUG_EVENT:
                        Emit($"EXIT_PROCESS code=0x{de.u.ExitProcess.dwExitCode:X}"); break;
                }
            }
            catch (Exception ex) { Emit($"Debugger event error: {ex.Message}", true); }
            ContinueDebugEvent(de.dwProcessId, de.dwThreadId, continueStatus);
            if (de.dwDebugEventCode == DebugEventType.EXIT_PROCESS_DEBUG_EVENT) break;
        }
    }

    private string ReadDebugString(DEBUG_EVENT.DebugStringInfo info)
    {
        try
        {
            using var process = Process.GetProcessById((int)attachedPid);
            var data = new byte[info.nDebugStringLength * (info.fUnicode != 0 ? 2u : 1u)];
            if (!ReadProcessMemory(process.Handle, info.lpDebugStringData, data, data.Length, out _)) return "OUTPUT_DEBUG_STRING <unreadable>";
            return "OUTPUT_DEBUG_STRING: " + (info.fUnicode != 0 ? Encoding.Unicode.GetString(data).TrimEnd('\0') : Encoding.Default.GetString(data).TrimEnd('\0'));
        }
        catch { return "OUTPUT_DEBUG_STRING <unreadable>"; }
    }

    private void Emit(string message, bool error=false) => Output?.Invoke(this, new DebugEventInfo(message, error));
    public void Dispose() => Stop();

    private enum DebugEventType:uint { EXCEPTION_DEBUG_EVENT=1, CREATE_THREAD_DEBUG_EVENT=2, CREATE_PROCESS_DEBUG_EVENT=3, EXIT_THREAD_DEBUG_EVENT=4, EXIT_PROCESS_DEBUG_EVENT=5, LOAD_DLL_DEBUG_EVENT=6, UNLOAD_DLL_DEBUG_EVENT=7, OUTPUT_DEBUG_STRING_EVENT=8 }
    private enum ContinueStatus:uint { DBG_CONTINUE=0x00010002, DBG_EXCEPTION_NOT_HANDLED=0x80010001 }
    [StructLayout(LayoutKind.Sequential)] private struct DEBUG_EVENT { public DebugEventType dwDebugEventCode; public uint dwProcessId, dwThreadId; public DEBUG_UNION u;
        [StructLayout(LayoutKind.Explicit, Size = 160)] public struct DEBUG_UNION { [FieldOffset(0)] public ExceptionInfo Exception; [FieldOffset(0)] public CreateThreadInfo CreateThread; [FieldOffset(0)] public CreateProcessInfo CreateProcessInfo; [FieldOffset(0)] public ExitThreadInfo ExitThread; [FieldOffset(0)] public ExitProcessInfo ExitProcess; [FieldOffset(0)] public LoadDllInfo LoadDll; [FieldOffset(0)] public UnloadDllInfo UnloadDll; [FieldOffset(0)] public DebugStringInfo DebugString; }
        [StructLayout(LayoutKind.Sequential)] public struct ExceptionInfo { public EXCEPTION_RECORD ExceptionRecord; public uint dwFirstChance; }
        [StructLayout(LayoutKind.Sequential)] public unsafe struct EXCEPTION_RECORD { public uint ExceptionCode, ExceptionFlags; public IntPtr ExceptionRecord, ExceptionAddress; public uint NumberParameters; public fixed ulong ExceptionInformation[15]; }
        [StructLayout(LayoutKind.Sequential)] public struct CreateThreadInfo { public IntPtr hThread, lpThreadLocalBase, lpStartAddress; }
        [StructLayout(LayoutKind.Sequential)] public struct CreateProcessInfo { public IntPtr hFile,hProcess,hThread,lpBaseOfImage,lpThreadLocalBase,lpStartAddress,lpImageName; public ushort fUnicode; }
        [StructLayout(LayoutKind.Sequential)] public struct ExitThreadInfo { public uint dwExitCode; }
        [StructLayout(LayoutKind.Sequential)] public struct ExitProcessInfo { public uint dwExitCode; }
        [StructLayout(LayoutKind.Sequential)] public struct LoadDllInfo { public IntPtr hFile,lpBaseOfDll,lpImageName; public ushort fUnicode; }
        [StructLayout(LayoutKind.Sequential)] public struct UnloadDllInfo { public IntPtr lpBaseOfDll; }
        [StructLayout(LayoutKind.Sequential)] public struct DebugStringInfo { public IntPtr lpDebugStringData; public ushort fUnicode,nDebugStringLength; }
    }

    [DllImport("kernel32.dll", SetLastError=true)] private static extern bool DebugActiveProcess(uint dwProcessId);
    [DllImport("kernel32.dll", SetLastError=true)] private static extern bool DebugActiveProcessStop(uint dwProcessId);
    [DllImport("kernel32.dll", SetLastError=true)] private static extern bool WaitForDebugEvent(ref DEBUG_EVENT lpDebugEvent, uint dwMilliseconds);
    [DllImport("kernel32.dll", SetLastError=true)] private static extern bool ContinueDebugEvent(uint dwProcessId,uint dwThreadId,ContinueStatus dwContinueStatus);
    [DllImport("kernel32.dll", SetLastError=true)] private static extern bool CloseHandle(IntPtr hObject);
    [DllImport("kernel32.dll", SetLastError=true)] private static extern bool DebugSetProcessKillOnExit(bool KillOnExit);
    [DllImport("kernel32.dll", SetLastError=true)] private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesRead);
}
