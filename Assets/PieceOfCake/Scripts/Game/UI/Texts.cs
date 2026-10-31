using PieceOfCake.Core;

namespace PieceOfCake.Game.UI
{
    /// <summary>
    /// Every user-facing string (Japanese). Kept in one place so wording can be reviewed and,
    /// later, localised without touching gameplay or layout code. Stage names and sign texts
    /// live in the level data instead.
    /// </summary>
    public static class Texts
    {
        public const string TitleSmall = "PIECE OF CAKE";
        public const string Title = "ピース・オブ・ケーキ";
        public const string Tagline = "朝飯前？ とんでもない。";
        public const string Play = "あそぶ";
        public const string HowCutTitle = "切る";
        public const string HowCut = "ケーキの上で使うと、前のピースを たてに切る。細くて高いピースは パタンと倒れて 橋になる";
        public const string HowEatTitle = "食べる";
        public const string HowEat = "目の前か足もとのケーキを食べる。いちご＝ジャンプ力アップ、チョコ＝ちからもち";
        public const string HowPushTitle = "おす";
        public const string HowPush = "小さいケーキは押して動かせる。細くて高いケーキは押すと倒れる";
        public const string HowGoalTitle = "ゴール";
        public const string HowGoal = "リボンつきの「ごちゅうもんのケーキ」を、おきゃくさんに届けよう";
        public const string KeyHint = "キーボード：←→ 移動 ／ Space・↑ ジャンプ ／ X 切る ／ C 食べる ／ R やり直し ／ Esc メニュー\nゲームパッド：スティック 移動 ／ A ジャンプ ／ X 切る ／ Y 食べる ／ Start メニュー";
        public const string RotateHint = "スマホは 横向きが おすすめ";

        public const string SelectTitle = "ステージえらび";
        public const string BackToTitle = "タイトルへ";

        public const string PauseTitle = "ひとやすみ";
        public const string Resume = "つづける";
        public const string FromCheckpoint = "チェックポイントから";
        public const string RestartStage = "ステージを最初から";
        public const string ToStageSelect = "ステージえらび";
        public const string SoundOn = "おと：ON";
        public const string SoundOff = "おと：OFF";

        public const string Retry = "やり直し";
        public const string Menu = "メニュー";
        public const string Carrying = "もってる！";
        public const string Searching = "さがそう";
        public const string Strawberry = "いちご";
        public const string Chocolate = "チョコ";

        public const string Delivered = "おとどけ かんりょう！";
        public const string AllDelivered = "ぜんぶ とどけた！ ありがとう！";
        public const string NextStage = "つぎのステージへ";
        public const string ToSelectAfterLast = "ステージえらびへ";
        public const string Again = "もういちど";
        public const string PerfectNote = "パーフェクト！ むだなく とどけた";
        public const string ImproveNote = "食べたり 落としたりを へらすと ★3つ";
        public const string FinalNote = "朝飯前…じゃ なかったね。おつかれさま！";
        public const string OrderLostToast = "ごちゅうもんの ケーキが おちちゃった…";
        public const string CustomerWaitingBubble = "ケーキ まだかな？";

        public const string ErrorTitle = "データを よみこめませんでした";

        public const string JumpButton = "ジャンプ";
        public const string CutButton = "切る";
        public const string EatButton = "食べる";

        private static readonly string[] YummyLines = { "もぐもぐ", "おいしい！", "あまーい" };

        public static string StageNumber(int index) => $"STAGE {index + 1}";
        public static string StageHeading(int index, string name) => $"{index + 1}. {name}";
        public static string Loss(int loss) => $"ロス {loss}";
        public static string WasteSummary(int loss, int par) => $"もったいない：{loss}こ（目標 {par}こ いか）";

        public static string TimeAndMisses(double seconds, int misses)
        {
            int s = (int)seconds;
            return $"タイム {s / 60}:{s % 60:00}　ミス {misses}";
        }

        public static string Stars(int stars)
        {
            var filled = new string('★', stars);
            var empty = new string('★', Scoring.MaxStars - stars);
            return filled + "<color=#E8D9CC>" + empty + "</color>";
        }

        /// <summary>Speech for a chef message. <paramref name="variant"/> picks among alternatives.</summary>
        public static string Speech(PlayerMessage message, int variant)
        {
            switch (message)
            {
                case PlayerMessage.CannotCutHere: return "ここでは 切れない";
                case PlayerMessage.TooHeavy: return "おもくて うごかない…";
                case PlayerMessage.ChocolateTooHard: return "カチン！ チョコは かたくて 切れない";
                case PlayerMessage.OrderIsNotFood: return "これは ごちゅうもんの しな！";
                case PlayerMessage.StrawberryPower: return "いちごパワー！ ジャンプ力アップ";
                case PlayerMessage.ChocolatePower: return "チョコパワー！ ちからもち";
                case PlayerMessage.Yummy: return YummyLines[(variant & int.MaxValue) % YummyLines.Length];
                case PlayerMessage.OrderPickedUp: return "ごちゅうもん ゲット！";
                case PlayerMessage.CheckpointReached: return "チェックポイント！";
                case PlayerMessage.CustomerWaiting: return "ごちゅうもんの ケーキを まってるよ";
                default: return string.Empty;
            }
        }
    }
}
