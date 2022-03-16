using System;

namespace Application.Exceptions
{
    public class TaskInvalidStatusException : Exception
    {
        public TaskInvalidStatusException(string message) : base(message)
        {
        }
    }
}