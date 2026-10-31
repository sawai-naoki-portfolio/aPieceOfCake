using System;
using PieceOfCake.Core;
using PieceOfCake.Game.Levels;
using PieceOfCake.Game.Persistence;
using UnityEngine.UIElements;

namespace PieceOfCake.Game.UI
{
    public sealed class TitleScreen : ScreenView
    {
        private readonly Button play;

        public TitleScreen(VisualElement root, bool touchDevice) : base(root, "title-screen")
        {
            Element.Required<Label>("title-small").text = Texts.TitleSmall;
            Element.Required<Label>("title-big").text = Texts.Title;
            Element.Required<Label>("tagline").text = Texts.Tagline;
            var howTo = Element.Required<VisualElement>("how-to");
            AddRow(howTo, Texts.HowCutTitle, Texts.HowCut);
            AddRow(howTo, Texts.HowEatTitle, Texts.HowEat);
            AddRow(howTo, Texts.HowPushTitle, Texts.HowPush);
            AddRow(howTo, Texts.HowGoalTitle, Texts.HowGoal);
            var rotate = Element.Required<Label>("rotate-hint");
            rotate.text = Texts.RotateHint;
            rotate.SetVisible(touchDevice);
            var keys = Element.Required<Label>("key-hint");
            keys.text = Texts.KeyHint;
            keys.SetVisible(!touchDevice);
            play = Element.Button("play-button", Texts.Play, () => PlayPressed?.Invoke());
        }

        public event Action PlayPressed;

        protected override Focusable DefaultFocus => play;

        private static void AddRow(VisualElement parent, string key, string text)
        {
            var row = new VisualElement();
            row.AddToClassList("how-row");
            var k = new Label(key);
            k.AddToClassList("how-key");
            var t = new Label(text);
            t.AddToClassList("how-text");
            row.Add(k);
            row.Add(t);
            parent.Add(row);
        }
    }

    public sealed class StageSelectScreen : ScreenView
    {
        private readonly VisualElement list;
        private readonly Button back;

        public StageSelectScreen(VisualElement root) : base(root, "select-screen")
        {
            Element.Required<Label>("select-title").text = Texts.SelectTitle;
            list = Element.Required<VisualElement>("stage-list");
            back = Element.Button("select-back-button", Texts.BackToTitle, () => BackPressed?.Invoke());
        }

        public event Action<int> StageChosen;
        public event Action BackPressed;

        protected override Focusable DefaultFocus => list.childCount > 0 ? (Focusable)list[0] : back;

        public void Populate(LevelCatalog levels, IProgressStore progress)
        {
            list.Clear();
            for (int i = 0; i < levels.Count; i++)
            {
                int index = i;
                var definition = levels.Stages[i].Definition;
                var card = new Button(() => StageChosen?.Invoke(index));
                card.AddToClassList("stage-card");
                card.Add(Styled(new Label(Texts.StageNumber(i)), "stage-number"));
                card.Add(Styled(new Label(definition.Name), "stage-name"));
                card.Add(Styled(new Label(Texts.Stars(progress.BestStars(definition.Id))), "stage-stars"));
                list.Add(card);
            }
        }

        private static Label Styled(Label label, string cls)
        {
            label.AddToClassList(cls);
            label.pickingMode = PickingMode.Ignore;
            return label;
        }
    }

    public sealed class PauseScreen : ScreenView
    {
        private readonly Button resume;
        private readonly Button sound;

        public PauseScreen(VisualElement root) : base(root, "pause-screen")
        {
            Element.Required<Label>("pause-title").text = Texts.PauseTitle;
            resume = Element.Button("resume-button", Texts.Resume, () => ResumePressed?.Invoke());
            Element.Button("checkpoint-button", Texts.FromCheckpoint, () => CheckpointPressed?.Invoke());
            Element.Button("restart-button", Texts.RestartStage, () => RestartPressed?.Invoke());
            Element.Button("pause-select-button", Texts.ToStageSelect, () => SelectPressed?.Invoke());
            sound = Element.Button("sound-button", Texts.SoundOn, () => SoundPressed?.Invoke());
        }

        public event Action ResumePressed;
        public event Action CheckpointPressed;
        public event Action RestartPressed;
        public event Action SelectPressed;
        public event Action SoundPressed;

        protected override Focusable DefaultFocus => resume;

        public void SetMuted(bool muted) => sound.text = muted ? Texts.SoundOff : Texts.SoundOn;
    }

    public sealed class ResultScreen : ScreenView
    {
        private readonly Label title;
        private readonly Label stars;
        private readonly Label loss;
        private readonly Label time;
        private readonly Label note;
        private readonly Button next;

        public ResultScreen(VisualElement root) : base(root, "result-screen")
        {
            title = Element.Required<Label>("result-title");
            stars = Element.Required<Label>("result-stars");
            loss = Element.Required<Label>("result-loss");
            time = Element.Required<Label>("result-time");
            note = Element.Required<Label>("result-note");
            next = Element.Button("next-button", Texts.NextStage, () => NextPressed?.Invoke());
            Element.Button("again-button", Texts.Again, () => AgainPressed?.Invoke());
            Element.Button("result-select-button", Texts.ToStageSelect, () => SelectPressed?.Invoke());
        }

        public event Action NextPressed;
        public event Action AgainPressed;
        public event Action SelectPressed;

        protected override Focusable DefaultFocus => next;

        public void Show(StageResult result, bool lastStage)
        {
            title.text = lastStage ? Texts.AllDelivered : Texts.Delivered;
            stars.text = Texts.Stars(result.Stars);
            loss.text = Texts.WasteSummary(result.Loss, result.Par);
            time.text = Texts.TimeAndMisses(result.Time, result.Misses);
            note.text = lastStage ? Texts.FinalNote : result.Stars < Scoring.MaxStars ? Texts.ImproveNote : Texts.PerfectNote;
            next.text = lastStage ? Texts.ToSelectAfterLast : Texts.NextStage;
        }
    }

    public sealed class ErrorScreen : ScreenView
    {
        private readonly Label message;

        public ErrorScreen(VisualElement root) : base(root, "error-screen")
        {
            Element.Required<Label>("error-title").text = Texts.ErrorTitle;
            message = Element.Required<Label>("error-message");
        }

        protected override Focusable DefaultFocus => null;

        public void Show(string text)
        {
            message.text = text;
            SetVisible(true);
        }
    }
}
