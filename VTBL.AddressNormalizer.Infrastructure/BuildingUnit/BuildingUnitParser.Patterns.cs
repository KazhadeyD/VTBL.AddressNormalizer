using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using VTBL.AddressNormalizer.Infrastructure.Shared;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VTBL.AddressNormalizer.Infrastructure.BuildingUnit
{
    /// <summary>
    /// Регулярные выражения парсера <see cref="BuildingUnitParser"/>.
    /// </summary>
    public sealed partial class BuildingUnitParser
    {
        /// <summary>
        /// Схлопывание повторяющихся пробельных символов в один пробел.
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>\s+</c></para>
        /// <para>Используется в <see cref="Preprocess"/>, после каждого <c>Extract*</c> и в <see cref="NormalizeValue"/>.</para>
        /// </remarks>
        private static readonly Regex WhitespaceCollapseRegex = new Regex(
            @"\s+",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Квартира: «КВАРТИРА 837», «КВ. 10», списки «13К1, 3, 6».
        /// </summary>
        private static readonly Regex ApartmentRegex =
            IndoorMarkerRegexFactory.MarkerThenDigitValueList(IndoorMarkerLexemes.Apartment);

        /// <summary>
        /// Кабинет: «КАБИНЕТ 69», списки кабинетов.
        /// </summary>
        private static readonly Regex CabinetRegex =
            IndoorMarkerRegexFactory.MarkerThenDigitValueList(IndoorMarkerLexemes.Cabinet);

        /// <summary>
        /// Подъезд: «ПОДЪЕЗД 5», «ПОДЪЕЗД/ЭТ 2».
        /// </summary>
        private static readonly Regex EntranceRegex = new Regex(
            $@"(?<!\p{{L}}){IndoorMarkerLexemes.Entrance}(?:/ЭТАЖ|/ЭТ)?\s*(?<v>[\d\w\-/]+)?",
            IndoorMarkerRegexFactory.MarkerOptions);

        /// <summary>
        /// Проезд: «ПРОЕЗД 1», «ПР-Д 1», «1-й проезд».
        /// </summary>
        private static readonly Regex PassageRegex = new Regex(
            $@"(?<!\p{{L}})(?:(?:{IndoorMarkerLexemes.Passage})\.?\s*(?<v>\d[\d\w\-/]*)|(?<v>\d+)\s*-\s*[ЙЯ]\s+(?:{IndoorMarkerLexemes.Passage}))",
            IndoorMarkerRegexFactory.MarkerOptions);

        /// <summary>
        /// Владение: «ВЛАДЕНИЕ 1», «ВЛАД 1», «ВЛ. 1».
        /// </summary>
        private static readonly Regex HoldingRegex =
            IndoorMarkerRegexFactory.MarkerThenDigitValue(IndoorMarkerLexemes.Holding);

        /// <summary>
        /// Склад: «СКЛАД 1», «СКЛ. 1».
        /// </summary>
        private static readonly Regex StorageRegex =
            IndoorMarkerRegexFactory.MarkerThenDigitValue(IndoorMarkerLexemes.Storage);

        /// <summary>
        /// Составной маркер «БЛОК-СЕКЦИЯ» с одним номером.
        /// </summary>
        /// <remarks>
        /// Обрабатывается в <see cref="ExtractBlockSection"/> до <see cref="BlockRegex"/>.
        /// </remarks>
        private static readonly Regex BlockSectionRegex = new Regex(
            $@"(?<!\p{{L}}){IndoorMarkerLexemes.Block}-{IndoorMarkerLexemes.SectionFull}\s*(?<v>[\d\w\-]+)?",
            IndoorMarkerRegexFactory.MarkerOptions);

        /// <summary>
        /// Блок: «БЛОК Е», «БЛОК 1».
        /// </summary>
        private static readonly Regex BlockRegex =
            IndoorMarkerRegexFactory.MarkerThenOptionalToken(IndoorMarkerLexemes.Block);

        /// <summary>
        /// Шаблон токена типа в slash-цепочке (этаж, пом, ком, оф, каб, раб.м).
        /// </summary>
        private const string SlashChainHeaderTokenPattern =
            @"ЭТАЖ|ЭТ\.?|ПОМЕЩЕНИЯ|ПОМЕЩЕНИЕ|ПОМЕЩ\.?|ПОМ\.?|КОМНАТА|КОМН\.?|КОМ\.?|ОФИС|ОФ\.?|КАБИНЕТ|КАБ\.?|РАБ\.?\s*М(?:ЕСТО)?";

        /// <summary>
        /// Цепочка заголовков slash-нотации без значений: «ЭТАЖ/ПОМЕЩ.», «КОМ./ОФИС».
        /// </summary>
        private static readonly Regex SlashChainHeaderChainRegex = new Regex(
            $@"(?<headers>(?:{SlashChainHeaderTokenPattern})(?:[/.-](?:{SlashChainHeaderTokenPattern}))+)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Граница следующей цепочки заголовков внутри блока значений.
        /// </summary>
        /// <remarks>
        /// Останавливает захват значений перед « КОМ./ОФИС» в «АНТРЕСОЛЬ 2/I КОМ./ОФИС 17/Е9Е».
        /// </remarks>
        private static readonly Regex NextSlashHeaderClusterBoundaryRegex = new Regex(
            $@"\s+(?:,\s*)?\s*(?:{SlashChainHeaderTokenPattern})[./]",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Составное значение помещение-комната в slash-цепочке: «XII-8» → пом XII, ком 8.
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>^(?&lt;premise&gt;[\d\w]+)-(?&lt;room&gt;\d+[\w]*)$</c></para>
        /// <para>Применяется, когда подряд идут заголовки ПОМЕЩ и КОМ, а значений меньше, чем заголовков.</para>
        /// </remarks>
        private static readonly Regex PremiseRoomCompoundRegex = new Regex(
            @"^(?<premise>[\d\w]+)-(?<room>\d+[\w]*)$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Токен типа внутри группы <c>headers</c> slash-цепочки.
        /// </summary>
        private static readonly Regex SlashChainHeaderTokenRegex = new Regex(
            SlashChainHeaderTokenPattern,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Секция: «СЕКЦИЯ НОМЕР 2», «СЕКЦИЯ 1».
        /// </summary>
        private static readonly Regex SectionRegex = new Regex(
            $@"(?<!\p{{L}}){IndoorMarkerLexemes.SectionFull}\s*(?:НОМЕР\s*)?(?<v>[\d\w\-]+)?",
            IndoorMarkerRegexFactory.MarkerOptions);

        /// <summary>
        /// Абонентский ящик: «А/Я 165».
        /// </summary>
        private static readonly Regex MailboxRegex =
            IndoorMarkerRegexFactory.MarkerThenOptionalToken(IndoorMarkerLexemes.Mailbox);

        /// <summary>
        /// Литера корпуса: «ЛИТЕРА А», «ЛИТ Б».
        /// </summary>
        private static readonly Regex LiteraRegex =
            IndoorMarkerRegexFactory.MarkerThenRequiredToken(IndoorMarkerLexemes.Litera);

        /// <summary>
        /// Примечание бизнес-центра: скобки и свободный текст «БЦ …».
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>(?:\(\s*(?&lt;v&gt;БЦ[^)]*)\s*\))|(?&lt;v&gt;БЦ\s*[^\d,;()]+…)</c></para>
        /// <list type="bullet">
        /// <item><description>Первая ветка — «(БЦ Речной Вокзал)» в скобках.</description></item>
        /// <item><description>Вторая ветка — «БЦ …» без скобок до цифры/разделителя.</description></item>
        /// </list>
        /// <para>Заполняет <see cref="BuildingUnitLocation.Notes"/>; номер рядом остаётся в <see cref="BuildingUnitLocation.RawCodes"/>
        /// («210 (БЦ Речной Вокзал)» → code:210, note:бц речной вокзал).</para>
        /// </remarks>
        private static readonly Regex BusinessCenterNoteRegex = new Regex(
            @"(?:\(\s*(?<v>БЦ[^)]*)\s*\))|(?<v>БЦ\s*[^\d,;()]+(?:\s+[^\d,;()]+)*)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Диапазон в остаточном токене: «74-82», «1-8».
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>^(?&lt;v&gt;\d+[А-ЯA-Z]?\s*-\s*\d+[А-ЯA-Z]?)$</c></para>
        /// <list type="bullet">
        /// <item><description>Вся строка токена — от-до; допускается буквенный суффикс и пробелы вокруг «-».</description></item>
        /// </list>
        /// <para>Применяется в <see cref="ExtractRanges"/> к элементам <see cref="BuildingUnitLocation.RawCodes"/>;
        /// совпадения переносятся в <see cref="BuildingUnitLocation.Ranges"/> (канон <c>диап:</c>).</para>
        /// </remarks>
        private static readonly Regex RangeRegex = new Regex(
            @"^(?<v>\d+[А-ЯA-Z]?\s*-\s*\d+[А-ЯA-Z]?)$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Нормализация дефиса между цифро-буквенными фрагментами: «74 - 82» → «74-82».
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>(?&lt;=[\d\p{L}])\s*-\s*(?=[\d\p{L}])</c></para>
        /// <list type="bullet">
        /// <item><description><c>(?&lt;=…)(?=…)</c> — дефис между символами слова; пробелы вокруг удаляются.</description></item>
        /// <item><description>Не трогает «1-Й» (между цифрой и буквой без пробелов) и «27-Н».</description></item>
        /// </list>
        /// <para>Выполняется в <see cref="Preprocess"/> до основного разбора.</para>
        /// </remarks>
        private static readonly Regex DashGapRegex = new Regex(
            @"(?<=[\d\p{L}])\s*-\s*(?=[\d\p{L}])",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Заголовок типа в slash-нотации: «ЭТ./», «ПОМЕЩ.», «КОМ./», «ОФИС 1/24».
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>(?:ЭТ|ПОМЕЩ|КОМ)\.\/?\s*|(?:ОФИС)(?:\./|\.\s+|\s+)</c></para>
        /// <list type="bullet">
        /// <item><description><c>(?:ЭТ|ПОМЕЩ|КОМ)\.\/?\s*</c> — сокращения с точкой и опциональным слэшем:
        /// «ЭТ.», «ЭТ./», «ПОМЕЩ.», «КОМ./»; после точки допускается пробел.</description></item>
        /// <item><description><c>(?:ОФИС)(?:\./|\.\s+|\s+)</c> — «ОФИС» без обязательной точки:
        /// «ОФИС./», «ОФИС. », «ОФИС » (перед значением «1/24»).</description></item>
        /// </list>
        /// <para>Примеры: «ЭТ./ПОМЕЩ.», «КОМ./ОФИС ». Не матчит обычный «ЭТ 3» без точки и слэша.</para>
        /// </remarks>
        private static readonly Regex SlashTypeHeaderRegex = new Regex(
            @"(?:ЭТ|ПОМЕЩ|КОМ)\.\/?\s*|(?:ОФИС)(?:\./|\.\s+|\s+)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Извлечение имени типа из совпадения <see cref="SlashTypeHeaderRegex"/>.
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>ЭТ|ПОМЕЩ|КОМ|ОФИС</c> — первый токен типа внутри заголовка.</para>
        /// <para>Используется после <see cref="SlashTypeHeaderRegex"/> для сопоставления
        /// с полями модели (этаж, помещение, комната, офис).</para>
        /// </remarks>
        private static readonly Regex SlashTypeTokenRegex = new Regex(
            @"ЭТ|ПОМЕЩ|КОМ|ОФИС",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Этаж: «ЭТАЖ 4», «ЭТ 3», «ЭТАЖ 1-Й» (после препроцессинга — «ЭТАЖ 1»).
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>(?&lt;!\p{L})(?:ЭТАЖ|ЭТ(?!\p{L}))\s*(?&lt;v&gt;[\d\w\-]+(?:/[\d\w\-]+)*)(?!\p{L})</c></para>
        /// <list type="bullet">
        /// <item><description><c>(?&lt;!\p{L})</c> — граница слова слева (не буква Unicode).</description></item>
        /// <item><description><c>ЭТАЖ|ЭТ(?!\p{L})</c> — полное «ЭТАЖ» или «ЭТ» не как префикс другого слова
        /// (исключает ложные срабатывания внутри длинных токенов).</description></item>
        /// <item><description><c>\s*</c> — пробелы между меткой и значением.</description></item>
        /// <item><description><c>(?&lt;v&gt;[\d\w\-]+(?:/[\d\w\-]+)*)</c> — значение: цифры, буквы, дефис;
        /// допускается составное через «/» (например «2/3»).</description></item>
        /// <item><description><c>(?!\p{L})</c> — граница слова справа.</description></item>
        /// </list>
        /// <para>Примеры: «ЭТАЖ 4» → 4, «ЭТ 3» → 3. Не матчит «ПОДВАЛЬНЫЙ» и slash-формат «ЭТ./».</para>
        /// </remarks>
        private static readonly Regex FloorTypedRegex = new Regex(
            @"(?<!\p{L})(?:ЭТАЖ|ЭТ(?!\p{L}))\s*(?<v>[\d\w\-]+(?:/[\d\w\-]+)*)(?!\p{L})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Помещение: «ПОМЕЩЕНИЕ 2», «ПОМЕЩ. 410», «ПОМ 183», «ПОМЕЩ. 5/1А».
        /// </summary>
        private static readonly Regex PremiseTypedRegex = new Regex(
            $@"(?<!\p{{L}})(?:ПОМЕЩЕНИЯ|ПОМЕЩЕНИЕ|ПОМЕЩ|ПОМ)(?!\p{{L}})\.?\s*(?<v>[^,\s;]+(?:\s*,\s*(?=\d)[^,\s;]+)*)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Комната: «КОМНАТА 136», «КОМ 35», «комн.9»; несколько номеров через «;» или «,».
        /// </summary>
        private static readonly Regex RoomTypedRegex = new Regex(
            $@"(?<!\p{{L}})(?:{IndoorMarkerLexemes.Room})(?!\./)(?!\p{{L}})\.?\s*(?<v>[\d\w\.\-/;]+)(?!\p{{L}})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Краткая запись комнаты: «К. 5-20».
        /// </summary>
        private static readonly Regex ShortRoomTypedRegex = new Regex(
            @"(?<!\p{L})К\.?\s*(?<v>[\d\w\-]+(?:/[\d\w\-]+)*)(?!\p{L})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Офис: «ОФИС 104», «ОФ 79», «офис 613-11».
        /// </summary>
        private static readonly Regex OfficeTypedRegex = new Regex(
            $@"(?<!\p{{L}})(?:{IndoorMarkerLexemes.Office})\.?\s*(?<v>[\d\w\-]+(?:/[\d\w\-]+)*)(?!\p{{L}})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Рабочее место: «РАБ.М.1», «РАБ М 2».
        /// </summary>
        private static readonly Regex WorkplaceTypedRegex = new Regex(
            $@"(?<!\p{{L}}){IndoorMarkerLexemes.Workplace}\.?\s*(?<v>[\d\w\-]+)(?!\p{{L}})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Часть помещения: «ч.п. 666», «Ч П 12».
        /// </summary>
        private static readonly Regex PartTypedRegex = new Regex(
            $@"(?<!\p{{L}}){IndoorMarkerLexemes.Part}\.?\s*(?<v>[\d\w\-]+)(?!\p{{L}})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Определение early-маркера (до <see cref="PreprocessIndoorRemainder"/>).
        /// </summary>
        private sealed class EarlyMarkerDefinition
        {
            public Regex Regex { get; set; }

            public Func<BuildingUnitLocation, IList<string>> Target { get; set; }

            public bool SplitValues { get; set; }

            public bool ExpandNumericRanges { get; set; }
        }

        /// <summary>
        /// Early-маркеры: квартира, кабинет, подъезд, проезд, владение, склад, блок, секция, а/я, литера.
        /// </summary>
        /// <remarks>
        /// «БЛОК-СЕКЦИЯ» обрабатывается отдельно между складом и блоком.
        /// </remarks>
        private static readonly EarlyMarkerDefinition[] EarlyMarkersBeforeBlockSection =
        {
            new EarlyMarkerDefinition
            {
                Regex = ApartmentRegex,
                Target = loc => loc.Apartments,
                SplitValues = true,
                ExpandNumericRanges = true
            },
            new EarlyMarkerDefinition
            {
                Regex = CabinetRegex,
                Target = loc => loc.Cabinets,
                SplitValues = true,
                ExpandNumericRanges = true
            },
            new EarlyMarkerDefinition { Regex = EntranceRegex, Target = loc => loc.Entrances },
            new EarlyMarkerDefinition { Regex = PassageRegex, Target = loc => loc.Passages },
            new EarlyMarkerDefinition { Regex = HoldingRegex, Target = loc => loc.Holdings },
            new EarlyMarkerDefinition { Regex = StorageRegex, Target = loc => loc.Storages },
        };

        private static readonly EarlyMarkerDefinition[] EarlyMarkersAfterBlockSection =
        {
            new EarlyMarkerDefinition { Regex = BlockRegex, Target = loc => loc.Blocks },
            new EarlyMarkerDefinition { Regex = SectionRegex, Target = loc => loc.Sections },
            new EarlyMarkerDefinition { Regex = MailboxRegex, Target = loc => loc.Mailboxes },
            new EarlyMarkerDefinition { Regex = LiteraRegex, Target = loc => loc.Literas },
        };

        /// <summary>
        /// Порядок применения typed-regex для indoor-сегментов (этаж → пом → ком → оф → …).
        /// </summary>
        private sealed class TypedPatternDefinition
        {
            public Regex Regex { get; set; }

            public Action<BuildingUnitLocation, string> Apply { get; set; }
        }

        private static readonly TypedPatternDefinition[] TypedPatterns =
        {
            new TypedPatternDefinition { Regex = FloorTypedRegex, Apply = (loc, v) => AddSingleValue(loc.Floors, v, expandNumericRanges: true) },
            new TypedPatternDefinition { Regex = PremiseTypedRegex, Apply = (loc, v) => AddMultiValue(loc.Premises, v, expandNumericRanges: true) },
            new TypedPatternDefinition { Regex = RoomTypedRegex, Apply = (loc, v) => AddMultiValue(loc.Rooms, v, expandNumericRanges: true) },
            new TypedPatternDefinition { Regex = ShortRoomTypedRegex, Apply = (loc, v) => AddSingleValue(loc.Rooms, v) },
            new TypedPatternDefinition { Regex = OfficeTypedRegex, Apply = (loc, v) => AddSingleValue(loc.Offices, v, expandNumericRanges: true) },
            new TypedPatternDefinition { Regex = WorkplaceTypedRegex, Apply = (loc, v) => AddSingleValue(loc.Workplaces, v, expandNumericRanges: true) },
            new TypedPatternDefinition { Regex = PartTypedRegex, Apply = (loc, v) => AddSingleValue(loc.Parts, v, expandNumericRanges: true) },
        };

        /// <summary>
        /// Текстовые примечания к локации (не номера).
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>(?&lt;!\p{L})(?:ВХОД\s+С\s+ТОРЦА|ВХОД\s+С\s+ФАСАДА)(?!\p{L})</c></para>
        /// <para>Фиксированные фразы «ВХОД С ТОРЦА», «ВХОД С ФАСАДА» целиком; попадают в <see cref="BuildingUnitLocation.Notes"/>,
        /// а не в офис/этаж. Расширяется по мере появления новых шаблонов примечаний.</para>
        /// </remarks>
        private static readonly Regex NoteRegex = new Regex(
            @"(?<!\p{L})(?:ВХОД\s+С\s+ТОРЦА|ВХОД\s+С\s+ФАСАДА)(?!\p{L})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Удаление порядкового суффикса после дефиса: «1-Й» → «1», «4-Я» → «4».
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>(\d+)\s*-\s*([ЙЯ])\b</c></para>
        /// <list type="bullet">
        /// <item><description><c>(\d+)</c> — число (группа замены $1).</description></item>
        /// <item><description><c>\s*-\s*</c> — дефис с пробелами.</description></item>
        /// <item><description><c>([ЙЯ])\b</c> — только «Й» или «Я» как окончание порядкового («-Й», «-Я»);
        /// не трогает «5-20», «27-Н».</description></item>
        /// </list>
        /// </remarks>
        private static readonly Regex OrdinalSuffixStripRegex = new Regex(
            @"(\d+)\s*-\s*([ЙЯ])\b",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Удаление одинокого слова «ЭТАЖ» без следующего номера.
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>\bЭТАЖ\b(?!\s+[\d\w])</c></para>
        /// <list type="bullet">
        /// <item><description><c>\bЭТАЖ\b</c> — целое слово «ЭТАЖ».</description></item>
        /// <item><description><c>(?!\s+[\d\w])</c> — negative lookahead: после «ЭТАЖ» нет пробела и цифро-буквенного
        /// значения. Удаляет «ЭТАЖ ЦОКОЛЬНЫЙ» (остаётся «цокольный»), не трогает «ЭТАЖ 4».</description></item>
        /// </list>
        /// </remarks>
        private static readonly Regex BareFloorWordRegex = new Regex(
            @"\bЭТАЖ\b(?!\s+[\d\w])",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Проверка, что остаточный токен похож на код/номер, а не на произвольный текст.
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>^[\d\w\-./]+$</c> — вся строка из цифр, букв, дефиса, точки, слэша.</para>
        /// <para>Примеры: «659318», «27-Н», «41X1Д», «305». Не матчит фразы с пробелами — они уходят в
        /// <see cref="BuildingUnitLocation.Unparsed"/>.</para>
        /// </remarks>
        private static readonly Regex RawCodeTokenRegex = new Regex(
            @"^[\d\w\-./]+$",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);
    }
}
