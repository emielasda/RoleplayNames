using System;
using System.Collections.Generic;
using System.Linq;
using PlayerRoles;

namespace RoleplayNames
{
    // Генерирует РП-имена и следит чтобы они не повторялись в пределах раунда
    public sealed class NameGenerator
    {
        // Сколько раз пробуем случайный вариант прежде чем перебирать все свободные
        private const int RandomAttempts = 64;

        private readonly Config _config;
        private readonly Random _random = new Random();

        // Уже выданные в этом раунде номера Class-D и сочетания «имя фамилия»
        private readonly HashSet<int> _usedClassDNumbers = new HashSet<int>();
        private readonly HashSet<string> _usedFullNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public NameGenerator(Config config)
        {
            _config = config;
        }

        // Сбрасывает всё что выдано за раунд
        public void Reset()
        {
            _usedClassDNumbers.Clear();
            _usedFullNames.Clear();
        }

        // РП-имя для роли или null если для роли имя не положено
        public string Generate(RoleTypeId role)
        {
            if (role == RoleTypeId.None || role == RoleTypeId.Destroyed)
                return null;

            if (_config.ExcludedRoles != null && _config.ExcludedRoles.Contains(role))
                return null;

            switch (role.GetTeam())
            {
                case Team.ClassD:
                    return GenerateClassD();
                case Team.SCPs:
                    return GetScpName(role);
                case Team.Scientists:
                case Team.FoundationForces:
                case Team.ChaosInsurgency:
                    return TryEasterEgg(role) ?? GenerateFullName();
                default:
                    // Фламинго Tutorial (если его убрали из исключений) и прочие - без РП-имени
                    return null;
            }
        }

        // «D-» + уникальный номер в пределах раунда
        private string GenerateClassD()
        {
            int min = Math.Min(_config.ClassDNumberMin, _config.ClassDNumberMax);
            int max = Math.Max(_config.ClassDNumberMin, _config.ClassDNumberMax);
            long rangeSize = (long)max - min + 1;

            int number = -1;
            for (int i = 0; i < RandomAttempts; i++)
            {
                int candidate = NextInclusive(min, max);
                if (!_usedClassDNumbers.Contains(candidate))
                {
                    number = candidate;
                    break;
                }
            }

            // Случайно не нашли - перебираем свободные номера (если они вообще остались)
            if (number < 0 && _usedClassDNumbers.Count < rangeSize)
            {
                int start = NextInclusive(min, max);
                for (long offset = 0; offset < rangeSize; offset++)
                {
                    int candidate = (int)(min + (start - min + offset) % rangeSize);
                    if (!_usedClassDNumbers.Contains(candidate))
                    {
                        number = candidate;
                        break;
                    }
                }
            }

            // Все номера заняты - допускаем повтор
            if (number < 0)
                number = NextInclusive(min, max);

            _usedClassDNumbers.Add(number);
            return (_config.ClassDPrefix ?? string.Empty) + number;
        }

        // Название SCP из конфига или автоматически из RoleTypeId (Scp1507 → SCP-1507)
        private string GetScpName(RoleTypeId role)
        {
            if (_config.ScpNames != null && _config.ScpNames.TryGetValue(role, out string name) && !string.IsNullOrWhiteSpace(name))
                return name;

            string raw = role.ToString();
            if (raw.StartsWith("Scp", StringComparison.OrdinalIgnoreCase) && raw.Length > 3)
                return "SCP-" + raw.Substring(3);

            return raw.ToUpperInvariant();
        }

        // Пасхалка: с шансом EasterEggChance - имя известного сотрудника Фонда (одно на раунд) иначе null
        private string TryEasterEgg(RoleTypeId role)
        {
            if (_config.EasterEggChance <= 0f || _config.EasterEggRoles == null || !_config.EasterEggRoles.Contains(role))
                return null;

            if (_random.NextDouble() * 100.0 >= _config.EasterEggChance)
                return null;

            // Только те что ещё не выдавались в этом раунде
            List<string> free = Clean(_config.EasterEggNames).Where(n => !_usedFullNames.Contains(n)).ToList();
            if (free.Count == 0)
                return null;

            string name = free[_random.Next(free.Count)];
            _usedFullNames.Add(name);
            return name;
        }

        // Случайные мужские имя и фамилия без повторов в пределах раунда (пока есть свободные сочетания)
        private string GenerateFullName()
        {
            List<string> firstNames = Clean(_config.FirstNames);
            List<string> lastNames = Clean(_config.LastNames);

            if (firstNames.Count == 0 || lastNames.Count == 0)
                return null;

            for (int i = 0; i < RandomAttempts; i++)
            {
                string fullName = firstNames[_random.Next(firstNames.Count)] + " " + lastNames[_random.Next(lastNames.Count)];
                if (_usedFullNames.Add(fullName))
                    return fullName;
            }

            // Перебираем все сочетания в случайном порядке и берём первое свободное
            IEnumerable<string> all = firstNames
                .SelectMany(f => lastNames.Select(l => f + " " + l))
                .OrderBy(_ => _random.Next());

            foreach (string fullName in all)
            {
                if (_usedFullNames.Add(fullName))
                    return fullName;
            }

            // Сочетания закончились - допускаем повтор
            return firstNames[_random.Next(firstNames.Count)] + " " + lastNames[_random.Next(lastNames.Count)];
        }

        // Убирает пустые строки и пробелы по краям (на случай кривого конфига)
        private static List<string> Clean(List<string> source) =>
            source?.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList() ?? new List<string>();

        // Случайное число в [min; max] включительно (Random.Next не берёт max)
        private int NextInclusive(int min, int max) =>
            max == int.MaxValue ? (int)(min + (long)(_random.NextDouble() * ((long)max - min + 1))) : _random.Next(min, max + 1);
    }
}
