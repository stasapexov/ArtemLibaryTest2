namespace ArtemLibaryTest.QuickStart
{
    public class AuthUiOptions
    {
        public string AppTitle { get; set; } = "Приложение";
        public string MainWelcomeText { get; set; } = "Вы вошли в систему";
        public IMenuProvider? MenuProvider { get; set; }
        public bool IsSettingsVisible { get; set; }
    }
}
