using AutoMapper;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Data.MapperProfiles;
public class ModelProfile : Profile
{
    public ModelProfile()
    {
        CreateMap<Pattern, Pattern>();
        CreateMap<PatternNode, PatternNode>();
        CreateMap<Resolution, Resolution>();
        CreateMap<Bounds, Bounds>();
    }
}