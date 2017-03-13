using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using S22.Imap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.MailNotifier
{
    public class NotifierToolWindowViewModel
    {
        private string _email;
        private DelegateCommand signIn;
        private IVsStatusbar bar;
        private ImapClient client;
        private AutoResetEvent reconnectEvent = new AutoResetEvent(false);

        public NotifierToolWindowViewModel()
        {
            signIn = new DelegateCommand(SignIn);
        }

        private void SignIn(object obj)
        {
            PasswordBox pwBox = obj as PasswordBox;
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        DisplayAndShowIcon("Connecting...", (short)Microsoft.VisualStudio.Shell.Interop.Constants.SBAI_Build);
                        InitializeClient(pwBox);
                        DisplayMessage("Connected");
                        reconnectEvent.WaitOne();
                    }
                }
                finally
                {
                    if (client != null)
                        client.Dispose();
                }
            });

           
        }
        public ICommand SignInCommand
        {
            get { return signIn; }
        }
        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }
        
        private IVsStatusbar StatusBar
        {
            get
            {
                if (bar == null)
                {
                    bar = Package.GetGlobalService(typeof(SVsStatusbar)) as IVsStatusbar;
                }

                return bar;
            }
        }

        private void InitializeClient(PasswordBox pwBox)
        {
            // Dispose of existing instance, if any.
            if (client != null)
                client.Dispose();
            client = new ImapClient("imap.gmail.com", 993, _email, pwBox.Password, AuthMethod.Login, true);
            // Setup event handlers.
            client.NewMessage += client_NewMessage;
            client.IdleError += client_IdleError;
        }
        private void client_IdleError(object sender, IdleErrorEventArgs e)
        {
            DisplayMessage("An error occurred while idling: ");
            DisplayMessage(e.Exception.Message);
            reconnectEvent.Set();
        }

        private void client_NewMessage(object sender, IdleMessageEventArgs e)
        {
            MailMessage msg = client.GetMessage(e.MessageUID);
            DisplayMessage("Got a new message!" + " From: " + msg.From  +" Subject: " + msg.Subject + " Priority: " + msg.Priority);
        }
        #region Public Implementation

        /// <summary>
        /// Displays the message.
        /// </summary>
        public void DisplayMessage(string msg)
        {
            int frozen;

            StatusBar.Clear();
            StatusBar.IsFrozen(out frozen);
            StatusBar.SetText(msg);
            StatusBar.FreezeOutput(0);

        }

        public void DisplayAndShowIcon(string message, object iconToShow)
        {
            object icon = (short)iconToShow;

            StatusBar.Animation(1, ref icon);
            StatusBar.SetText(message);
            Thread.Sleep(3000);

            StatusBar.Animation(0, ref icon);
            StatusBar.FreezeOutput(0);
            StatusBar.Clear();
        }

        public void DisplayAndShowProgress()
        {
            var messages = new string[]
                {
                    "Demo Long running task...Step 1...",
                    "Step 2...",
                    "Step 3...",
                    "Step 4...",
                    "Completing long running task."
                };
            uint cookie = 0;

            // Initialize the progress bar.
            StatusBar.Progress(ref cookie, 1, "", 0, 0);

            for (uint j = 0; j < 5; j++)
            {
                uint count = j + 1;
                StatusBar.Progress(ref cookie, 1, "", count, 5);
                StatusBar.SetText(messages[j]);
                // Display incremental progress.
                Thread.Sleep(1500);
            }

            // Clear the progress bar.
            StatusBar.Progress(ref cookie, 0, "", 0, 0);
            StatusBar.FreezeOutput(0);
            StatusBar.Clear();

        }

        public void SetAndGetStatusBarText(string message)
        {
            int frozen;

            StatusBar.IsFrozen(out frozen);

            if (frozen == 0)
            {
                // Set the status bar text and make its display static.
                StatusBar.SetText(message);
                StatusBar.FreezeOutput(1);

                // Retrieve the status bar text. 
                string text;
                StatusBar.GetText(out text);
                MessageBox.Show(text);
            }
            // Clear the status bar text.
            StatusBar.FreezeOutput(0);
            StatusBar.Clear();
        }
        #endregion


    }
    public class DelegateCommand : ICommand
    {
        private Action<object> _executeMethod;
        public DelegateCommand(Action<object> executeMethod)
        {
            _executeMethod = executeMethod;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged;
        public void Execute(object parameter)
        {
            _executeMethod.Invoke(parameter);
        }
    }
}
