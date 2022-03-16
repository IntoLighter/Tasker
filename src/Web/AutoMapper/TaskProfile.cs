using AutoMapper;
using Domain.Entities;
using Web.Models.DTOs;
using Web.Models.VMs;

namespace Web.AutoMapper
{
    public class TaskProfile : Profile
    {
        public TaskProfile()
        {
            CreateMap<TaskEntity, TaskDTO>();
            CreateMap<TaskEntity, TaskForestNodeVM>();
        }
    }
}