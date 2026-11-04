using System.Collections.Generic;
using Game.Settings;
using HarmonyLib;
using Helpers;
using Model;
using UI;
using UI.Tags;
using UI.Tooltips;
using UnityEngine;
using UnityEngine.UI;

namespace PipTip
{
    // One callout for the hovered pip, built from the Tab tag prefab so it has the same
    // colored title bar and background, drawn on the game's tooltip canvas.
    internal sealed class PipTooltip : MonoBehaviour
    {
        private const float RefreshSeconds = 0.5f;

        private static readonly AccessTools.FieldRef<TagController, TagCallout> TagPrefab =
            AccessTools.FieldRefAccess<TagController, TagCallout>("tagCalloutPrefab");
        private static readonly AccessTools.FieldRef<UITooltipController, Canvas> TooltipCanvas =
            AccessTools.FieldRefAccess<UITooltipController, Canvas>("canvas");
        private static readonly AccessTools.FieldRef<UITooltipController, Callout> TooltipPrefab =
            AccessTools.FieldRefAccess<UITooltipController, Callout>("calloutPrefab");

        private static PipTooltip _shared;

        private Canvas _canvas;
        private RectTransform _canvasRect;
        private Callout _callout;
        private CanvasGroup _group;
        private readonly List<Image> _colorImages = new List<Image>();
        private PipHover _hover;
        private float _nextRefresh;

        public static void Show(PipHover hover)
        {
            PipTooltip tooltip = GetOrCreate();
            if (tooltip == null)
                return;
            tooltip._hover = hover;
            tooltip._nextRefresh = 0f;
            tooltip.Refresh();
        }

        public static void Hide(PipHover hover)
        {
            if (_shared != null && _shared._hover == hover)
                _shared.HideNow();
        }

        public static void Shutdown()
        {
            if (_shared != null)
                Destroy(_shared._callout != null ? _shared._callout.gameObject : _shared.gameObject);
            _shared = null;
        }

        private static PipTooltip GetOrCreate()
        {
            if (_shared != null)
                return _shared;
            UITooltipController controller = UITooltipController.Shared;
            Canvas canvas = controller != null ? TooltipCanvas(controller) : null;
            if (canvas == null)
                return null;

            // Prefer the Tab tag callout; fall back to the game's plain tooltip callout.
            TagController tags = TagController.Shared;
            TagCallout tagPrefab = tags != null ? TagPrefab(tags) : null;
            Callout prefab = tagPrefab != null ? tagPrefab.callout : TooltipPrefab(controller);
            if (prefab == null)
                return null;

            Callout callout = Instantiate(prefab, canvas.transform);
            callout.gameObject.name = "PipTip Callout";
            callout.transform.localRotation = Quaternion.identity;
            callout.transform.localScale = Vector3.one;
            PipTooltip tooltip = callout.gameObject.AddComponent<PipTooltip>();
            tooltip._canvas = canvas;
            tooltip._canvasRect = canvas.GetComponent<RectTransform>();
            tooltip._callout = callout;
            tooltip._group = callout.GetComponent<CanvasGroup>() ?? callout.gameObject.AddComponent<CanvasGroup>();
            tooltip._group.blocksRaycasts = false;
            tooltip._group.interactable = false;
            foreach (Graphic graphic in callout.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;
            if (tagPrefab != null)
                tooltip.FindColorImages(tagPrefab);
            ModEntry.Log?.Info($"PipTip: callout from {(tagPrefab != null ? "tag" : "tooltip")} prefab, {tooltip._colorImages.Count} color images.");
            tooltip.HideNow();
            _shared = tooltip;
            return tooltip;
        }

        // The tag's colorImages are references into the prefab; find the same objects in our copy.
        private void FindColorImages(TagCallout tagPrefab)
        {
            Transform prefabRoot = tagPrefab.callout.transform;
            foreach (Image image in tagPrefab.colorImages)
            {
                if (image == null || !image.transform.IsChildOf(prefabRoot))
                    continue;
                Transform copy = image.transform == prefabRoot ? _callout.transform : _callout.transform.Find(PathFrom(prefabRoot, image.transform));
                Image copyImage = copy != null ? copy.GetComponent<Image>() : null;
                if (copyImage != null)
                    _colorImages.Add(copyImage);
            }
        }

        private static string PathFrom(Transform root, Transform target)
        {
            string path = target.name;
            for (Transform t = target.parent; t != null && t != root; t = t.parent)
                path = t.name + "/" + path;
            return path;
        }

        private void HideNow()
        {
            _hover = null;
            if (_group != null)
                _group.alpha = 0f;
        }

        private void Update()
        {
            if (_hover == null)
                return;
            if (!_hover.isActiveAndEnabled || UI.ContextMenu.ContextMenu.IsShown)
            {
                HideNow();
                return;
            }
            if (Time.unscaledTime >= _nextRefresh)
                Refresh();
            Place();
        }

        private void Refresh()
        {
            Car car = _hover != null ? _hover.Car : null;
            if (car == null)
            {
                HideNow();
                return;
            }
            _nextRefresh = Time.unscaledTime + RefreshSeconds;
            _callout.Title = PipInfo.Title(car);
            _callout.Text = PipInfo.Text(car);
            _callout.Layout();
            Color color = PipInfo.TagColor(car);
            foreach (Image image in _colorImages)
                image.color = color;
            _callout.transform.SetAsLastSibling();
            _group.alpha = 1f;
            Place();
        }

        // Above and right of the mouse (the strip is at the bottom of the screen), kept on screen.
        private void Place()
        {
            RectTransform rect = _callout.RectTransform != null ? _callout.RectTransform : (RectTransform)_callout.transform;
            rect.pivot = new Vector2(0f, 0f);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            Vector2 mouse = _canvas.ScreenToCanvasPosition(Input.mousePosition).XY();
            Vector2 size = rect.rect.size;
            Vector2 canvasSize = _canvasRect.sizeDelta;
            Vector2 position = mouse + new Vector2(12f, 16f);
            if (position.x + size.x > canvasSize.x)
                position.x = canvasSize.x - size.x;
            if (position.y + size.y > canvasSize.y)
                position.y = canvasSize.y - size.y;
            float scale = CanvasSettingsApplicator.EffectiveCanvasScale;
            rect.anchoredPosition = new Vector2(Mathf.Round(position.x * scale) / scale, Mathf.Round(position.y * scale) / scale);
        }
    }
}
