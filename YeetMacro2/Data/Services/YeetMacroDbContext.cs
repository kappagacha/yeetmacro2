using Microsoft.EntityFrameworkCore;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Data.Services;
//https://docs.microsoft.com/en-us/ef/core/get-started/xamarin
public class YeetMacroDbContext : DbContext
{
    public DbSet<Node> Nodes { get; set; }
    public DbSet<NodeClosure> Closures { get; set; }
    public DbSet<ParentNode> ParentNodes { get; set; }
    public DbSet<LeafNode> LeafNodes { get; set; }
    public DbSet<PatternBase> Patterns { get; set; }
    public DbSet<PatternNode> PatternNodes { get; set; }

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

        modelBuilder.Entity<Node>().HasKey(pn => pn.NodeId);
        modelBuilder.Entity<ParentNode>().HasMany(pn => pn.Children).WithOne().HasForeignKey(n => n.ParentId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<NodeClosure>().HasKey(nc => nc.ClosureId);
        modelBuilder.Entity<NodeClosure>().HasOne(nc => nc.Ancestor).WithMany().HasForeignKey(n => n.AncestorId);
        modelBuilder.Entity<NodeClosure>().HasOne(nc => nc.Descendant).WithMany().HasForeignKey(n => n.DescendantId);
        modelBuilder.Entity<NodeClosure>().Navigation(pn => pn.Ancestor).AutoInclude();
        modelBuilder.Entity<NodeClosure>().Navigation(pn => pn.Descendant).AutoInclude();

        //https://stackoverflow.com/questions/48017766/what-automatically-causes-ef-core-to-include-collection-properties
        modelBuilder.Entity<PatternNode>().Navigation(pn => pn.Patterns).AutoInclude();
        modelBuilder.Entity<PatternNode>().Navigation(pn => pn.UserPatterns).AutoInclude();
        //https://www.tektutorialshub.com/entity-framework-core/cascade-delete-in-entity-framework-core/
        modelBuilder.Entity<PatternNode>().HasMany(pn => pn.Children).WithOne().HasForeignKey(n => n.ParentId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PatternNode>().HasMany(pn => pn.Patterns).WithOne().HasForeignKey(p => p.ParentNodeId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PatternNode>().HasMany(pn => pn.UserPatterns).WithOne().HasForeignKey(p => p.ParentNodeId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PatternBase>().HasKey(p => p.PatternId);
        modelBuilder.Entity<PatternBase>().OwnsOne(p => p.Resolution);
        modelBuilder.Entity<PatternBase>().OwnsOne(p => p.Bounds);
    }
}