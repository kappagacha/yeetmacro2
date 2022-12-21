using Microsoft.EntityFrameworkCore;
using YeetMacro2.Data.Models;

namespace YeetMacro2.Data.Services;
public static class EfDataProviderExtensions
{
    public static void AddYeetMacroData(this IServiceCollection services, Action<DbContextOptionsBuilder> setup = null)
    {
        services.AddDbContext<YeetMacroDbContext>(setup);
        services.AddTransient<IRepository<PatternNode>, EfRepository<YeetMacroDbContext, PatternNode>>();
        services.AddTransient<IRepository<PatternBase>, EfRepository<YeetMacroDbContext, PatternBase>>();
        services.AddTransient<IRepository<Pattern>, EfRepository<YeetMacroDbContext, Pattern>>();
        services.AddTransient<IRepository<UserPattern>, EfRepository<YeetMacroDbContext, UserPattern>>();
        services.AddTransient<IRepository<Node>, EfRepository<YeetMacroDbContext, Node>>();
        services.AddTransient<IRepository<NodeClosure>, EfRepository<YeetMacroDbContext, NodeClosure>>();
        services.AddTransient<IRepository<ParentNode>, EfRepository<YeetMacroDbContext, ParentNode>>();
        services.AddTransient<IRepository<LeafNode>, EfRepository<YeetMacroDbContext, LeafNode>>();
        services.AddTransient<INodeService<PatternNode, PatternNode>, NodeService<PatternNode, PatternNode>>();
    }
}