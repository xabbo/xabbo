using System;

using GalaSoft.MvvmLight;

using Xabbo.Interceptor;
using Xabbo.Core;

using b7.Xabbo.Components;

namespace b7.Xabbo.ViewModel
{
    public class GeneralViewManager : ObservableObject
    {
        public AntiKickComponent AntiKick { get; }
        public AntiIdleComponent AntiIdle { get; }
        public AntiBobbaComponent AntiBobba { get; }
        public AntiTypingComponent AntiTyping { get; }
        public AntiTurnComponent AntiTurn { get; }
        public AntiWalkComponent AntiWalk { get; }
        public ClickThroughComponent ClickThrough { get; }
        public ClickToComponent ClickTo { get; }

        public EscapeComponent Escape { get; }
        public ChatComponent Chat { get; }
        public RespectedComponent Respected { get; }
        public FurniActionsComponent FurniActions { get; }
        public RoomEntryComponent RoomEntry { get; }

        public FlattenRoomComponent FlattenRoom { get; }

        public AntiHandItemComponent HandItem { get; }

        /* 
         public DoorbellComponent Doorbell { get; set; }

         public FlattenRoomComponent FlattenRoom { get; set; }

         public AntiHandItemComponent HandItem { get; set; }*/

        public GeneralViewManager(
            AntiKickComponent antiKick,
            AntiIdleComponent antiIdle,
            AntiBobbaComponent antiBobba,
            AntiTypingComponent antiTyping,
            AntiTurnComponent antiTurn,
            AntiWalkComponent antiWalk,
            ClickThroughComponent clickThrough,
            ClickToComponent clickTo,
            EscapeComponent escape,
            ChatComponent chat,
            RespectedComponent respected,
            FurniActionsComponent furniActions,
            RoomEntryComponent roomEntry,
            FlattenRoomComponent flattenRoom,
            AntiHandItemComponent handItem)
        {
            AntiKick = antiKick;
            AntiIdle = antiIdle;
            AntiBobba = antiBobba;
            AntiTyping = antiTyping;
            AntiTurn = antiTurn;
            AntiWalk = antiWalk;
            ClickThrough = clickThrough;
            ClickTo = clickTo;
            Escape = escape;
            Chat = chat;
            Respected = respected;
            FurniActions = furniActions;
            RoomEntry = roomEntry;
            FlattenRoom = flattenRoom;
            HandItem = handItem;
        }
    }
}
