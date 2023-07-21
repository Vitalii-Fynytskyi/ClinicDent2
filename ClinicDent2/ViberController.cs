using ClinicDent2.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClinicDent2
{
    class ViberController
    {
        private const string windowClassName= "Qt632QWindowOwnDCIcon";
        private const string windowName= "Viber";
        IntPtr windowHandler= IntPtr.Zero;
        public void SendToUser(string phoneNumber, StageViewModel stage)
        {
            ///focus window
            windowHandler = WinApi.FindWindowA(windowClassName, windowName);
            if(windowHandler == IntPtr.Zero)
            {
                throw new Exception("Вікно Viber не знайдено");
            }
            WinApi.ShowWindow(windowHandler, WinApi.SW_RESTORE);
            WinApi.SetForegroundWindow(windowHandler);
            WinApi.GetWindowSize(windowHandler,out int width,out int height);
            ///focus search text box and write number there
            WinApi.LeftMouseClickInWindow(windowHandler, 120, 50 - 30); //-30 is OS frame that is ignored
            Clipboard.SetText(phoneNumber);
            WinApi.PressCtrlA();
            WinApi.PressCtrlV();
            Thread.Sleep(500);

            ///go to dialog with patient
            WinApi.LeftMouseClickInWindow(windowHandler, 120, 155);
            Thread.Sleep(1000);
            WinApi.LeftMouseClickInWindow(windowHandler, (int)(width*0.6), height - 60);
            Thread.Sleep(1000);

            ///start typing message
            Clipboard.SetText(stage.Title);
            Thread.Sleep(500);
            WinApi.PressCtrlV();
            foreach (var image in stage.Images)
            {
                image.CopyImageCommand.Execute(null);
                WinApi.PressCtrlV();
                Thread.Sleep(500);
            }
            WinApi.PressEnter();

        }

    }
}
