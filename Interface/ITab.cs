using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Interface
{
    public interface ITab
    {
        event EventHandler ProgressReportStart;
        event EventHandler ProgressReportUpdate;
        event EventHandler ProgressReportComplete;
        event Func<object, EventArgs, Task> ReadFromFileRequest;
        event Func<object, EventArgs, Task> WriteToFileRequest;

        CancellationTokenSource CancellationToken
        {
            get;
        }

        string Name
        {
            get;
        }

        bool isBusy
        {
            get;
        }

        bool isChanged
        {
            get;
        }

        object GetUIElements();
        Task DumpAsync();
        Task RestoreAsync();
    }

    public interface IProgressReportUpdateEventArgs
    {

        string Status
        {
            get; set;
        }

        double Value
        {
            get; set;
        }
    }

    public interface IProgressReportCompleteEventArgs
    {
        string SenderName
        {
            get; set;
        }
    }

    public interface IFileActionRequestEventArgs
    {
        string FileName { get; set; }
        byte[] InOutData { get; set; } 
        Exception Exception { get; set; }
    }
}
