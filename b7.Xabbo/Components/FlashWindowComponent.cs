using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Configuration;

using Xabbo.Interceptor;
using Xabbo.Core.Game;
using Xabbo.Core.Events;
using Xabbo.Core;

namespace b7.Xabbo.Components
{
    public class FlashWindowComponent : Component
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        /// <summary>
        /// Flash both the window caption and taskbar button.
        /// This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
        /// </summary>
        const uint FLASHW_ALL = 3;

        /// <summary>
        /// Flash continuously until the window comes to the foreground.
        /// </summary>
        const uint FLASHW_TIMERNOFG = 12;

        private static bool FlashWindow(IntPtr windowHandle)
        {
            FLASHWINFO info = new();

            info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
            info.hwnd = windowHandle;
            info.dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG;
            info.uCount = uint.MaxValue;
            info.dwTimeout = 0;

            return FlashWindowEx(ref info);
        }

        private readonly ProfileManager _profileManager;
        private readonly FriendManager _friendManager;
        private readonly RoomManager _roomManager;

        private string? _currentUserName;
        private Process? _currentProcess;

        private bool _flashOnPrivateMessage;
        public bool FlashOnPrivateMessage
        {
            get => _flashOnPrivateMessage;
            set => Set(ref _flashOnPrivateMessage, value);
        }

        private bool _flashOnWhisper;
        public bool FlashOnWhisper
        {
            get => _flashOnWhisper;
            set => Set(ref _flashOnWhisper, value);
        }

        private bool _flashOnUserChat;
        public bool FlashOnUserChat
        {
            get => _flashOnUserChat;
            set => Set(ref _flashOnUserChat, value);
        }


        private bool _flashOnFriendChat;
        public bool FlashOnFriendChat
        {
            get => _flashOnFriendChat;
            set => Set(ref _flashOnFriendChat, value);
        }

        private bool _flashOnUserEntered;
        public bool FlashOnUserEntered
        {
            get => _flashOnUserEntered;
            set => Set(ref _flashOnUserEntered, value);
        }

        private bool _flashOnFriendEntered;
        public bool FlashOnFriendEntered
        {
            get => _flashOnFriendEntered;
            set => Set(ref _flashOnFriendEntered, value);
        }

        public FlashWindowComponent(IInterceptor interceptor,
            IConfiguration config,
            ProfileManager profileManager,
            FriendManager friendManager,
            RoomManager roomManager)
            : base(interceptor)
        {
            FlashOnPrivateMessage = config.GetValue("FlashWindow:OnPrivateMessage", true);
            FlashOnWhisper = config.GetValue("FlashWindow:OnWhisper", true);
            FlashOnUserChat = config.GetValue("FlashWindow:OnUserChat", false);
            FlashOnFriendChat = config.GetValue("FlashWindow:OnFriendChat", false);
            FlashOnUserEntered = config.GetValue("FlashWindow:OnUserEntered", false);
            FlashOnFriendEntered = config.GetValue("FlashWindow:OnFriendEntered", true);

            _profileManager = profileManager;
            _friendManager = friendManager;
            _roomManager = roomManager;

            _profileManager.LoadedUserData += OnLoadedUserData;
            _friendManager.MessageReceived += OnReceivedPrivateMessage;

            _roomManager.EntitiesAdded += OnEntitiesAdded;
            _roomManager.EntityChat += OnEntityChat;
        }

        private void FlashWindow()
        {
            if (_currentProcess is null) return;

            FlashWindow(_currentProcess.MainWindowHandle);
        }

        private void OnLoadedUserData(object? sender, EventArgs e)
        {
            UserData? userData = _profileManager.UserData;
            
            if (userData is null ||
                userData.Name == _currentUserName)
            {
                return;
            }

            Process? process = Process.GetProcesses().FirstOrDefault(
                p =>
                    p.MainWindowTitle.StartsWith("Habbo") &&
                    p.MainWindowTitle.EndsWith(userData.Name)
            );

            if (process is null) return;

            _currentUserName = userData.Name;
            _currentProcess = process;
        }

        private void OnReceivedPrivateMessage(object? sender, FriendMessageEventArgs e)
        {
            if (FlashOnPrivateMessage)
            {
                FlashWindow();
            }
        }

        private void OnEntityChat(object? sender, EntityChatEventArgs e)
        {
            if (e.Entity is not IRoomUser user) return;

            if (FlashOnUserChat ||
                (FlashOnWhisper && e.ChatType == ChatType.Whisper) ||
                (FlashOnFriendChat && _friendManager.IsFriend(user.Id)))
            {
                FlashWindow();
            }
        }

        private void OnEntitiesAdded(object? sender, EntitiesEventArgs e)
        {
            IEnumerable<IRoomUser> users = e.Entities.OfType<IRoomUser>()
                .Where(u => u.Id != _profileManager.UserData?.Id);

            if ((FlashOnUserEntered && users.Any(u => u.Id != _profileManager.UserData?.Id)) ||
                (FlashOnFriendEntered && users.Any(u => _friendManager.IsFriend(u.Id))))
            {
                FlashWindow();
            }
        }
    }
}
