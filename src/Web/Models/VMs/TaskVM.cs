using System;
using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Web.Models.VMs
{
    public class TaskVM
    {
        [Required(ErrorMessage = "Name required")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "List of executors required")]
        public string ListOfExecutors { get; set; }

        [Required(ErrorMessage = "Date of registration required")]
        public DateTime DateOfRegistration { get; set; }

        [Required(ErrorMessage = "Date of complete required")]
        public DateTime DateOfComplete { get; set; }

        public TaskStatus Status { get; set; }

        [Required(ErrorMessage = "Planned complexity required")]
        public int PlannedComplexity { get; set; }

        [Required(ErrorMessage = "Actual execution time required")]
        public int ActualExecutionTime { get; set; }
    }
}