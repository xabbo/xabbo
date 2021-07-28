using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Configuration;

using Xabbo.Interceptor;
using Xabbo.Messages;

namespace b7.Xabbo.Components
{
    public class AntiBobbaComponent : Component
    {
        private static readonly Regex _regexBrackets = new Regex(@"\[([^\[\]]+?)\]", RegexOptions.Compiled);

        private readonly IConfiguration _config;
        private readonly string _injectString;

        private bool _isLocalized = true;
        public bool IsLocalized
        {
            get => _isLocalized;
            set => Set(ref _isLocalized, value);
        }

        public AntiBobbaComponent(IInterceptor interceptor, IConfiguration config)
            : base(interceptor)
        {
            _config = config;
            _injectString = config.GetValue("AntiBobba:Inject", "");

            IsActive = _config.GetValue("AntiBobba:Active", true);
        }

        protected override void OnInitialized(object? sender, InterceptorInitializedEventArgs e)
        {
            base.OnInitialized(sender, e);
        }

        [InterceptOut(nameof(Outgoing.Chat), nameof(Outgoing.Shout), nameof(Outgoing.Whisper))]
        private void OnChat(InterceptArgs e)
        {
            if (!IsActive) return;

            string message = e.Packet.ReadString();

            if (IsLocalized)
            {
                message = _regexBrackets.Replace(message, m =>
                {
                    var sb = new StringBuilder();
                    foreach (var c in m.Groups[1].Value)
                    {
                        sb.Append(_injectString);
                        sb.Append(c);
                    }
                    sb.Append(_injectString);
                    return sb.ToString();
                });
            }
            else
            {
                var sb = new StringBuilder();

                if (e.Packet.Header == Out.Chat ||
                    e.Packet.Header == Out.Shout)
                {
                    if (message.StartsWith(":"))
                        return;
                }
                else if (e.Packet.Header == Out.Whisper)
                {
                    int spaceIndex = message.IndexOf(' ');
                    if (spaceIndex != -1)
                    {
                        sb.Append(message[..(spaceIndex + 1)]);
                        message = message[(spaceIndex + 1)..];
                    }
                }

                for (int i = 0; i < message.Length; i++)
                {
                    if (i > 0)
                        sb.Append(_injectString);
                    sb.Append(message[i]);
                }
                message = sb.ToString();
            }

            e.Packet.ReplaceAt(0, message);
        }
    }
}
