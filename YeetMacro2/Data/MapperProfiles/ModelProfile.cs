using AutoMapper;
using System.Text.Json;
using System.Text.Json.Nodes;
using YeetMacro2.Data.Models;
using YeetMacro2.Services;
using YeetMacro2.ViewModels;
using YeetMacro2.ViewModels.NodeViewModels;

namespace YeetMacro2.Data.MapperProfiles;
public class ModelProfile : Profile
{
    public ModelProfile()
    {
        CreateMap<MacroSet, MacroSetViewModel>()
            .ForCtorParam("nodeViewModelManagerFactory", o => o.MapFrom(ms => ServiceHelper.GetService<NodeManagerViewModelFactory>()))
            .ForCtorParam("scriptService", o => o.MapFrom(ms => ServiceHelper.GetService<IScriptService>()))
            .ReverseMap();

        CreateMap<PatternNode, PatternNodeViewModel>().ReverseMap();
        CreateMap<Pattern, PatternViewModel>().ReverseMap();
        CreateMap<TextMatchProperties, TextMatchPropertiesViewModel>().ReverseMap();
        CreateMap<ColorThresholdProperties, ColorThresholdPropertiesViewModel>().ReverseMap();

        CreateMap<ScriptNode, ScriptNodeViewModel>().ReverseMap();

        var mappedSettingNodeTypes = NodeTypeMappingAttribute.GetMappedType<ParentSettingViewModel>();
        foreach (var mappedType in mappedSettingNodeTypes)
        {
            CreateMap(mappedType.Key, mappedType.Value).ReverseMap();
        }

        CreateMap<TodoNode, TodoViewModel>();
        //https://stackoverflow.com/questions/75877586/automapper-exception-when-mapping-jsonobject-in-net6-the-node-already-has-a
        CreateMap<JsonObject, JsonObject>()
          .ConvertUsing(src => src == null ? null : JsonNode.Parse(src.ToJsonString(JsonSerializerOptions.Default), null, default)
          .AsObject());

        CreateMap<ScriptLog, ScriptLogViewModel>();
        CreateMap<ExceptionLog, ExceptionLogViewModel>();
        CreateMap<ScreenCaptureLog, ScreenCaptureLogViewModel>();
        CreateMap<Log, LogViewModel>();

    }
}