using System;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Loader.Features.Plugins;

namespace RoleplayNames
{
    // Точка входа LabAPI: РП-имена «ID | РП-имя | ник» для всех ролей
    public sealed class Plugin : Plugin<Config>
    {
        // Доступ к плагину из других классов/плагинов
        public static Plugin Instance { get; private set; }

        // Метаданные плагина для LabAPI
        public override string Name => "RoleplayNames";

        public override string Description => "Выдаёт игрокам РП-имена в зависимости от роли";

        public override string Author => "emielasda";

        public override Version Version { get; } = new Version(1, 0, 0);

        // Версия LabAPI с которой собран плагин
        public override Version RequiredApiVersion { get; } = new Version(LabApiProperties.CompiledVersion);

        // Генератор имён (живёт пока плагин включён)
        public NameGenerator Generator { get; private set; }

        // Обработчик событий; null если плагин выключен
        public EventHandler Handler { get; private set; }

        public override void Enable()
        {
            Instance = this;
            if (Config == null || !Config.IsEnabled)
                return;

            Generator = new NameGenerator(Config);
            Handler = new EventHandler(Config, Generator);

            // Смена роли / выход игрока / рестарт раунда
            PlayerEvents.ChangedRole += Handler.OnChangedRole;
            PlayerEvents.Left += Handler.OnLeft;
            ServerEvents.RoundRestarted += Handler.OnRoundReset;
            ServerEvents.WaitingForPlayers += Handler.OnRoundReset;
        }

        public override void Disable()
        {
            if (Handler != null)
            {
                PlayerEvents.ChangedRole -= Handler.OnChangedRole;
                PlayerEvents.Left -= Handler.OnLeft;
                ServerEvents.RoundRestarted -= Handler.OnRoundReset;
                ServerEvents.WaitingForPlayers -= Handler.OnRoundReset;

                // Возвращаем игрокам обычные ники
                Handler.RestoreAll();
            }

            Handler = null;
            Generator = null;
            Instance = null;
        }
    }
}
