using VTBL.AddressNormalizer.Abstractions.BuildingUnit;
using System;
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
        /// <remarks>
        /// <para>Паттерн: <c>(?&lt;!\p{L})(?:КВАРТИРА|КВ)\.?\s*(?&lt;v&gt;\d[\d\w\-/]*(?:\s*[,;]\s*\d[\d\w\-/]*)*)</c></para>
        /// <list type="bullet">
        /// <item><description><c>\d[\d\w\-/]*</c> — значение обязано начинаться с цифры, чтобы «КВ. 10, КОМ. 3»
        /// не захватывало «КОМ» как часть квартиры.</description></item>
        /// <item><description><c>(?:\s*[,;]\s*\d[\d\w\-/]*)*</c> — списки квартир через «,» или «;».</description></item>
        /// </list>
        /// <para>Заполняет <see cref="BuildingUnitLocation.Apartments"/>; при <c>splitValues: true</c> списки дробятся.</para>
        /// </remarks>
        private static readonly Regex ApartmentRegex = new Regex(
            @"(?<!\p{L})(?:КВАРТИРА|КВ)\.?\s*(?<v>\d[\d\w\-/]*(?:\s*[,;]\s*\d[\d\w\-/]*)*)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Кабинет: «КАБИНЕТ 69», списки кабинетов.
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>(?&lt;!\p{L})(?:КАБИНЕТ|КАБ)\.?\s*(?&lt;v&gt;\d[\d\w\-/]*(?:\s*[,;]\s*\d[\d\w\-/]*)*)</c></para>
        /// <list type="bullet">
        /// <item><description>Аналогично <see cref="ApartmentRegex"/>: значение с цифры, списки через «,»/«;».</description></item>
        /// </list>
        /// <para>Заполняет <see cref="BuildingUnitLocation.Cabinets"/>. Не пересекается с slash-цепочкой «ПОМЕЩ./КАБ.» —
        /// та обрабатывается раньше в <see cref="ExtractSlashChains"/>.</para>
        /// </remarks>
        private static readonly Regex CabinetRegex = new Regex(
            @"(?<!\p{L})(?:КАБИНЕТ|КАБ)\.?\s*(?<v>\d[\d\w\-/]*(?:\s*[,;]\s*\d[\d\w\-/]*)*)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Подъезд: «ПОДЪЕЗД 5», «ПОДЪЕЗД/ЭТ 2».
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>(?&lt;!\p{L})ПОДЪЕЗД(?:/ЭТАЖ|/ЭТ)?\s*(?&lt;v&gt;[\d\w\-/]+)?</c></para>
        /// <list type="bullet">
        /// <item><description><c>(?:/ЭТАЖ|/ЭТ)?</c> — опциональный хвост slash-нотации после «ПОДЪЕЗД».</description></item>
        /// <item><description><c>(?&lt;v&gt;[\d\w\-/]+)?</c> — номер подъезда опционален.</description></item>
        /// </list>
        /// <para>Заполняет <see cref="BuildingUnitLocation.Entrances"/>.</para>
        /// </remarks>
        private static readonly Regex EntranceRegex = new Regex(
            @"(?<!\p{L})ПОДЪЕЗД(?:/ЭТАЖ|/ЭТ)?\s*(?<v>[\d\w\-/]+)?",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Составной маркер «БЛОК-СЕКЦИЯ» с одним номером.
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>(?&lt;!\p{L})БЛОК-СЕКЦИЯ\s*(?&lt;v&gt;[\d\w\-]+)?</c></para>
        /// <list type="bullet">
        /// <item><description>Обрабатывается в <see cref="ExtractBlockSection"/> до <see cref="BlockRegex"/>,
        /// чтобы не разбить на отдельные «БЛОК» и «СЕКЦИЯ».</description></item>
        /// <item><description>Значение дублируется в <see cref="BuildingUnitLocation.Blocks"/> и <see cref="BuildingUnitLocation.Sections"/>.</description></item>
        /// </list>
        /// <para>Пример: «БЛОК-СЕКЦИЯ 1» → блок:1, секц:1.</para>
        /// </remarks>
        private static readonly Regex BlockSectionRegex = new Regex(
            @"(?<!\p{L})БЛОК-СЕКЦИЯ\s*(?<v>[\d\w\-]+)?",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Блок: «БЛОК Е», «БЛОК 1».
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>(?&lt;!\p{L})БЛОК\s*(?&lt;v&gt;[\d\w\-]+)?</c></para>
        /// <list type="bullet">
        /// <item><description><c>БЛОК</c> без дефиса — отдельно от «БЛОК-СЕКЦИЯ» (та извлекается раньше).</description></item>
        /// </list>
        /// <para>Заполняет <see cref="BuildingUnitLocation.Blocks"/>.</para>
        /// </remarks>
        private static readonly Regex BlockRegex = new Regex(
            @"(?<!\p{L})БЛОК\s*(?<v>[\d\w\-]+)?",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

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
        /// <remarks>
        /// <para>Паттерн: перечисление аббревиатур этаж/пом/ком/оф/каб/раб.м (с опциональными точками).</para>
        /// <para>Используется в <see cref="ParseSlashHeaders"/> → <see cref="NormalizeSlashHeader"/> →
        /// <see cref="ApplySlashChainValue"/>.</para>
        /// </remarks>
        private static readonly Regex SlashChainHeaderTokenRegex = new Regex(
            @"ЭТАЖ|ЭТ\.?|ПОМЕЩЕНИЯ|ПОМЕЩЕНИЕ|ПОМЕЩ\.?|ПОМ\.?|КОМНАТА|КОМН\.?|КОМ\.?|ОФИС|ОФ\.?|КАБИНЕТ|КАБ\.?|РАБ\.?\s*М(?:ЕСТО)?",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Секция: «СЕКЦИЯ НОМЕР 2», «СЕКЦИЯ 1».
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>(?&lt;!\p{L})СЕКЦИЯ\s*(?:НОМЕР\s*)?(?&lt;v&gt;[\d\w\-]+)?</c></para>
        /// <list type="bullet">
        /// <item><description><c>(?:НОМЕР\s*)?</c> — слово «НОМЕР» между меткой и значением опционально.</description></item>
        /// </list>
        /// <para>Заполняет <see cref="BuildingUnitLocation.Sections"/>. «БЛОК-СЕКЦИЯ» обрабатывается отдельно.</para>
        /// </remarks>
        private static readonly Regex SectionRegex = new Regex(
            @"(?<!\p{L})СЕКЦИЯ\s*(?:НОМЕР\s*)?(?<v>[\d\w\-]+)?",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Абонентский ящик: «А/Я 165».
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>(?&lt;!\p{L})А/Я\s*(?&lt;v&gt;[\d\w\-]+)?</c></para>
        /// <list type="bullet">
        /// <item><description>Слэш в «А/Я» обязателен; «а\я» после <see cref="Preprocess"/> тоже матчится.</description></item>
        /// </list>
        /// <para>Заполняет <see cref="BuildingUnitLocation.Mailboxes"/>.</para>
        /// </remarks>
        private static readonly Regex MailboxRegex = new Regex(
            @"(?<!\p{L})А/Я\s*(?<v>[\d\w\-]+)?",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Литера корпуса: «ЛИТЕРА А», «ЛИТ Б».
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>(?&lt;!\p{L})ЛИТЕ?РА?\s*(?&lt;v&gt;[\d\w\-]+)</c></para>
        /// <list type="bullet">
        /// <item><description><c>ЛИТЕ?РА?</c> — «ЛИТЕРА», «ЛИТЕР», «ЛИТ»; значение обязательно.</description></item>
        /// </list>
        /// <para>Заполняет <see cref="BuildingUnitLocation.Literas"/>.</para>
        /// </remarks>
        private static readonly Regex LiteraRegex = new Regex(
            @"(?<!\p{L})ЛИТЕ?РА?\s*(?<v>[\d\w\-]+)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

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
        /// <remarks>
        /// <para>Паттерн: <c>(?&lt;!\p{L})(?:ПОМЕЩЕНИЕ|ПОМЕЩ|ПОМ)(?!\p{L})\.?\s*(?&lt;v&gt;[^,\s;]+)</c></para>
        /// <list type="bullet">
        /// <item><description><c>ПОМЕЩЕНИЕ|ПОМЕЩ|ПОМ</c> — полная и сокращённые формы; <c>(?!\p{L})</c> после «ПОМ»
        /// не даёт съесть «ПОМЕЩ» как «ПОМ» + «ЕЩ».</description></item>
        /// <item><description><c>\.?</c> — опциональная точка после сокращения («ПОМЕЩ.»).</description></item>
        /// <item><description><c>(?&lt;v&gt;[^,\s;]+(?:\s*,\s*(?=\d)[^,\s;]+)*)</c> — значение до пробела или «;»;
        /// список через «,» только если следующий фрагмент начинается с цифры («35,38»), чтобы не съесть «35, Ч.П. 1».</description></item>
        /// </list>
        /// </remarks>
        private static readonly Regex PremiseTypedRegex = new Regex(
            @"(?<!\p{L})(?:ПОМЕЩЕНИЯ|ПОМЕЩЕНИЕ|ПОМЕЩ|ПОМ)(?!\p{L})\.?\s*(?<v>[^,\s;]+(?:\s*,\s*(?=\d)[^,\s;]+)*)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Комната: «КОМНАТА 136», «КОМ 35», «комн.9»; несколько номеров через «;» или «,».
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>(?&lt;!\p{L})(?:КОМНАТА|КОМН|КОМ)(?!\./)(?!\p{L})\.?\s*(?&lt;v&gt;[\d\w\.\-/;]+)(?!\p{L})</c></para>
        /// <list type="bullet">
        /// <item><description><c>(?!\./)</c> — не матчить «КОМ./» из slash-формата
        /// («ЭТ./ПОМЕЩ. 0/II КОМ./ОФИС 1/24» обрабатывается отдельно).</description></item>
        /// <item><description><c>(?&lt;v&gt;[\d\w\.\-/;]+)</c> — номер комнаты с точкой, слэшем, дефисом, «;»
        /// для списков («1;2»).</description></item>
        /// </list>
        /// <para>Примеры: «КОМ 35» → 35, «КОМ 1;2» → 1 и 2 (через <see cref="AddMultiValue"/>).</para>
        /// </remarks>
        private static readonly Regex RoomTypedRegex = new Regex(
            @"(?<!\p{L})(?:КОМНАТА|КОМН|КОМ)(?!\./)(?!\p{L})\.?\s*(?<v>[\d\w\.\-/;]+)(?!\p{L})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Краткая запись комнаты: «К. 5-20».
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>(?&lt;!\p{L})К\.?\s*(?&lt;v&gt;[\d\w\-]+(?:/[\d\w\-]+)*)(?!\p{L})</c></para>
        /// <para>Отдельно от <see cref="RoomTypedRegex"/>, чтобы не путать «К.» с другими токенами
        /// и не пересекаться с «КОМ».</para>
        /// </remarks>
        private static readonly Regex ShortRoomTypedRegex = new Regex(
            @"(?<!\p{L})К\.?\s*(?<v>[\d\w\-]+(?:/[\d\w\-]+)*)(?!\p{L})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Офис: «ОФИС 104», «ОФ 79», «офис 613-11».
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>(?&lt;!\p{L})(?:ОФИС|ОФ)\.?\s*(?&lt;v&gt;[\d\w\-]+(?:/[\d\w\-]+)*)(?!\p{L})</c></para>
        /// <para>Значение — буквенно-цифровое с дефисом и опциональным «/»; точка после «ОФ.» опциональна.</para>
        /// </remarks>
        private static readonly Regex OfficeTypedRegex = new Regex(
            @"(?<!\p{L})(?:ОФИС|ОФ)\.?\s*(?<v>[\d\w\-]+(?:/[\d\w\-]+)*)(?!\p{L})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Рабочее место: «РАБ.М.1», «РАБ М 2».
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>(?&lt;!\p{L})РАБ\.?\s*М\.?\s*(?&lt;v&gt;[\d\w\-]+)(?!\p{L})</c></para>
        /// <para>Точки после «РАБ» и «М» опциональны; между частями допускаются пробелы.</para>
        /// </remarks>
        private static readonly Regex WorkplaceTypedRegex = new Regex(
            @"(?<!\p{L})РАБ\.?\s*М\.?\s*(?<v>[\d\w\-]+)(?!\p{L})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Часть помещения: «ч.п. 666», «Ч П 12».
        /// </summary>
        /// <remarks>
        /// <para>Паттерн: <c>(?&lt;!\p{L})Ч\.?\s*П\.?\s*(?&lt;v&gt;[\d\w\-]+)(?!\p{L})</c></para>
        /// <para>«Ч» и «П» — с опциональными точками и пробелами между ними.</para>
        /// </remarks>
        private static readonly Regex PartTypedRegex = new Regex(
            @"(?<!\p{L})Ч\.?\s*П\.?\s*(?<v>[\d\w\-]+)(?!\p{L})",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
        /// <para>Паттерн: <c>(?&lt;!\p{L})(?:ВХОД\s+С\s+ТОРЦА)(?!\p{L})</c></para>
        /// <para>Фиксированная фраза «ВХОД С ТОРЦА» целиком; попадает в <see cref="BuildingUnitLocation.Notes"/>,
        /// а не в офис/этаж. Расширяется по мере появления новых шаблонов примечаний.</para>
        /// </remarks>
        private static readonly Regex NoteRegex = new Regex(
            @"(?<!\p{L})(?:ВХОД\s+С\s+ТОРЦА)(?!\p{L})",
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
