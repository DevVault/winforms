﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Forms.UITests.Input;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;
using Xunit.Abstractions;
using static Interop;

namespace System.Windows.Forms.UITests;

public class SendInput
{
    private readonly Func<Task> _waitForIdleAsync;
    private readonly ITestOutputHelper _testOutputHelper;

    public SendInput(Func<Task> waitForIdleAsync, ITestOutputHelper testOutputHelper)
    {
        _waitForIdleAsync = waitForIdleAsync;
        _testOutputHelper = testOutputHelper;
    }

    internal async Task SendAsync(CustomForm window, params object[] keys)
    {
        await SendAsync(window, inputSimulator =>
        {
            foreach (var key in keys)
            {
                switch (key)
                {
                    case string str:
                        var text = str.Replace("\r\n", "\r").Replace("\n", "\r");
                        int index = 0;
                        while (index < text.Length)
                        {
                            if (text[index] == '\r')
                            {
                                inputSimulator.Keyboard.KeyPress(VIRTUAL_KEY.VK_RETURN);
                                index++;
                            }
                            else
                            {
                                int nextIndex = text.IndexOf('\r', index);
                                if (nextIndex == -1)
                                {
                                    nextIndex = text.Length;
                                }

                                inputSimulator.Keyboard.TextEntry(text.Substring(index, nextIndex - index));
                                index = nextIndex;
                            }
                        }

                        break;

                    case char c:
                        inputSimulator.Keyboard.TextEntry(c);
                        break;

                    case VIRTUAL_KEY virtualKeyCode:
                        inputSimulator.Keyboard.KeyPress(virtualKeyCode);
                        break;

                    case null:
                        throw new ArgumentNullException(nameof(keys));

                    default:
                        throw new ArgumentException($"Unexpected type encountered: {key.GetType()}", nameof(keys));
                }
            }
        });
    }

    internal async Task SendAsync(CustomForm window, Action<InputSimulator> actions)
    {
        if (actions is null)
        {
            throw new ArgumentNullException(nameof(actions));
        }

        SetForegroundWindow(window);
        //InputSimulator inputSimulator = new InputSimulator();
        actions(new InputSimulator());
        Thread.Sleep(100);
        /*    if (window.HandleInternal != IntPtr.Zero)
            {
                // Reset ManualResetEventSlim on CustomForm so we can block thread until it is set again.
                // window.ResetManualResetEventSlim();
                window.TestKeyProcessed = false;
                // Sending TestKey as end of the input message. SendInput is async and might have not been dispatched to the
                // control before we proceed with further verifications. TestKey is introduced to helps synchronize the
                // input being sent and confirms that the underlying control has received input before proceeding.
                window.Focus();
                new InputSimulator().Keyboard.KeyPress(CustomForm.TestKey);
                _testOutputHelper.WriteLine($"TestKey sent to window - {window.HandleInternal}");
                Thread.Sleep(100);
                await Task.Run(() =>
                {
                    int timeOut = 15000;
                    while (!window.TestKeyProcessed && timeOut > 0 && !window.ParentClosed)
                    {
                        Thread.Sleep(100);
                        timeOut -= 100;
                    }

                    if (timeOut <= 0)
                    {
                        throw new TimeoutException($"Timeout reached while waiting to process SendInput.");
                    }
                });

                await Task.Run(() =>
                {
                    // Wait for the completion of input processing in CustomForm or until CustomForm is closed/destroyed by the
                    // preceding input sent before the TestKey input, and block the thread accordingly.
                    if (!window.WaitOnManualResetEventSlim(5000))
                    {
                        if (window.WaitOnManualResetEventSlim(15000))
                        {
                            _testOutputHelper.WriteLine("Increased timeout helped.");
                            throw new TimeoutException($"Increased timeout required.");
                        }

                        _testOutputHelper.WriteLine($"Increased timeout didn't help.");
                        throw new TimeoutException($"Timeout reached while waiting to process SendInput. {window.HandleInternal}");
                    }
                });

            }*/

        await _waitForIdleAsync();
    }

    private static HWND GetForegroundWindow()
    {
        var startTime = DateTime.Now;

        // Attempt to get the foreground window in a loop, as the NativeMethods function can return IntPtr.Zero
        // in certain circumstances, such as when a window is losing activation.
        HWND foregroundWindow;
        do
        {
            foregroundWindow = PInvoke.GetForegroundWindow();
        }
        while (foregroundWindow == IntPtr.Zero
            && DateTime.Now - startTime < TimeSpan.FromMilliseconds(500));

        return foregroundWindow;
    }

    private static void SetForegroundWindow(CustomForm window)
    {
        // Make the window a top-most window so it will appear above any existing top-most windows
        PInvoke.SetWindowPos(window, HWND.HWND_TOPMOST, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOMOVE);

        // Move the window into the foreground as it may not have been achieved by the 'SetWindowPos' call
        if (!PInvoke.SetForegroundWindow(window))
        {
            string windowTitle = User32.GetWindowText(window);
            if (PInvoke.GetWindowThreadProcessId(window, out uint processId) == 0 || processId != Environment.ProcessId)
            {
                string message = $"ForegroundWindow doesn't belong the test process! The current window HWND: {window}, title:{windowTitle}.";
                throw new InvalidOperationException(message);
            }
        }

        // Ensure the window is 'Active' as it may not have been achieved by 'SetForegroundWindow'
        PInvoke.SetActiveWindow(window);

        // Give the window the keyboard focus as it may not have been achieved by 'SetActiveWindow'
        PInvoke.SetFocus(window);

        // Remove the 'Top-Most' qualification from the window
        PInvoke.SetWindowPos(window, HWND.HWND_NOTOPMOST, 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOMOVE);
    }
}
