using System;
using PieceOfCake.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace PieceOfCake.Game.UI
{
    /// <summary>Stage name, order status, waste counter, power-up timers, stage banner and toasts.</summary>
    public sealed class HudView
    {
        private const float BannerSeconds = 2.4f;
        private const float ToastSeconds = 2.2f;

        private readonly VisualElement element;
        private readonly Label stageTitle;
        private readonly VisualElement orderIcon;
        private readonly Label orderLabel;
        private readonly Label lossLabel;
        private readonly VisualElement strawberry;
        private readonly VisualElement strawberryFill;
        private readonly VisualElement chocolate;
        private readonly VisualElement chocolateFill;
        private readonly VisualElement banner;
        private readonly Label bannerStage;
        private readonly Label bannerName;
        private readonly Label toast;
        private float bannerTimer;
        private float toastTimer;

        public HudView(VisualElement root)
        {
            element = root.Required<VisualElement>("hud");
            stageTitle = element.Required<Label>("stage-title");
            orderIcon = element.Required<VisualElement>("order-icon");
            orderLabel = element.Required<Label>("order-label");
            lossLabel = element.Required<Label>("loss-label");
            strawberry = element.Required<VisualElement>("power-strawberry");
            strawberryFill = element.Required<VisualElement>("power-strawberry-fill");
            chocolate = element.Required<VisualElement>("power-chocolate");
            chocolateFill = element.Required<VisualElement>("power-chocolate-fill");
            element.Required<Label>("power-strawberry-label").text = Texts.Strawberry;
            element.Required<Label>("power-chocolate-label").text = Texts.Chocolate;
            banner = element.Required<VisualElement>("banner");
            bannerStage = element.Required<Label>("banner-stage");
            bannerName = element.Required<Label>("banner-name");
            toast = element.Required<Label>("toast");
            element.Button("retry-button", Texts.Retry, () => RetryPressed?.Invoke());
            element.Button("menu-button", Texts.Menu, () => MenuPressed?.Invoke());
        }

        public event Action RetryPressed;
        public event Action MenuPressed;

        public void SetVisible(bool visible) => element.SetVisible(visible);

        public void BeginStage(int index, string name)
        {
            stageTitle.text = Texts.StageHeading(index, name);
            bannerStage.text = Texts.StageNumber(index);
            bannerName.text = name;
            bannerTimer = BannerSeconds;
            toastTimer = 0f;
        }

        public void ShowToast(string text)
        {
            toast.text = text;
            toastTimer = ToastSeconds;
        }

        public void Render(StageSimulation sim, float deltaTime)
        {
            var p = sim.Player;
            orderIcon.EnableInClassList("empty", !p.IsCarryingOrder);
            orderLabel.text = p.IsCarryingOrder ? Texts.Carrying : Texts.Searching;
            lossLabel.text = Texts.Loss(sim.Stats.Loss);
            ShowPower(strawberry, strawberryFill, p.PowerTimer);
            ShowPower(chocolate, chocolateFill, p.StrengthTimer);

            bannerTimer = Mathf.Max(0f, bannerTimer - deltaTime);
            banner.SetVisible(bannerTimer > 0f);
            banner.style.opacity = Mathf.Clamp01(bannerTimer);

            toastTimer = Mathf.Max(0f, toastTimer - deltaTime);
            toast.SetVisible(toastTimer > 0f);
            toast.style.opacity = Mathf.Clamp01(toastTimer * 2f);
        }

        private static void ShowPower(VisualElement pill, VisualElement fill, double remaining)
        {
            bool active = remaining > 0;
            pill.SetVisible(active);
            if (active) fill.style.width = Length.Percent((float)(remaining / SimConstants.PowerUpDuration) * 100f);
        }
    }
}
