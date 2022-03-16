using System;
using System.Collections.Generic;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class TaskEntity
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
        public List<TaskEntity> ChildrenTasks { get; set; } = new List<TaskEntity>();
        public TaskEntity Parent { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
    }
}