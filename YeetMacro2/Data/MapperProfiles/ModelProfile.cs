using AutoMapper;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Data.MapperProfiles;
public class ModelProfile : Profile
{
    public ModelProfile()
    {
        CreateMap<MacroSet, MacroSet>();
        CreateMap<Pattern, Pattern>();
        CreateMap<PatternNode, PatternNode>();
        CreateMap<Resolution, Resolution>();
        CreateMap<Bounds, Bounds>();
    }
}