namespace PieceOfCake.EditorTools
{
    /// <summary>Asset locations used by the editor tools (single source of truth).</summary>
    internal static class ProjectPaths
    {
        public const string Root = "Assets/PieceOfCake";
        public const string Models = Root + "/Art/Models";
        public const string Materials = Root + "/Art/Materials";
        public const string Config = Root + "/Config";
        public const string GameArt = Config + "/GameArt.asset";
        public const string Levels = Root + "/Data/Levels/levels.json";
        public const string Ui = Root + "/UI";
        public const string UiDocument = Ui + "/Main.uxml";
        public const string PanelSettings = Ui + "/PanelSettings.asset";
        public const string RuntimeTheme = Ui + "/RuntimeTheme.tss";
        public const string Scenes = Root + "/Scenes";
        public const string MainScene = Scenes + "/Main.unity";
        public const string VolumeProfile = "Assets/Settings/SampleSceneProfile.asset";
        public const string MenuRoot = "Tools/Piece of Cake/";
    }
}
