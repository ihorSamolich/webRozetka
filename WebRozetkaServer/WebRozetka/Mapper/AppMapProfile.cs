﻿using AutoMapper;
using WebRozetka.Data.Entities;
using WebRozetka.Data.Entities.Identity;
using WebRozetka.Helpers;
using WebRozetka.Mapper.Converters;
using WebRozetka.Models.Account;
using WebRozetka.Models.Category;

namespace WebRozetka.Mapper
{
    public class AppMapProfile : Profile
    {
        public AppMapProfile()
        {
            CreateMap<CategoryCreateViewModel, CategoryEntity>()
                .AfterMap((src, dest) =>
                {
                    dest.IsDeleted = false;
                    dest.DateCreated = DateTime.UtcNow;
                    dest.Image = ImageWorker.SaveImageAsync(src.Image).Result;
                });

            CreateMap<CategoryEntity, CategoryItemViewModel>();
        }
    }
}
