using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.Extensions.Hosting;

using GalaSoft.MvvmLight.Command;

using Xabbo.Messages;
using Xabbo.Interceptor;

using Xabbo.Core;
using Xabbo.Core.Game;
using Xabbo.Core.Events;

namespace b7.Xabbo.ViewModel
{
    public class MimicViewManager : ComponentViewModel
    {
        private ProfileManager _profileManager;
        private RoomManager _roomManager;

        enum State { Initializing, Inactive, SelectingTarget, Active }
        State state = State.Initializing;
        private bool Active => state == State.Active;

        private long selfId = -1, targetId = -1;
        private IRoomUser? self, target;

        private bool _isAvailable;
        public bool IsAvailable
        {
            get => _isAvailable;
            set => Set(ref _isAvailable, value);
        }

        private bool isInitialized;
        public bool IsInitialized
        {
            get => isInitialized;
            set => Set(ref isInitialized, value);
        }

        private bool mimicFigure;
        public bool MimicFigure
        {
            get => mimicFigure;
            set => Set(ref mimicFigure, value);
        }

        private bool mimicMotto;
        public bool MimicMotto
        {
            get => mimicMotto;
            set => Set(ref mimicMotto, value);
        }

        private bool mimicAction;
        public bool MimicAction
        {
            get => mimicAction;
            set => Set(ref mimicAction, value);
        }

        private bool mimicDance;
        public bool MimicDance
        {
            get => mimicDance;
            set => Set(ref mimicDance, value);
        }

        private bool mimicSign;
        public bool MimicSign
        {
            get => mimicSign;
            set => Set(ref mimicSign, value);
        }

        private bool mimicEffect;
        public bool MimicEffect
        {
            get => mimicEffect;
            set => Set(ref mimicEffect, value);
        }

        private bool mimicSit;
        public bool MimicSit
        {
            get => mimicSit;
            set => Set(ref mimicSit, value);
        }

        private bool followTarget;
        public bool FollowTarget
        {
            get => followTarget;
            set => Set(ref followTarget, value);
        }

        private bool mimicTyping;
        public bool MimicTyping
        {
            get => mimicTyping;
            set => Set(ref mimicTyping, value);
        }

        private bool mimicTalk;
        public bool MimicTalk
        {
            get => mimicTalk;
            set => Set(ref mimicTalk, value);
        }

        private bool mimicShout;
        public bool MimicShout
        {
            get => mimicShout;
            set => Set(ref mimicShout, value);
        }

        private bool mimicWhisper;
        public bool MimicWhisper
        {
            get => mimicWhisper;
            set => Set(ref mimicWhisper, value);
        }

        private bool delaySpeech;
        public bool DelaySpeech
        {
            get => delaySpeech;
            set => Set(ref delaySpeech, value);
        }

        private int speechDelay;
        public int SpeechDelay
        {
            get => speechDelay;
            set => Set(ref speechDelay, value);
        }

        private string buttonText = "...";
        public string ButtonText
        {
            get => buttonText;
            set => Set(ref buttonText, value);
        }

        private string statusText;
        public string StatusText
        {
            get => statusText;
            set => Set(ref statusText, value);
        }

        public ICommand EnableDisableCommand { get; }

        public MimicViewManager(IInterceptor interceptor,
            IHostApplicationLifetime lifetime,
            ProfileManager profileManager,
            RoomManager roomManager)
            : base(interceptor)
        {
            _profileManager = profileManager;
            _roomManager = roomManager;

            EnableDisableCommand = new RelayCommand(OnEnableDisable);

            lifetime.ApplicationStarted.Register(() => Task.Run(InitializeAsync));
        }

        private async Task InitializeAsync()
        {
            try
            {
                StatusText = "loading user data...";
                await _profileManager.GetUserDataAsync();
            }
            catch
            {
                StatusText = "unavailable";
                return;
            }

            selfId = _profileManager.UserData?.Id ?? -1;
            StatusText = "(re-)enter room to initialize";

            _roomManager.Entering += RoomManager_Entering;
            _roomManager.EntitiesAdded += EntityManager_EntitiesAdded;
            _roomManager.EntityRemoved += EntityManager_EntityRemoved;
            _roomManager.UserDataUpdated += EntityManager_UserDataUpdated;
            _roomManager.EntityAction += EntityManager_EntityAction;
            _roomManager.EntityIdle += EntityManager_EntityIdle;
            _roomManager.EntityDance += EntityManager_EntityDance;
            _roomManager.EntityEffect += EntityManager_EntityEffect;
            _roomManager.EntityTyping += EntityManager_EntityTyping;
            _roomManager.EntityUpdated += EntityManager_EntityUpdated;
            _roomManager.EntityChat += ChatManager_EntityChat;

            IsAvailable = true;
        }

        public override async void RaisePropertyChanged(string propertyName)
        {
            base.RaisePropertyChanged(propertyName);

            if (!Active)
                    return;

            switch (propertyName)
            {
                case "MimicFigure":
                    if (MimicFigure)
                        await SetFigureAsync(target);
                    break;
                case "MimicMotto":
                    if (MimicMotto)
                        await SetMottoAsync(target);
                    break;
                case "MimicAction":
                    await SetIdleAsync(MimicAction ? target : null);
                    break;
                case "MimicDance":
                    await SetDanceAsync(MimicDance ? target : null);
                    break;
                case "MimicEffect":
                    await SetEffectAsync(MimicEffect ? target : null);
                    break;
                case "MimicSit":
                    await SetSittingAsync(MimicSit ? target : null);
                    break;
                case "MimicTyping":
                    await SetTypingAsync(MimicTyping ? target : null);
                    break;

                default:
                    break;
            }
        }

        private async void OnEnableDisable()
        {
            await SetStateAsync(state == State.Inactive ? State.SelectingTarget : State.Inactive);
        }

        private async void RoomManager_Entering(object? sender, EventArgs e)
        {
            target = null;
            self = null;

            if (!IsInitialized)
            {
                IsInitialized = true;
                await SetStateAsync(State.Inactive);
            }
        }

        private async void EntityManager_EntitiesAdded(object? sender, EntitiesEventArgs e)
        {
            foreach (var user in e.Entities.OfType<IRoomUser>())
            {
                if (user.Id == selfId)
                {
                    self = user;
                    if (target != null)
                        await AttachAsync(target);
                }
                else if (user.Id == targetId)
                {
                    await SendInfoMessageAsync("Mimic target found");
                    target = user;
                    if (self != null)
                        await AttachAsync(user);
                }
            }
        }

        private async void EntityManager_EntityRemoved(object? sender, EntityEventArgs e)
        {
            var entity = e.Entity;
            if (entity.Id == targetId)
            {
                target = null;
                await AttachAsync(null);
                if (followTarget)
                    await SendAsync(Out.Move, entity.X, entity.Y);
                await SendInfoMessageAsync("Mimic target left the room");
            }
        }

        private async Task SetStateAsync(State newState)
        {
            if (state == newState)
                return;

            switch (newState)
            {
                case State.Inactive:
                    {
                        target = null;
                        targetId = -1;
                        ButtonText = "Select target";
                        StatusText = "";
                        // TODO move this out
                        SetTarget(null);
                        if (state == State.Active)
                            await AttachAsync(null);
                    }
                    break;
                case State.SelectingTarget:
                    {
                        ButtonText = "Finding target...";
                        StatusText = "Click user to select";
                    }
                    break;
                case State.Active:
                    {
                        ButtonText = "Disable";
                        StatusText = $"Target: {target.Name}";
                    }
                    break;
                default:
                    return;
            }

            state = newState;
        }

        #region /* Message handlers */
        [InterceptOut(nameof(Outgoing.GetSelectedBadges))]
        private async void OnSelectUser(InterceptArgs e)
        {
            IRoom? room = _roomManager.Room;
            if (room is null) return;

            if (state == State.SelectingTarget)
            {
                long id = e.Packet.ReadLegacyLong();
                if (id == selfId)
                    return;

                if (room.TryGetEntityById(id, out IRoomUser? user))
                    await SetMimicTargetAsync(user);
            }
        }

        [InterceptOut(nameof(Outgoing.LookTo))]
        private void OnLookAtPoint(InterceptArgs e)
        {
            if (state == State.SelectingTarget)
                e.Block();
        }

        private async void EntityManager_UserDataUpdated(object? sender, UserDataUpdatedEventArgs e)
        {
            var user = e.User;

            if (user.Id == targetId)
            {
                if (e.FigureUpdated || e.GenderUpdated)
                    await SetFigureAsync(user);
                if (e.MottoUpdated)
                    await SetMottoAsync(user);
            }
        }

        private async void EntityManager_EntityAction(object? sender, EntityActionEventArgs e)
        {
            if (e.Entity.Id == targetId && e.Action != Actions.Idle)
                await DoActionAsync((int)e.Action);
        }

        private async void EntityManager_EntityIdle(object? sender, EntityIdleEventArgs e)
        {
            // TODO: fix unidle bug
            // if someone is idle, then does an action,
            // the client receives RoomUserAction then RoomUnitIdle 0,
            // so it will mimic that action, then unidle, which cancels it out

            if (e.Entity is RoomUser user &&
                user.Id == targetId &&
                user.IsIdle != e.WasIdle)
            {
                await SetIdleAsync(user);
            }
        }

        private async void EntityManager_EntityDance(object? sender, EntityDanceEventArgs e)
        {
            if (e.Entity is RoomUser user &&
                user.Id == targetId &&
                user.Dance != e.PreviousDance)
            {
                await SetDanceAsync(user);
            }
        }

        private async void EntityManager_EntityEffect(object? sender, EntityEffectEventArgs e)
        {
            if (e.Entity is RoomUser user &&
                user.Id == targetId &&
                user.Effect != e.PreviousEffect)
            {
                await SetEffectAsync(user);
            }
        }

        private async void EntityManager_EntityTyping(object? sender, EntityTypingEventArgs e)
        {
            if (e.Entity is RoomUser user &&
                user.Id == targetId &&
                user.IsTyping != e.WasTyping)
            {
                await SetTypingAsync(user);
            }
        }

        private async void EntityManager_EntityUpdated(object? sender, EntityEventArgs e)
        {
            if (e.Entity is RoomUser user)
            {
                if (user == self)
                    await HandleSelfUpdateAsync(user);
                else if (user == target)
                    await HandleTargetUpdateAsync(user);
            }
        }

        private async void ChatManager_EntityChat(object? sender, EntityChatEventArgs e)
        {
            if ((e.ChatType == ChatType.Talk && !mimicTalk) ||
                (e.ChatType == ChatType.Shout && !mimicShout) ||
                (e.ChatType == ChatType.Whisper && !mimicWhisper))
            {
                return;
            }

            if (e.Entity is RoomUser user &&
                user.Id == targetId)
            {
                string message = e.Message;
                if (e.ChatType == ChatType.Whisper)
                    message = $"{user.Name} {message}";

                if (DelaySpeech && SpeechDelay > 0)
                {
                    await Task.Yield();
                    await Task.Delay(SpeechDelay);
                }

                await SendChatAsync(e.ChatType, message, e.BubbleStyle);
            }
        }

        private async Task HandleTargetUpdateAsync(IRoomUser user)
        {
            if (user.PreviousUpdate is null ||
                user.CurrentUpdate is null) return;

            if (user.PreviousUpdate.SittingOnFloor != user.CurrentUpdate.SittingOnFloor)
                await SetSittingAsync(user);
            if (user.PreviousUpdate.Sign != user.CurrentUpdate.Sign && user.CurrentUpdate.Sign != Signs.None)
                await SendSignAsync((int)user.CurrentUpdate.Sign);

            if (user.X != user.PreviousUpdate.Location.X ||
                user.Y != user.PreviousUpdate.Location.Y)
            {
                if (followTarget)
                    await SendAsync(Out.Move, user.PreviousUpdate.Location.X, user.PreviousUpdate.Location.Y);
            }
        }

        private async Task HandleSelfUpdateAsync(RoomUser user)
        {
            if (target is null ||
                user.PreviousUpdate is null ||
                user.CurrentUpdate is null)
            {
                return;
            }

            if (user.PreviousUpdate.Stance == Stances.Sit &&
                user.CurrentUpdate.Stance == Stances.Stand &&
                target.Dance != 0)
            {
                await SetDanceAsync(target);
            }

            if (followTarget &&
                user.PreviousUpdate.MovingTo != null &&
                user.CurrentUpdate.MovingTo == null) // Stopped
            {
                if (target.PreviousUpdate != null && (user.CurrentUpdate.Location != target.PreviousUpdate.Location))
                {
                    await SendAsync(Out.Move, target.PreviousUpdate.Location.X, target.PreviousUpdate.Location.Y);
                }
            }
        }
        #endregion

        #region /* Mimic functions */
        private Task SendInfoMessageAsync(string message)
            => SendAsync(In.Whisper, self?.Index ?? -1, message, 0, 34, 0, 0);

        private async Task SetFigureAsync(IRoomUser? user)
        {
            if (!mimicFigure || user == null)
                return;
            if (self != null &&
                self.Gender.Equals(user.Gender) &&
                self.Figure.Equals(user.Figure))
                return;

            await SendAsync(Out.UpdateAvatar,
                user.Gender.ToShortString(),
                user.Figure
            );
        }

        private async Task SetMottoAsync(IRoomUser? user)
        {
            if (!mimicMotto || user == null)
                return;
            if (self != null &&
                self.Motto.Equals(user.Motto))
                return;
            await SendAsync(Out.ChangeAvatarMotto, user.Motto);
        }

        private async Task DoActionAsync(int action)
        {
            if (!mimicAction)
                return;
            await SendAsync(Out.Expression, action);
        }

        private async Task SetIdleAsync(IRoomUser? user)
        {
            bool idle;
            if (user != null)
            {
                if (!mimicAction)
                    return;
                idle = user.IsIdle;
            }
            else
                idle = false;

            if (self != null && self.IsIdle == idle)
                return;

            await SendAsync(Out.Expression, idle ? 5 : 0);
        }

        private async Task SetDanceAsync(IRoomUser? user)
        {
            int dance;
            if (user != null)
            {
                if (!mimicDance)
                    return;
                dance = user.Dance;
            }
            else
                dance = 0;

            if (self != null && self.Dance.Equals(dance))
                return;

            await SendAsync(Out.Dance, dance);
        }

        private async Task SendSignAsync(int sign)
        {
            if (!mimicSign)
                return;
            await SendAsync(Out.ShowSign, sign);
        }

        private async Task SetEffectAsync(IRoomUser? user)
        {
            int effect;
            if (user != null)
            {
                if (!mimicEffect)
                    return;
                effect = user.Effect;
            }
            else
                effect = 0;

            if (self != null && self.Effect.Equals(effect))
                return;

            if (effect == 0) effect = -1;

            if (!await SendSpecialEffectAsync(effect))
            {
                if (await SendSpecialEffectAsync(self?.Effect ?? 0) && effect == -1)
                    return;
                await SendAsync(Out.UseAvatarEffect, effect);
            }
        }

        private async Task<bool> SendSpecialEffectAsync(int effect)
        {
            switch (effect)
            {
                case 0x8c: await SendChatAsync(":habnam"); break;
                case 0x88: await SendChatAsync(":moonwalk"); break;
                case 0xc4: await SendChatAsync(":yyxxabxa"); break;
                default: return false;
            }
            return true;
        }

        private async Task SetTypingAsync(IRoomUser? user)
        {
            bool typing;
            if (user != null)
            {
                if (!mimicTyping)
                    return;
                typing = user.IsTyping;
            }
            else
                typing = false;

            if (self != null && self.IsTyping.Equals(typing))
                return;

            await SendAsync(typing ? Out.UserStartTyping : Out.UserCancelTyping);
        }

        private Task SendChatAsync(string message) => SendChatAsync(ChatType.Talk, message);

        private async Task SendChatAsync(ChatType type, string message, int chatBubbleStyle = 0)
        {
            var packet = new Packet();
            switch (type)
            {
                case ChatType.Talk: packet.Header = Out.Chat; break;
                case ChatType.Shout: packet.Header = Out.Shout; break;
                case ChatType.Whisper: packet.Header = Out.Whisper; break;
                default: return;
            }

            packet.WriteString(message);
            packet.WriteInt(chatBubbleStyle);
            if (type == ChatType.Talk)
                packet.WriteInt(-1);

            await SendAsync(packet);
        }

        private async Task SetSittingAsync(IEntity e)
        {
            bool sitting;
            if (e != null)
            {
                if (!MimicSit)
                    return;
                sitting = e.CurrentUpdate?.SittingOnFloor ?? false;
            }
            else
                sitting = false;

            if (self?.CurrentUpdate != null && self.CurrentUpdate.SittingOnFloor == sitting)
                return;

            await SendAsync(Out.Posture, sitting ? 1 : 0);
        }

        private void SetTarget(IRoomUser user)
        {
            target = user;
            targetId = target?.Id ?? -1;
        }

        private async Task SetMimicTargetAsync(IRoomUser user)
        {
            SetTarget(user);
            await SetStateAsync(State.Active);
            await AttachAsync(user);

            await SendInfoMessageAsync($"Mimic target set to {user.Name}");
        }

        private async Task AttachAsync(IRoomUser? user)
        {
            if (state == State.Active)
            {
                await SetFigureAsync(user);
                await SetMottoAsync(user);
                await SetDanceAsync(user);
                await SetEffectAsync(user);
                await SetSittingAsync(user);
            }
            await SetTypingAsync(user);
            await SetIdleAsync(user);
        }
        #endregion
    }
}
