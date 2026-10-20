using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using PlayerRoles;
using Logger = LabApi.Features.Console.Logger;

namespace RoleplayNames
{
    // Выдаёт РП-имена при смене роли и чистит данные между раундами
    public sealed class EventHandler
    {
        private readonly Config _config;
        private readonly NameGenerator _generator;

        // Текущее РП-имя каждого игрока (ключ - PlayerId)
        private readonly Dictionary<int, string> _rpNames = new Dictionary<int, string>();

        public EventHandler(Config config, NameGenerator generator)
        {
            _config = config;
            _generator = generator;
        }

        // Текущее РП-имя игрока или null
        public string GetRpName(Player player) =>
            player != null && _rpNames.TryGetValue(player.PlayerId, out string name) ? name : null;
        // Срабатывает на любую смену роли: спавн в начале раунда / респавн МОГ и Хаоса /
        // воскрешение SCP-049 в SCP-049-2 / смерть (→ Spectator) / forceclass и тд
        public void OnChangedRole(PlayerChangedRoleEventArgs ev)
        {
            Player player = ev?.Player;
            if (!IsValid(player))
                return;

            // Новая роль из события; если её нет - берём текущую у игрока
            RoleTypeId newRole = ev.NewRole != null ? ev.NewRole.RoleTypeId : player.Role;
            string rpName = _generator.Generate(newRole);

            if (rpName == null)
            {
                // Исключённая роль (наблюдатель туториал и тп) - показываем обычный ник
                ResetName(player);
                return;
            }

            _rpNames[player.PlayerId] = rpName;
            ApplyName(player, rpName);
        }

        // Игрок вышел - забываем его
        public void OnLeft(PlayerLeftEventArgs ev)
        {
            if (ev?.Player != null)
                _rpNames.Remove(ev.Player.PlayerId);
        }

        // Рестарт раунда / ожидание игроков - очищаем всё сохранённое
        public void OnRoundReset()
        {
            _rpNames.Clear();
            _generator.Reset();
        }

        // Возвращает обычные ники всем кому плагин выдал РП-имя (при выключении плагина)
        public void RestoreAll()
        {
            foreach (Player player in Player.List)
            {
                if (IsValid(player) && _rpNames.ContainsKey(player.PlayerId))
                    ResetName(player);
            }

            OnRoundReset();
        }

        // Подставляет {id}/{name}/{nick} в формат и выставляет DisplayName
        private void ApplyName(Player player, string rpName)
        {
            string format = string.IsNullOrEmpty(_config.Format) ? "{id} | {name} | {nick}" : _config.Format;
            string display = format
                .Replace("{id}", player.PlayerId.ToString())
                .Replace("{name}", rpName)
                .Replace("{nick}", player.Nickname ?? string.Empty);

            try
            {
                player.DisplayName = display;
            }
            catch (Exception e)
            {
                Logger.Warn($"[RoleplayNames] Не удалось выставить имя игроку {player.PlayerId}: {e.Message}");
            }
        }

        private void ResetName(Player player)
        {
            _rpNames.Remove(player.PlayerId);

            try
            {
                // null сбрасывает кастомное имя - снова виден настоящий ник
                player.DisplayName = null;
            }
            catch (Exception e)
            {
                Logger.Warn($"[RoleplayNames] Не удалось сбросить имя игроку {player.PlayerId}: {e.Message}");
            }
        }

        // Игрок существует ещё на сервере и это не хост-объект сервера
        private static bool IsValid(Player player) =>
            player != null && !player.IsDestroyed && !player.IsHost && player.ReferenceHub != null;
    }
}
