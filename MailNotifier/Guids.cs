// Guids.cs
// MUST match guids.h
using System;

namespace Microsoft.MailNotifier
{
    static class GuidList
    {
        public const string guidMailNotifierPkgString = "2f3b8cdf-4e4e-49d7-8ac6-cf98ec4ccd1b";
        public const string guidMailNotifierCmdSetString = "5e4455ba-d7c5-4397-95d5-a8fc5d21d8bd";
        public const string guidToolWindowPersistanceString = "821af082-cbd0-469e-9ec4-991f28bac7e5";

        public static readonly Guid guidMailNotifierCmdSet = new Guid(guidMailNotifierCmdSetString);
    };
}