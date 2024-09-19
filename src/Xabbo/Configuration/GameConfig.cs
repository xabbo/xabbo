namespace Xabbo.Configuration;

public class GameConfig
{
    private HashSet<string> _staffList = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> StaffList
    {
        get => _staffList;
        set
        {
            if (value.Comparer != StringComparer.OrdinalIgnoreCase)
                _staffList = new HashSet<string>(value, StringComparer.OrdinalIgnoreCase);
            else
                _staffList = value;
        }
    }

    private HashSet<string> _ambassadorList = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> AmbassadorList
    {
        get => _ambassadorList;
        set
        {
            if (value.Comparer != StringComparer.OrdinalIgnoreCase)
                _ambassadorList = new HashSet<string>(value, StringComparer.OrdinalIgnoreCase);
            else
                _ambassadorList = value;
        }
    }

    private HashSet<string> _petCommands = new(StringComparer.Ordinal);
    public HashSet<string> PetCommands
    {
        get => _petCommands;
        set
        {
            if (value.Comparer != StringComparer.OrdinalIgnoreCase)
                _petCommands = new HashSet<string>(value, StringComparer.OrdinalIgnoreCase);
            else
                _petCommands = value;
        }
    }
}
