using AutoMapper;
using YeetMacro2.Data.Models;
using YeetMacro2.ViewModels;
using YeetMacro2.ViewModels.NodeViewModels;

namespace YeetMacro2.Data.MapperProfiles;
public class ModelProfile : Profile
{
    public ModelProfile()
    {
        CreateMap<MacroSet, MacroSetViewModel>().ReverseMap();

        CreateMap<PatternNode, PatternNodeViewModel>().ReverseMap();
        CreateMap<Pattern, PatternViewModel>().ReverseMap();
        CreateMap<TextMatchProperties, TextMatchPropertiesViewModel>().ReverseMap();
        CreateMap<ColorThresholdProperties, ColorThresholdPropertiesViewModel>().ReverseMap();

        CreateMap<ScriptNode, ScriptNodeViewModel>().ReverseMap();

        CreateMap<ParentSetting, ParentSettingViewModel>().ReverseMap();
        CreateMap<BooleanSetting, BooleanSettingViewModel>().ReverseMap();
        CreateMap<OptionSetting, OptionSettingViewModel>().ReverseMap();
        CreateMap<StringSetting, StringSettingViewModel>().ReverseMap();
        CreateMap<IntegerSetting, IntegerSettingViewModel>().ReverseMap();
        CreateMap<EnabledIntegerSetting, EnabledIntegerSettingViewModel>().ReverseMap();
        CreateMap<PatternSetting, PatternSettingViewModel>().ReverseMap();
    }
}