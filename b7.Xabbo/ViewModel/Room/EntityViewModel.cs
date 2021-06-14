using System;
using System.Windows.Media;

using GalaSoft.MvvmLight;

using Xabbo.Core;

namespace b7.Xabbo.ViewModel
{
    public class EntityViewModel : ObservableObject
    {
        public IEntity Entity { get; }

        public int Index => Entity.Index;
        public long Id => Entity.Id;

        private string name;
        public string Name
        {
            get => name;
            set => Set(ref name, value);
        }

        private string figure;
        public string Figure
        {
            get => figure;
            set
            {
                if (Set(ref figure, value))
                    RaisePropertyChanged(nameof(AvatarImageUrl));
            }
        }

        private string motto;
        public string Motto
        {
            get => motto;
            set => Set(ref motto, value);
        }

        private bool isStaff;
        public bool IsStaff
        {
            get => isStaff;
            set
            {
                if (Set(ref isStaff, value))
                    UpdateVisualGroup();
            }
        }

        private bool isAmbassador;
        public bool IsAmbassador
        {
            get => isAmbassador;
            set
            {
                if (Set(ref isAmbassador, value))
                    UpdateVisualGroup();
            }
        }

        private bool isRoomOwner;
        public bool IsRoomOwner
        {
            get => isRoomOwner;
            set
            {
                if (Set(ref isRoomOwner, value))
                    UpdateVisualGroup();
            }
        }

        private bool hasRights;
        public bool HasRights
        {
            get => hasRights;
            set
            {
                if (Set(ref hasRights, value))
                    UpdateVisualGroup();
            }
        }

        private string imageSource = string.Empty;
        public string ImageSource
        {
            get => imageSource;
            set => Set(ref imageSource, value);
        }

        public string VisualGroupName
        {
            get
            {
                if (Entity.Type == EntityType.Pet)
                    return "Pets";
                else if (Entity.Type == EntityType.PrivateBot || Entity.Type == EntityType.PublicBot)
                    return "Bots";
                else
                {
                    if (IsRoomOwner)
                        return "Room owner";
                    else if (IsStaff)
                        return "Staff";
                    else if (IsAmbassador)
                        return "Ambassadors";
                    else if (HasRights)
                        return "Rights holders";
                    else
                        return "Users";
                }
            }
        }

        public int VisualGroupSort
        {
            get
            {
                if (IsStaff) return 0;
                else if (IsAmbassador) return 1;
                else if (IsRoomOwner) return 2;
                else if (HasRights) return 3;
                else
                {
                    switch (Entity.Type)
                    {
                        case EntityType.User: return 4;
                        case EntityType.Pet: return 5;
                        case EntityType.PublicBot: return 6;
                        case EntityType.PrivateBot: return 6;
                        default: break;
                    }
                }

                return 100;
            }
        }

        private Color headerColor = Colors.Red;
        public Color HeaderColor
        {
            get => headerColor;
            set => Set(ref headerColor, value);
        }

        private bool isIdle;
        public bool IsIdle
        {
            get => isIdle;
            set => Set(ref isIdle, value);
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
            name = entity.Name;
            Entity = entity;
            motto = entity.Motto;
            figure = entity.Figure;
        }

        private void UpdateVisualGroup()
        {
            RaisePropertyChanged(nameof(VisualGroupName));
            RaisePropertyChanged(nameof(VisualGroupSort));
        }
    }
}
