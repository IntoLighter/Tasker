using System;
using System.Collections.Generic;
using Domain.Enums;

namespace Web.Models.DTOs
{
    public class TaskDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ListOfExecutors { get; set; }
        public DateTime DateOfRegistration { get; set; }
        public DateTime DateOfComplete { get; set; }
        public TaskStatus Status { get; set; }
        public int PlannedComplexity { get; set; }
        public int ActualExecutionTime { get; set; }
        public List<TaskDTO> ChildrenTasks { get; set; }
    }
}