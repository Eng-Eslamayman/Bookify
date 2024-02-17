﻿using AutoMapper;
using Bookify.Web.Core.Models;
using Bookify.Web.Core.ViewModels;

namespace Bookify.Web.Core.Mapping
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
             CreateMap<Category,CategoryViewModel>();
            CreateMap<CategoryFormViewModel,Category>().ReverseMap();
        }
    }
}
