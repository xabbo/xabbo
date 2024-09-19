using Xabbo.Ext.Model;

namespace Xabbo.Ext.Services;

public interface IWardrobeRepository
{
    void Initialize();

    IEnumerable<FigureModel> Load();

    bool Add(FigureModel figure);
    int Add(IEnumerable<FigureModel> figures);
    bool Update(FigureModel figure);
    int Update(IEnumerable<FigureModel> figures);
    bool Remove(FigureModel figure);
    int Remove(IEnumerable<FigureModel> figures);
}
