using Microsoft.EntityFrameworkCore;

namespace PromoCodeFactory.DataAccess.Repositories;

public static class DebugContext
{
    public static void ShowChangeTacker(this DbContext context, string msg)
    {
        Console.WriteLine($"[{msg}]");
        var entries = context.ChangeTracker.Entries()
            .Select(x => new
            {
                Entity = x.Entity.GetType().Name,
                x.State
            })
            .ToList();
        foreach (var item in entries)
        {
            Console.WriteLine($"[{item.Entity}] - [{item.State}]");
        }
    }
}
