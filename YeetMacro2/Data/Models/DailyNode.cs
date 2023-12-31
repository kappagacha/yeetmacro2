﻿using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace YeetMacro2.Data.Models;

public class DailyNode : Node, IParentNode<DailyNode, DailyNode>
{
    static JsonSerializerOptions _opts = new JsonSerializerOptions() { WriteIndented = true };
    public override bool IsParentNode => false;     // prevents tree heirarchy in the UI
    [JsonIgnore]
    public virtual ICollection<DailyNode> Nodes { get; set; } = new List<DailyNode>();
    public virtual DateOnly Date { get; set; }
    public virtual JsonObject Data { get; set; }
    [JsonIgnore]
    public string DataText
    {
        get => Data?.ToJsonString(_opts);
    }
}