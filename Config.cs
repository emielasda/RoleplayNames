using System.Collections.Generic;
using System.ComponentModel;
using PlayerRoles;

namespace RoleplayNames
{
    // Конфиг плагина (configs/<порт>/RoleplayNames/config.yml)
    public sealed class Config
    {
        [Description("Включён ли плагин")]
        public bool IsEnabled { get; set; } = true;

        [Description("Формат отображаемого имени: {id} - ID игрока / {name} - РП-имя / {nick} - настоящий ник")]
        public string Format { get; set; } = "{id} | {name} | {nick}";

        [Description("Префикс номера Class-D")]
        public string ClassDPrefix { get; set; } = "D-";

        [Description("Минимальный номер Class-D (включительно)")]
        public int ClassDNumberMin { get; set; } = 1000;

        [Description("Максимальный номер Class-D (включительно)")]
        public int ClassDNumberMax { get; set; } = 9999;

        [Description("Мужские имена")]
        public List<string> FirstNames { get; set; } = new List<string>
        {
            "Алексей", "Андрей", "Антон", "Артём", "Борис", "Вадим", "Валентин", "Василий",
            "Виктор", "Виталий", "Владимир", "Вячеслав", "Георгий", "Григорий", "Денис", "Дмитрий",
            "Евгений", "Егор", "Иван", "Игорь", "Илья", "Кирилл", "Константин", "Леонид",
            "Максим", "Михаил", "Никита", "Николай", "Олег", "Павел", "Пётр", "Роман",
            "Сергей", "Станислав", "Тимофей", "Фёдор", "Юрий", "Ярослав",
        };

        [Description("Мужские фамилии")]
        public List<string> LastNames { get; set; } = new List<string>
        {
            "Смирнов", "Иванов", "Кузнецов", "Соколов", "Попов", "Лебедев", "Козлов", "Новиков",
            "Морозов", "Петров", "Волков", "Соловьёв", "Васильев", "Зайцев", "Павлов", "Семёнов",
            "Голубев", "Виноградов", "Богданов", "Воробьёв", "Фёдоров", "Михайлов", "Беляев", "Тарасов",
            "Белов", "Комаров", "Орлов", "Киселёв", "Макаров", "Андреев", "Ковалёв", "Ильин",
            "Гусев", "Титов", "Кудрявцев", "Баранов", "Куликов", "Алексеев", "Степанов", "Яковлев",
            "Сорокин", "Карпин", "Никитин", "Белый", "Толстой", "Ковальский", "Вишневский", "Высоцкий",
            "Горький", "Шевченко", "Бондаренко", "Кох", "Черных", "Дурново",
        };

        [Description("Шанс (0-100 %) получить пасхальное имя известного сотрудника Фонда вместо обычного (0 - выключено)")]
        public float EasterEggChance { get; set; } = 3f;

        [Description("Пасхальные имена (каждое выдаётся не больше одного раза за раунд)")]
        public List<string> EasterEggNames { get; set; } = new List<string>
        {
            "Джек Брайт", "Альто Клеф", "Чарльз Гирс", "Бенджамин Кондраки",
            "Эверетт Манн", "Саймон Гласс", "Стивен Стрэндж",
        };

        [Description("Роли которые могут получить пасхальное имя")]
        public List<RoleTypeId> EasterEggRoles { get; set; } = new List<RoleTypeId>
        {
            RoleTypeId.Scientist,
            RoleTypeId.FacilityGuard,
            RoleTypeId.NtfPrivate,
            RoleTypeId.NtfSergeant,
            RoleTypeId.NtfSpecialist,
            RoleTypeId.NtfCaptain,
        };

        [Description("Названия SCP по ролям (если роли нет в списке название строится из RoleTypeId автоматически)")]
        public Dictionary<RoleTypeId, string> ScpNames { get; set; } = new Dictionary<RoleTypeId, string>
        {
            [RoleTypeId.Scp173] = "SCP-173",
            [RoleTypeId.Scp106] = "SCP-106",
            [RoleTypeId.Scp049] = "SCP-049",
            [RoleTypeId.Scp0492] = "SCP-049-2",
            [RoleTypeId.Scp079] = "SCP-079",
            [RoleTypeId.Scp096] = "SCP-096",
            [RoleTypeId.Scp939] = "SCP-939",
            [RoleTypeId.Scp3114] = "SCP-3114",
        };

        [Description("Роли для которых РП-имя не выдаётся (показывается обычный ник)")]
        public List<RoleTypeId> ExcludedRoles { get; set; } = new List<RoleTypeId>
        {
            RoleTypeId.Tutorial,
            RoleTypeId.Spectator,
            RoleTypeId.Overwatch,
            RoleTypeId.Filmmaker,
        };
    }
}
