using System;
using System.Windows.Media;

using CommunityToolkit.Mvvm.ComponentModel;

using Xabbo.Core;

namespace b7.Xabbo.ViewModel;

public class EntityViewModel : ObservableObject
{
    public IEntity Entity { get; }

    public int Index => Entity.Index;
    public long Id => Entity.Id;

    private string _name;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _figure;
    public string Figure
    {
        get => _figure;
        set
        {
            if (SetProperty(ref _figure, value))
            {
                OnPropertyChanged(nameof(AvatarImageUrl));
            }
        }
    }

    private string _motto;
    public string Motto
    {
        get => _motto;
        set => SetProperty(ref _motto, value);
    }

    private bool _isStaff;
    public bool IsStaff
    {
        get => _isStaff;
        set
        {
            if (SetProperty(ref _isStaff, value))
            {
                UpdateVisualGroup();
            }
        }
    }

    private bool _isAmbassador;
    public bool IsAmbassador
    {
        get => _isAmbassador;
        set
        {
            if (SetProperty(ref _isAmbassador, value))
            {
                UpdateVisualGroup();
            }
        }
    }

    private bool _isRoomOwner;
    public bool IsRoomOwner
    {
        get => _isRoomOwner;
        set
        {
            if (SetProperty(ref _isRoomOwner, value))
            {
                UpdateVisualGroup();
            }
        }
    }

    private int _controlLevel;
    public int ControlLevel
    {
        get => _controlLevel;
        set
        {
            if (SetProperty(ref _controlLevel, value))
            {
                OnPropertyChanged(nameof(HasRights));
                UpdateVisualGroup();
            }
        }
    }

    public bool HasRights => ControlLevel > 0;

    private string _imageSource = string.Empty;
    public string ImageSource
    {
        get => _imageSource;
        set => SetProperty(ref _imageSource, value);
    }

    public string VisualGroupName
    {
        get
        {
            if (Entity.Type == EntityType.User)
            {
                if (IsRoomOwner)
                {
                    return "Room Owner";
                }
                else if (IsStaff)
                {
                    return "Staff";
                }
                else if (IsAmbassador)
                {
                    return "Ambassadors";
                }
                else if (HasRights)
                {
                    return ControlLevel switch
                    {
                        4 => "Room Owner",
                        3 => "Group Admins",
                        1 => "Rights Holders",
                        _ => "Users"
                    };
                }
                else
                {
                    return "Users";
                }
            }
            else if (Entity.Type == EntityType.Pet)
            {
                return "Pets";
            }
            else if (Entity.Type == EntityType.PrivateBot || Entity.Type == EntityType.PublicBot)
            {
                return "Bots";
            }
            else
            {
                return string.Empty;
            }
        }
    }

    public int VisualGroupSort
    {
        get
        {
            if (Entity.Type == EntityType.User)
            {
                if (IsRoomOwner)
                {
                    return -300;
                }
                else if (IsStaff)
                {
                    return -200;
                }
                else if (IsAmbassador)
                {
                    return -100;
                }
                else
                {
                    return ControlLevel switch
                    {
                        2 => 0,
                        _ => -ControlLevel
                    };
                }
            }
            else if (Entity.Type == EntityType.PublicBot ||
                     Entity.Type == EntityType.PrivateBot)
            {
                return 100;
            }
            else if (Entity.Type == EntityType.Pet)
            {
                return 200;
            }
            else
            {
                return 1000;
            }
        }
    }

    private Color _headerColor = Colors.Red;
    public Color HeaderColor
    {
        get => _headerColor;
        set => SetProperty(ref _headerColor, value);
    }

    private bool _isIdle;
    public bool IsIdle
    {
        get => _isIdle;
        set => SetProperty(ref _isIdle, value);
    }

    private bool _isTrading;
    public bool IsTrading
    {
        get => _isTrading;
        set => SetProperty(ref _isTrading, value);
    }

    public string AvatarImageUrl
    {
        get
        {
            if (Entity.Type == EntityType.Pet)
                return "";

            return $"https://www.habbo.com/habbo-imaging/avatarimage" +
                $"?size=m" +
                $"&figure={Figure}" +
                $"&direction=4" +
                $"&head_direction=4";
        }
    }

    public EntityViewModel(IEntity entity)
    {
        _name = entity.Name;
        Entity = entity;
        _motto = entity.Motto;
        _figure = entity.Figure;
    }

    private void UpdateVisualGroup()
    {
        OnPropertyChanged(nameof(VisualGroupName));
        OnPropertyChanged(nameof(VisualGroupSort));
    }
}
