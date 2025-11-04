using Microsoft.EntityFrameworkCore;

namespace YeetMacro2.Data.Services;
public static class EfDataProviderExtensions
{
    public static void AddYeetMacroData(this IServiceCollection services, Action<DbContextOptionsBuilder> setup = null, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        // https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#avoiding-dbcontext-threading-issues
        services.AddDbContext<YeetMacroDbContext>(setup, lifetime);

        // registering open generic types is so clean
        services.AddTransient(typeof(IRepository<>), typeof(YeetMacroEfRepository<>));
        services.AddTransient(typeof(INodeService<,>), typeof(NodeService<,>));
        services.AddTransient<INodeTagService, NodeTagService>();

        //services.AddTransient<IRepository<MacroSet>, EfRepository<YeetMacroDbContext, MacroSet>>();
        //services.AddTransient<IRepository<PatternNode>, EfRepository<YeetMacroDbContext, PatternNode>>();
        //services.AddTransient<IRepository<PatternBase>, EfRepository<YeetMacroDbContext, PatternBase>>();
        //services.AddTransient<IRepository<Pattern>, EfRepository<YeetMacroDbContext, Pattern>>();
        //services.AddTransient<IRepository<UserPattern>, EfRepository<YeetMacroDbContext, UserPattern>>();
        //services.AddTransient<IRepository<Node>, EfRepository<YeetMacroDbContext, Node>>();
        //services.AddTransient<IRepository<NodeClosure>, EfRepository<YeetMacroDbContext, NodeClosure>>();
        ////services.AddTransient<IRepository<ParentNode>, EfRepository<YeetMacroDbContext, ParentNode>>();
        ////services.AddTransient<IRepository<LeafNode>, EfRepository<YeetMacroDbContext, LeafNode>>();
        //services.AddTransient<INodeService<PatternNode, PatternNode>, NodeService<PatternNode, PatternNode>>();
        //services.AddTransient<IRepository<Script>, EfRepository<YeetMacroDbContext, Script>>();
    }
}