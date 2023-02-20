using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Data.Services;

//https://docs.microsoft.com/en-us/ef/core/get-started/xamarin
public class YeetMacroDbContext : DbContext
{
    public DbSet<MacroSet> MacroSets { get; set; }
    public DbSet<Node> Nodes { get; set; }
    public DbSet<NodeClosure> Closures { get; set; }
    //public DbSet<ParentNode> ParentNodes { get; set; }
    //public DbSet<LeafNode> LeafNodes { get; set; }
    public DbSet<PatternBase> Patterns { get; set; }
    public DbSet<PatternNode> PatternNodes { get; set; }
    public DbSet<Setting> Settings { get; set; }
    public DbSet<ParentSetting> ParentSettings { get; set; }
    public DbSet<BooleanSetting> BooleanSettings { get; set; }
    public DbSet<OptionSetting> OptionSettings { get; set; }

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

        modelBuilder.Entity<MacroSet>().HasKey(ms => ms.MacroSetId);
        modelBuilder.Entity<MacroSet>().HasOne(ms => ms.RootPattern).WithOne()
            .HasPrincipalKey<MacroSet>(ms => ms.RootPatternNodeId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<MacroSet>().HasMany(ms => ms.Scripts).WithOne().HasForeignKey(s => s.MacroSetId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Node>().HasKey(pn => pn.NodeId);
        modelBuilder.Entity<Node>().UseTptMappingStrategy();
        modelBuilder.Entity<NodeClosure>().HasKey(nc => nc.ClosureId);
        modelBuilder.Entity<NodeClosure>().HasOne(nc => nc.Ancestor).WithMany().HasForeignKey(n => n.AncestorId);
        modelBuilder.Entity<NodeClosure>().HasOne(nc => nc.Descendant).WithMany().HasForeignKey(n => n.DescendantId);
        modelBuilder.Entity<NodeClosure>().Navigation(pn => pn.Ancestor).AutoInclude();
        modelBuilder.Entity<NodeClosure>().Navigation(pn => pn.Descendant).AutoInclude();

        //https://stackoverflow.com/questions/48017766/what-automatically-causes-ef-core-to-include-collection-properties
        modelBuilder.Entity<PatternNode>().Navigation(pn => pn.Patterns).AutoInclude();
        modelBuilder.Entity<PatternNode>().Navigation(pn => pn.UserPatterns).AutoInclude();
        //https://www.tektutorialshub.com/entity-framework-core/cascade-delete-in-entity-framework-core/
        modelBuilder.Entity<PatternNode>().HasMany(pn => pn.Children).WithOne().HasForeignKey($"{nameof(PatternNode)}{nameof(Node.ParentId)}").OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PatternNode>().HasMany(pn => pn.Patterns).WithOne().HasForeignKey(p => p.ParentNodeId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PatternNode>().HasMany(pn => pn.UserPatterns).WithOne().HasForeignKey(p => p.ParentNodeId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PatternBase>().HasKey(p => p.PatternId);
        modelBuilder.Entity<PatternBase>().OwnsOne(p => p.Resolution);
        modelBuilder.Entity<PatternBase>().OwnsOne(p => p.Bounds);

        modelBuilder.Entity<ParentSetting>().HasMany(pn => pn.Children).WithOne().HasForeignKey($"{nameof(Setting)}{nameof(Node.ParentId)}").OnDelete(DeleteBehavior.Cascade);
        // https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions?tabs=data-annotations
        // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to?pivots=dotnet-7-0
        modelBuilder.Entity<OptionSetting>().Property(os => os.Options).HasConversion(
            opts => JsonSerializer.Serialize(opts, new JsonSerializerOptions()),
            opts => JsonSerializer.Deserialize<string[]>(opts, new JsonSerializerOptions())
        );
    }
}