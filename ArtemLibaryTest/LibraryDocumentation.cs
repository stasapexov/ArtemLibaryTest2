namespace ArtemLibaryTest
{
    /// <summary>
    /// Подсказка для Visual Studio Object Browser: README.md и readme.txt не отображаются там как файлы,
    /// потому что обозреватель объектов показывает только публичные типы, члены и XML-комментарии сборки.
    /// Полные README-файлы упаковываются в NuGet-пакет, а краткие подсказки доступны в XML-документации типов библиотеки.
    /// </summary>
    public static class LibraryDocumentation
    {
        /// <summary>
        /// Имя основного файла документации, который попадает в корень NuGet-пакета и показывается на странице пакета.
        /// </summary>
        public const string PackageReadmeFile = "README.md";

        /// <summary>
        /// Имя дополнительного файла с подробной справкой по DbHelper внутри NuGet-пакета.
        /// </summary>
        public const string DbHelperReadmeFile = "Dbhelper/readme.txt";

        /// <summary>
        /// Короткое напоминание: для подсказок в Object Browser используйте XML-комментарии, а не markdown/txt-файлы.
        /// </summary>
        public const string ObjectBrowserHint = "Object Browser показывает XML documentation comments, а README-файлы смотрите в NuGet package/репозитории.";

        /// <summary>
        /// Минимальный набор namespace для быстрого старта с библиотекой.
        /// </summary>
        public const string QuickStartNamespaces = "using ArtemLibaryTest.Core; using ArtemLibaryTest.Models; using ArtemLibaryTest.QuickStart;";
    }
}
