using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Configuration;

using Xabbo.Interceptor;
using Xabbo.Messages;

using b7.Xabbo.Configuration;

namespace b7.Xabbo.Components
{
    public class AntiBobbaComponent : Component
    {
        private static readonly Regex _regexBrackets = new Regex(@"\[([^\[\]]+?)\]", RegexOptions.Compiled);

        private readonly AntiBobbaOptions _options;
        private readonly Regex _regexAuto;

        private bool _isLocalized;
        public bool IsLocalized
        {
            get => _isLocalized;
            set => Set(ref _isLocalized, value);
        }

        private bool _isAutoEnabled;
        public bool IsAutoEnabled
        {
            get => _isAutoEnabled;
            set => Set(ref _isAutoEnabled, value);
        }

        public AntiBobbaComponent(IInterceptor interceptor,
            IConfiguration config)
            : base(interceptor)
        {
            _options = config.GetValue<AntiBobbaOptions>("AntiBobba");

            IsActive = _options.Active;
            IsLocalized = _options.Localized;
            IsAutoEnabled = _options.Auto;

            string autoPattern = "(" + string.Join('|', _options.AutoList.Select(x => Regex.Escape(x))) + ")";
            _regexAuto = new Regex(autoPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        protected override void OnInitialized(object? sender, InterceptorInitializedEventArgs e)
        {
            base.OnInitialized(sender, e);
        }

        private string InjectAntiBobba(Match m)
        {
            var sb = new StringBuilder();
            foreach (var c in m.Groups[1].Value)
            {
                sb.Append(_options.Inject);
                sb.Append(c);
            }
            sb.Append(_options.Inject);
            return sb.ToString();
        }

        [InterceptOut(nameof(Outgoing.Chat), nameof(Outgoing.Shout), nameof(Outgoing.Whisper))]
        private void OnChat(InterceptArgs e)
        {
            if (!IsActive) return;

            string message = e.Packet.ReadString();

            if (!IsLocalized && !IsAutoEnabled)
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
                        sb.Append(_options.Inject);
                    sb.Append(message[i]);
                }
                message = sb.ToString();
            }
            else
            {
                if (IsLocalized)
                {
                    message = _regexBrackets.Replace(message, InjectAntiBobba);
                }

                if (IsAutoEnabled)
                {
                    message = _regexAuto.Replace(message, InjectAntiBobba);
                }
            }

            e.Packet.ReplaceAt(0, message);
        }
    }
}
