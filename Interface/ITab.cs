using System;
using System.Threading;
using System.Threading.Tasks;

namespace Interface
{
    public interface ITab
    {
        //
        // Summary:
        //     Occurs when a tab begins to report progress of long-running operation.
        event EventHandler ProgressReportStart;
        //
        // Summary:
        //     Occurs when a tab updates progress of long-running operation.
        event EventHandler ProgressReportUpdate;
        //
        // Summary:
        //     Occurs when a tab stops to report progress of long-running operation.
        event EventHandler ProgressReportComplete;
        //
        // Summary:
        //     Occurs when a tab call data reader to obtain data from a file.
        event Func<object, EventArgs, Task> ReadFromFileRequest;
        //
        // Summary:
        //     Occurs when a tab call data writer to write data to a file.
        event Func<object, EventArgs, Task> WriteToFileRequest;
        //
        // Summary:
        //     Property represents CancellationToken used by long-running actions to provide cancellation
        //     option to framework.
        CancellationTokenSource CancellationToken
        {
            get;
        }
        //
        // Summary:
        //     Property represents plugin name.
        string Name
        {
            get;
        }
        //
        // Summary:
        //     Property represents state of the plugin. It will be true, if a long-running action is ongoing and
        //     tab close action must be delayed or forcefully executed.
        bool isBusy
        {
            get;
        }
        //
        // Summary:
        //     Property represents state of the plugin data. It will be true, if plugin data was changed and  
        //     required save method to be executed in case of tab close event.
        bool isChanged
        {
            get;
        }

        //
        // Summary:
        //     Asynchronously executes tab save actions if isChanged property returns true.
        //
        // Returns:
        //     A task that represents the asynchronous tab save operation.
        Task DumpAsync();
        //
        // Summary:
        //     Asynchronously executes tab load actions.
        //
        // Returns:
        //     A task that represents the asynchronous tab load operation.
        Task RestoreAsync();
    }

    public interface IProgressReportUpdateEventArgs
    {
        //
        // Summary:
        //     Property represents progress report status. Supported values are OK, Error and empty which  
        //     represents transparrent progress bar.
        string Status
        {
            get; set;
        }
        //
        // Summary:
        //     Property represents current progress value.
        double Value
        {
            get; set;
        }
    }

    public interface IFileActionRequestEventArgs
    {
        //
        // Summary:
        //     Property represents file name to access.
        string FileName { get; set; }
        //
        // Summary:
        //     Property represents byte array which is used to pass data to and from action handler. 
        byte[] InOutData { get; set; }
        //
        // Summary:
        //     Property represents action request excetion. Null if no exception occured.
        Exception Exception { get; set; }
    }
}
