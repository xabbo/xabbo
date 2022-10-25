using System;
using System.Collections.Generic;

using LiteDB;

using b7.Xabbo.Model;
using Microsoft.Extensions.Configuration;

namespace b7.Xabbo.Services;

public class LiteDbWardrobeRepository : IWardrobeRepository
{
    public string Path { get; }

    public LiteDbWardrobeRepository(IConfiguration config)
    {
        Path = config.GetValue("Wardrobe:Path", "wardrobe.db");
    }

    private ILiteRepository OpenDatabase() => new LiteRepository(Path);

    public void Initialize()
    {
        using (var db = OpenDatabase())
        {
            db.EnsureIndex<FigureModel>(nameof(FigureModel.FigureString));
        }
    }

    public IEnumerable<FigureModel> Load()
    {
        using (var db = OpenDatabase())
        {
            return db.Query<FigureModel>().ToList();
        }
    }

    public bool Insert(FigureModel figure)
    {
        using (var db = OpenDatabase())
        {
            if (db.Query<FigureModel>()
                .Where(x => x.FigureString == figure.FigureString)
                .Exists())
            {
                return false;
            }
            else
            {
                db.Insert(figure);
                return true;
            }
        }
    }

    public int Insert(IEnumerable<FigureModel> figures)
    {
        using (var db = OpenDatabase())
        {
            int c = 0;
            foreach (var figure in figures)
            {
                if (!db.Query<FigureModel>()
                    .Where(x => x.FigureString == figure.FigureString)
                    .Exists())
                {
                    db.Insert(figure);
                    c++;
                }
            }
            return c;
        }
    }

    public bool Update(FigureModel figure)
    {
        using (var db = OpenDatabase())
        {
            return db.Update(figure);
        }
    }

    public int Update(IEnumerable<FigureModel> figures)
    {
        using (var db = OpenDatabase())
        {
            return db.Update(figures);
        }
    }

    public bool Delete(FigureModel figure)
    {
        using (var db = OpenDatabase())
        {
            return db.Delete<FigureModel>(figure.Id);
        }
    }

    public int Delete(IEnumerable<FigureModel> figures)
    {
        using (var db = OpenDatabase())
        {
            int c = 0;
            foreach (var figure in figures)
                if (db.Delete<FigureModel>(figure.Id))
                    c++;
            return c;
        }
    }
}
