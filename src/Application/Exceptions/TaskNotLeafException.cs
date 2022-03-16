using System;

namespace Application.Exceptions
{
    public class TaskNotLeafException : Exception
    {
        public TaskNotLeafException(string message) : base(message)
        {
        }
    }
}