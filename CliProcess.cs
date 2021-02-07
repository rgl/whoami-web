using Microsoft.Win32.SafeHandles;
using Microsoft.Windows.Sdk;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace whoami
{
    public class CliProcess
    {
        public class Result
        {
            public uint ExitCode;
            public string StandardOutput;
            public string StandardError;
        }

        // see https://docs.microsoft.com/en-us/windows/win32/api/processthreadsapi/ns-processthreadsapi-startupinfow
        private const uint STARTF_USESHOWWINDOW = 0x00000001;
        private const uint STARTF_USESTDHANDLES = 0x00000100;

        // see https://docs.microsoft.com/en-us/windows/win32/procthread/process-creation-flags
        private const uint CREATE_NEW_PROCESS_GROUP = 0x00000200;
        private const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;

        // see https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow
        private const ushort SW_HIDE = 0;

        public unsafe static Result Run(SafeAccessTokenHandle userToken, string commandLine)
        {
            // see https://docs.microsoft.com/en-us/windows/win32/ProcThread/creating-a-child-process-with-redirected-input-and-output
            CreateStandardInputProcessPipe(out var processStandardInputRead, out var processStandardInputWrite);
            CreateStandardOutputProcessPipe(out var processStandardOutputRead, out var processStandardOutputWrite);
            CreateStandardOutputProcessPipe(out var processStandardErrorRead, out var processStandardErrorWrite);

            var startupInformation = new STARTUPINFOW
            {
                cb = (uint)sizeof(STARTUPINFOW),
                dwFlags = STARTF_USESHOWWINDOW | STARTF_USESTDHANDLES,
                wShowWindow = SW_HIDE,
                hStdInput = (HANDLE)processStandardInputRead.DangerousGetHandle(),
                hStdOutput = (HANDLE)processStandardOutputWrite.DangerousGetHandle(),
                hStdError = (HANDLE)processStandardErrorWrite.DangerousGetHandle(),
            };

            // TODO explicitly configure which handles can be inherited.
            //      """
            //      To specify a list of the handles that should be inherited by a
            //      specific child process, call the UpdateProcThreadAttribute function
            //      with the PROC_THREAD_ATTRIBUTE_HANDLE_LIST flag.
            //      """
            //      see https://docs.microsoft.com/en-us/windows/win32/procthread/inheritance

            if (!PInvoke.CreateProcessAsUser(
                new CloseHandleSafeHandle(userToken.DangerousGetHandle(), false),
                null,
                commandLine,
                null,
                null,
                true,
                CREATE_NEW_PROCESS_GROUP,
                null,
                null,
                in startupInformation,
                out var processInformation))
            {
                var lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception(lastError, string.Format("Failed to start process with error {0}", lastError));
            }

            var processHandle = new CloseHandleSafeHandle(processInformation.hProcess);
            var threadHandle = new CloseHandleSafeHandle(processInformation.hThread);

            // close unneeded thread handle.
            threadHandle.Dispose();

            // close child inherithed handles.
            processStandardInputRead.Dispose();
            processStandardOutputWrite.Dispose();
            processStandardErrorWrite.Dispose();

            // close the child stdin.
            processStandardInputWrite.Dispose();

            // read the child stdout and stderr in parallel.
            // NB Anonymous pipes do not support asynchronous/overlapped operations and
            //    that is why we are not using ReadToEndAsync, instead, we use ReadToEnd
            //    in a thread pool.
            //    see https://docs.microsoft.com/en-us/windows/win32/ipc/anonymous-pipe-operations
            var standardOutputReader = new StreamReader(new FileStream(processStandardOutputRead, FileAccess.Read), Console.OutputEncoding);
            var standardErrorReader = new StreamReader(new FileStream(processStandardErrorRead, FileAccess.Read), Console.OutputEncoding);
            var standardOutputReaderTask = Task.Run(() => standardOutputReader.ReadToEnd());
            var standardErrorReaderTask = Task.Run(() => standardErrorReader.ReadToEnd());
            Task.WaitAll(standardOutputReaderTask, standardErrorReaderTask);
            var standardOutput = standardOutputReaderTask.Result;
            var standardError = standardErrorReaderTask.Result;

            // wait for the process to exit.
            var processWaitHandle = new ProcessWaitHandle(processHandle);
            processWaitHandle.WaitOne();

            // get the exited process exit code.
            // NB we could also use ThreadPool.RegisterWaitForSingleObject.
            if (!PInvoke.GetExitCodeProcess(processHandle, out uint exitCode))
            {
                var lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception(lastError, string.Format("Failed to get process exit code with error {0}", lastError));
            }

            return new Result
            {
                ExitCode = exitCode,
                StandardOutput = standardOutput,
                StandardError = standardError,
            };
        }

        private unsafe static void CreateStandardInputProcessPipe(out SafeFileHandle readHandle, out SafeFileHandle writeHandle)
        {
            CreateStandardProcessPipe(out readHandle, out writeHandle);
            // prevent the child from inherithing the parent handle.
            PInvoke.SetHandleInformation(
                new CloseHandleSafeHandle(writeHandle.DangerousGetHandle(), false),
                (uint)HANDLE_FLAG_OPTIONS.HANDLE_FLAG_INHERIT,
                0);
        }

        private unsafe static void CreateStandardOutputProcessPipe(out SafeFileHandle readHandle, out SafeFileHandle writeHandle)
        {
            CreateStandardProcessPipe(out readHandle, out writeHandle);
            // prevent the child from inherithing the parent handle.
            PInvoke.SetHandleInformation(
                new CloseHandleSafeHandle(readHandle.DangerousGetHandle(), false),
                (uint)HANDLE_FLAG_OPTIONS.HANDLE_FLAG_INHERIT,
                0);
        }

        private unsafe static void CreateStandardProcessPipe(out SafeFileHandle readHandle, out SafeFileHandle writeHandle)
        {
            var sa = new SECURITY_ATTRIBUTES
            {
                nLength = (uint)sizeof(SECURITY_ATTRIBUTES),
                bInheritHandle = true,
            };
            if (!PInvoke.CreatePipe(
                out IntPtr r,
                out IntPtr w,
                sa,
                0))
            {
                var lastError = Marshal.GetLastWin32Error();
                throw new Win32Exception(lastError, string.Format("Failed to create standard process pipe with error {0}", lastError));
            }
            readHandle = new SafeFileHandle(r, true);
            writeHandle = new SafeFileHandle(w, true);
        }

        private sealed class ProcessWaitHandle : WaitHandle
        {
            internal ProcessWaitHandle(CloseHandleSafeHandle processHandle)
            {
                // see https://docs.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getcurrentprocess
                var currentProcessHandle = new CloseHandleSafeHandle(new IntPtr(-1), false);
                if (!PInvoke.DuplicateHandle(
                    currentProcessHandle,
                    processHandle,
                    currentProcessHandle,
                    out IntPtr waitHandle,
                    0,
                    false,
                    DUPLICATE_HANDLE_OPTIONS.DUPLICATE_SAME_ACCESS))
                {
                    var lastError = Marshal.GetLastWin32Error();
                    throw new Win32Exception(lastError, string.Format("Failed to duplice process handle with error {0}", lastError));
                }
                this.SetSafeWaitHandle(new SafeWaitHandle(waitHandle, true));
            }
        }
    }
}
