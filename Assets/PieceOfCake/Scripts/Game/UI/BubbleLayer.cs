using PieceOfCake.Core;
using PieceOfCake.Game.World;
using UnityEngine;
using UnityEngine.UIElements;

namespace PieceOfCake.Game.UI
{
    /// <summary>Speech bubbles anchored to world positions: the chef, the nearest sign and the customer.</summary>
    public sealed class BubbleLayer
    {
        private const int T = SimConstants.CellSize;
        private const float EdgeMargin = 8f;

        private readonly VisualElement layer;
        private readonly Label chef;
        private readonly Label sign;
        private readonly Label customer;

        public BubbleLayer(VisualElement root)
        {
            layer = root.Required<VisualElement>("bubbles");
            chef = Create("chef");
            sign = Create("sign");
            customer = Create("customer");
            customer.text = Texts.CustomerWaitingBubble;
        }

        public void Hide()
        {
            chef.SetVisible(false);
            sign.SetVisible(false);
            customer.SetVisible(false);
        }

        public void Render(StageSimulation sim, StageView view, Camera camera)
        {
            var p = sim.Player;
            var map = view.Map;

            bool talking = p.IsTalking && !p.IsDead;
            if (talking)
            {
                chef.text = Texts.Speech(p.Message, sim.Stats.Eaten);
                Place(chef, map.Point(p.CenterX, p.Y) + new Vector3(0f, p.IsCarryingOrder ? 1.1f : 0.6f, 0f), camera);
            }
            chef.SetVisible(talking);

            var nearSign = talking ? null : NearestSign(sim);
            if (nearSign != null)
            {
                sign.text = nearSign.Text;
                Place(sign, map.CellBottom(nearSign.Cell) + new Vector3(0f, 1.25f, 0f), camera);
            }
            sign.SetVisible(nearSign != null);

            bool waiting = sim.Phase == GamePhase.Playing && !p.IsCarryingOrder
                           && Mathf.Abs((float)(p.X - sim.Layout.Goal.X * T)) < T * 6;
            if (waiting) Place(customer, view.Props.CustomerHead, camera);
            customer.SetVisible(waiting);
        }

        /// <summary>The sign the chef is standing next to, if any (same rule as the prototype).</summary>
        private static SignInfo NearestSign(StageSimulation sim)
        {
            var p = sim.Player;
            foreach (var s in sim.Layout.Signs)
            {
                double cx = s.Cell.X * T + T / 2.0;
                if (System.Math.Abs(p.CenterX - cx) < T * 2.2 && System.Math.Abs(p.Bottom - (s.Cell.Y + 1) * T) < T * 1.6)
                    return s;
            }

            return null;
        }

        private void Place(Label bubble, Vector3 world, Camera camera)
        {
            var panel = layer.panel;
            if (panel == null) return;
            var anchor = RuntimePanelUtils.CameraTransformWorldToPanel(panel, world, camera);
            float width = float.IsNaN(bubble.resolvedStyle.width) ? 0f : bubble.resolvedStyle.width;
            float height = float.IsNaN(bubble.resolvedStyle.height) ? 0f : bubble.resolvedStyle.height;
            var bounds = layer.layout;
            float left = Mathf.Clamp(anchor.x - width / 2f, EdgeMargin, Mathf.Max(EdgeMargin, bounds.width - width - EdgeMargin));
            float top = Mathf.Max(EdgeMargin, anchor.y - height - 8f);
            bubble.style.left = left;
            bubble.style.top = top;
        }

        private Label Create(string kind)
        {
            var label = new Label { pickingMode = PickingMode.Ignore };
            label.AddToClassList("bubble");
            label.AddToClassList(kind);
            label.SetVisible(false);
            layer.Add(label);
            return label;
        }
    }
}
