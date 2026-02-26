using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities.Extensions
{
        public static class ScrollRectExtensions
        {
                public static int Search(this ScrollRect scroll, string objectName)
                {
                        if (!scroll || !scroll.content || string.IsNullOrWhiteSpace(objectName)) throw new ArgumentNullException(nameof(scroll), "ScrollRect or content is null, or objectName is invalid.");

                        // Optimize for performance by caching and grouping in a single pass
                        var items = scroll.content.GetComponentsInChildren<Transform>().Where(item => item != scroll.content).ToArray();
                        if (items.IsNullOrEmpty()) return 0;

                        var search = new List<Transform>();
                        var unSearch = new List<Transform>();

                        foreach (var item in items)
                        {
                                if (string.Equals(item.name, objectName, StringComparison.OrdinalIgnoreCase)) search.Add(item);
                                else unSearch.Add(item);
                        }

                        unSearch.ForEach(item => item.gameObject.SetActive(false));

                        // Return the count of matching items
                        return search.Count;
                }

                public static int ResetSearch(this ScrollRect scroll)
                {
                        if (!scroll || !scroll.content) throw new ArgumentNullException(nameof(scroll), "ScrollRect or content is null.");

                        var items = scroll.content.GetComponentsInChildren<Transform>(true).Where(item => item != scroll.content).ToArray();
                        if (items.IsNullOrEmpty()) return 0;

                        // Iterate through all items and ensure they are set to active.
                        items.ForEach(item => item.gameObject.SetActive(false));

                        // Return the count of re-enabled items
                        return items.Length;
                }


                /// Normalize a distance to be used in verticalNormalizedPosition or horizontalNormalizedPosition.
                /// <param name="axis">Scroll axis, 0 = horizontal, 1 = vertical</param>
                /// <param name="distance">The distance in the scroll rect's view's coordiante space</param>
                /// <returns>The normalized scoll distance</returns>
                public static float NormalizeScrollDistance(this ScrollRect scrollRect, int axis, float distance)
                {
                        // Based on code in ScrollRect's internal SetNormalizedPosition method
                        var viewport = scrollRect.viewport;
                        var viewRect = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();
                        var viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);

                        var content = scrollRect.content;
                        var contentBounds = content != null ? content.TransformBoundsTo(viewRect) : new Bounds();

                        var hiddenLength = contentBounds.size[axis] - viewBounds.size[axis];
                        return distance / hiddenLength;
                }

                /// Scroll the target element to the vertical center of the scroll rect's viewport.
                /// Assumes the target element is part of the scroll rect's contents.
                /// <param name="scrollRect">Scroll rect to scroll</param>
                /// <param name="target">Element of the scroll rect's content to center vertically</param>
                public static void ScrollToCeneter(this ScrollRect scrollRect, RectTransform target)
                {
                        // The scroll rect's view's space is used to calculate scroll position
                        var view = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();

                        // Calcualte the scroll offset in the view's space
                        var viewRect = view.rect;
                        var elementBounds = target.TransformBoundsTo(view);
                        var offset = viewRect.center.y - elementBounds.center.y;

                        // Normalize and apply the calculated offset
                        var scrollPos = scrollRect.verticalNormalizedPosition - scrollRect.NormalizeScrollDistance(1, offset);
                        scrollRect.verticalNormalizedPosition = Mathf.Clamp(scrollPos, 0f, 1f);
                }
        }
}