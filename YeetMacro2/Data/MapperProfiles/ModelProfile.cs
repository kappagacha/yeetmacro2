using AutoMapper;
using System.Text.Json;
using System.Text.Json.Nodes;
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
        CreateMap<EnabledOptionSetting, EnabledOptionSettingViewModel>().ReverseMap();
        CreateMap<StringSetting, StringSettingViewModel>().ReverseMap();
        CreateMap<EnabledStringSetting, EnabledStringSettingViewModel>().ReverseMap();
        CreateMap<IntegerSetting, IntegerSettingViewModel>().ReverseMap();
        CreateMap<EnabledIntegerSetting, EnabledIntegerSettingViewModel>().ReverseMap();
        CreateMap<PatternSetting, PatternSettingViewModel>().ReverseMap();
        CreateMap<EnabledPatternSetting, EnabledPatternSettingViewModel>().ReverseMap();
        CreateMap<TimestampSetting, TimestampSettingViewModel>().ReverseMap();
        CreateMap<DailyNode, DailyNodeViewModel>();
        //https://stackoverflow.com/questions/75877586/automapper-exception-when-mapping-jsonobject-in-net6-the-node-already-has-a
        CreateMap<JsonObject, JsonObject>()
          .ConvertUsing(src => src == null ? null : JsonNode.Parse(src.ToJsonString(JsonSerializerOptions.Default), null, default)
          .AsObject());

    }
}