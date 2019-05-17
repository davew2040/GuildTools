using AutoMapper;
using GuildTools.Controllers.Models;
using ControllerModels = GuildTools.Controllers.Models;
using EfModels = GuildTools.EF.Models;
using GuildTools.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Mapping
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<EF.Models.FriendGuild, Controllers.Models.FriendGuild>();
            CreateMap<EF.Models.StoredBlizzardModels.StoredGuild, Controllers.Models.StoredGuild>();
            CreateMap<EF.Models.StoredBlizzardModels.StoredPlayer, Controllers.Models.BlizzardPlayer>();
            CreateMap<EF.Models.StoredBlizzardModels.StoredRealm, Controllers.Models.StoredRealm>();
            CreateMap<EF.Models.StoredBlizzardModels.StoredPlayer, Controllers.Models.StoredPlayer>();

            CreateMap<UserWithData, UserStub>().ConvertUsing<UserStubConverter>();
            CreateMap<EfModels.GuildProfile, GuildProfileSlim>().ConvertUsing<SlimProfileConverter>();
        }

        public class UserStubConverter : ITypeConverter<UserWithData, UserStub>
        {
            public UserStub Convert(UserWithData source, UserStub destination, ResolutionContext context)
            {
                if (destination == null)
                {
                    destination = new UserStub();
                }

                destination.Email = source.Email;
                destination.Id = source.Id;
                destination.Username = source.UserName;

                return destination;
            }
        }

        public class SlimProfileConverter : ITypeConverter<EfModels.GuildProfile, GuildProfileSlim>
        {
            public GuildProfileSlim Convert(GuildProfile source, GuildProfileSlim destination, ResolutionContext context)
            {
                if (destination == null)
                {
                    destination = new GuildProfileSlim();
                }

                destination.Id = source.Id;
                destination.RegionName = source.Realm.Region.RegionName;
                destination.ProfileName = source.ProfileName;
                destination.PrimaryGuildName = source.CreatorGuild.Name;
                destination.RealmName = source.CreatorGuild.Realm.Name;
                destination.RegionName = source.CreatorGuild.Realm.Region.RegionName;
                destination.Creator = context.Mapper.Map<UserStub>(source.Creator);
                destination.IsPublic = source.IsPublic;

                return destination;
            }
        }
    }
}