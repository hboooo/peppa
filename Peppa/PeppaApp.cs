using System;
using System.Threading;
using System.Windows.Threading;

namespace Peppa
{
    public class PeppaApp
    {

        private static DispatcherOperationCallback exitFrameCallback = new DispatcherOperationCallback(ExitFrame);
        /// <summary>
        /// 页面刷新
        /// </summary>
        public static void DoEvents()
        {
            DispatcherFrame nestedFrame = new DispatcherFrame();
            DispatcherOperation exitOperation = Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, exitFrameCallback, nestedFrame);
            Dispatcher.PushFrame(nestedFrame);
            if (exitOperation.Status != DispatcherOperationStatus.Completed)
            {
                exitOperation.Abort();
            }
        }

        private static Object ExitFrame(Object state)
        {
            DispatcherFrame frame = state as DispatcherFrame;
            frame.Continue = false;
            return null;
        }

        /// <summary>
        /// 保持页面刷新
        /// </summary>
        /// <param name="isFinished"></param>
        /// <param name="timeout"></param>
        public static void UiThreadAlive(ref bool isFinished, int timeout = 1000)
        {
            int sleepspan = 10;
            int i = 0;
            while (isFinished == false && (i + 1) * sleepspan < timeout)
            {
                Thread.Sleep(sleepspan);
                DoEvents();
                i++;
            }
        }
        
    }
}
