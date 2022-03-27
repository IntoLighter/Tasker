using System.Collections.Generic;

namespace Web.Models.VMs
{
    public class TaskForestNodeVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<TaskForestNodeVM> ChildrenTasks { get; set; } = new List<TaskForestNodeVM>();
    }
}
