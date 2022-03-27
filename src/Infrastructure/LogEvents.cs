namespace Application.Common
{
    public static class LogEvents
    {
        public const int GetTaskForest = 1001;
        public const int GetTaskSubtree = 1002;
        public const int AddTask = 1003;
        public const int AddSubtask = 1004;
        public const int UpdateTask = 1005;
        public const int DeleteTask = 1006;

        public const int Register = 1101;
        public const int LogIn = 1102;
        public const int LogOut = 1103;
        public const int SendingConfirmationEmail = 1104;
        public const int ConfirmedEmail = 1105;

        public const int AuthenticationException = 2001;
        public const int TaskInvalidStatusException = 2101;
        public const int TaskNotLeafException = 2102;
    }
}