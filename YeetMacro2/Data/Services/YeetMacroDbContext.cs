using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using YeetMacro2.Data.Models;
using YeetMacro2.Data.Serialization;

namespace YeetMacro2.Data.Services;

//https://docs.microsoft.com/en-us/ef/core/get-started/xamarin
public class YeetMacroDbContext : DbContext
{
    public DbSet<MacroSet> MacroSets { get; set; }
    public DbSet<Node> Nodes { get; set; }
    public DbSet<NodeClosure> Closures { get; set; }
    public DbSet<Pattern> Patterns { get; set; }
    public DbSet<PatternNode> PatternNodes { get; set; }
    public DbSet<SettingNode> Settings { get; set; }
    public DbSet<ParentSetting> ParentSettings { get; set; }
    public DbSet<BooleanSetting> BooleanSettings { get; set; }
    public DbSet<OptionSetting> OptionSettings { get; set; }
    public DbSet<StringSetting> StringSettings { get; set; }
    public DbSet<PatternSetting> PatternSettings { get; set; }

    public YeetMacroDbContext()
    {
        this.Database.EnsureCreated();
    }

    public YeetMacroDbContext(DbContextOptions<YeetMacroDbContext> options) : base(options)
    {
        this.Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var serializationOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = CombinedPropertiesResolver.Combine(RectPropertiesResolver.Instance, SizePropertiesResolver.Instance)
        };

        // https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions?tabs=data-annotations#the-valueconverter-class
        var rectConverter = new ValueConverter<Rect, string>(
            r => JsonSerializer.Serialize(r, serializationOptions),
            r => JsonSerializer.Deserialize<Rect>(r, serializationOptions));
        var sizeConverter = new ValueConverter<Size, string>(
            r => JsonSerializer.Serialize(r, serializationOptions),
            r => JsonSerializer.Deserialize<Size>(r, serializationOptions));

        modelBuilder.Entity<MacroSet>().HasKey(ms => ms.MacroSetId);
        modelBuilder.Entity<MacroSet>().HasOne(ms => ms.RootPattern).WithOne()
            .HasPrincipalKey<MacroSet>(ms => ms.RootPatternNodeId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<MacroSet>().HasOne(ms => ms.RootScript).WithOne()
            .HasPrincipalKey<MacroSet>(ms => ms.RootScriptNodeId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<MacroSet>().HasOne(ms => ms.RootSetting).WithOne()
            .HasPrincipalKey<MacroSet>(ms => ms.RootSettingNodeId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<MacroSet>().Property(ms => ms.Resolution).HasConversion(sizeConverter);
        modelBuilder.Entity<MacroSet>().OwnsOne(ms => ms.Source);

        modelBuilder.Entity<Node>().HasKey(pn => pn.NodeId);
        modelBuilder.Entity<Node>().UseTptMappingStrategy();
        modelBuilder.Entity<NodeClosure>().HasKey(nc => nc.ClosureId);
        modelBuilder.Entity<NodeClosure>().HasOne(nc => nc.Ancestor).WithMany().HasForeignKey(n => n.AncestorId);
        modelBuilder.Entity<NodeClosure>().HasOne(nc => nc.Descendant).WithMany().HasForeignKey(n => n.DescendantId);
        modelBuilder.Entity<NodeClosure>().Navigation(pn => pn.Ancestor).AutoInclude();
        modelBuilder.Entity<NodeClosure>().Navigation(pn => pn.Descendant).AutoInclude();

        //https://stackoverflow.com/questions/48017766/what-automatically-causes-ef-core-to-include-collection-properties
        modelBuilder.Entity<PatternNode>().Navigation(pn => pn.Patterns).AutoInclude();
        //https://www.tektutorialshub.com/entity-framework-core/cascade-delete-in-entity-framework-core/
        modelBuilder.Entity<PatternNode>().HasMany(pn => pn.Nodes).WithOne().HasForeignKey($"{nameof(PatternNode)}{nameof(Node.ParentId)}").OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PatternNode>().HasMany(pn => pn.Patterns).WithOne().HasForeignKey(p => p.PatternNodeId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Pattern>().HasKey(p => p.PatternId);
        modelBuilder.Entity<Pattern>().Property(p => p.Resolution).HasConversion(sizeConverter);
        modelBuilder.Entity<Pattern>().Property(p => p.Rect).HasConversion(rectConverter);
        modelBuilder.Entity<Pattern>().OwnsOne(p => p.TextMatch);
        modelBuilder.Entity<Pattern>().OwnsOne(p => p.ColorThreshold);

        modelBuilder.Entity<ScriptNode>().HasMany(pn => pn.Nodes).WithOne().HasForeignKey($"{nameof(ScriptNode)}{nameof(Node.ParentId)}").OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ParentSetting>().HasMany(pn => pn.Nodes).WithOne().HasForeignKey($"{nameof(SettingNode)}{nameof(Node.ParentId)}").OnDelete(DeleteBehavior.Cascade);
        // https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions?tabs=data-annotations
        // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to?pivots=dotnet-7-0
        modelBuilder.Entity<OptionSetting>().Property(os => os.Options).HasConversion(
            opts => JsonSerializer.Serialize(opts, serializationOptions),
            opts => JsonSerializer.Deserialize<List<string>>(opts, serializationOptions)
        );

        modelBuilder.Entity<PatternSetting>().HasOne(ps => ps.Value).WithOne()
            .HasPrincipalKey<PatternSetting>(ps => ps.NodeId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PatternSetting>().Navigation(ps => ps.Value).AutoInclude();
    }
}