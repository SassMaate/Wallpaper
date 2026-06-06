using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Sucrose.Portal.Controls
{
    /// <summary>
    /// A UI-virtualizing <see cref="VirtualizingPanel"/> that reproduces the justified
    /// row layout of <c>LibraryStackPanel</c>/<c>StoreStackPanel</c> while recycling
    /// containers and owning its own scrolling via <see cref="IScrollInfo"/>.
    ///
    /// Cards are fixed height (<see cref="ItemHeight"/>) and variable width, clamped to
    /// [<see cref="ItemMinWidth"/>, <see cref="ItemMaxWidth"/>]. Because every row has a
    /// uniform height, the vertical extent is exact (rowCount * rowHeight) and only the
    /// rows intersecting the viewport (plus a small cache) are realized.
    ///
    /// Each row is justified exactly like <c>LibraryStackPanel.DistributeExtraSpace</c>:
    /// the items-per-row is computed from the minimum cell width, the leftover width is
    /// distributed evenly across the row's cards (each capped at <see cref="ItemMaxWidth"/>),
    /// and the row is centered only when it genuinely underflows (a single card wider than
    /// the viewport).
    /// </summary>
    public class JustifiedVirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
    {
        private const int CacheRows = 2;

        private Size _extent;
        private Size _viewport;
        private Point _offset;

        public static readonly DependencyProperty ItemMarginProperty = DependencyProperty.Register(nameof(ItemMargin), typeof(Thickness), typeof(JustifiedVirtualizingWrapPanel), new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty MaxItemsPerRowProperty = DependencyProperty.Register(nameof(MaxItemsPerRow), typeof(int), typeof(JustifiedVirtualizingWrapPanel), new FrameworkPropertyMetadata(int.MaxValue, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty ItemMinWidthProperty = DependencyProperty.Register(nameof(ItemMinWidth), typeof(double), typeof(JustifiedVirtualizingWrapPanel), new FrameworkPropertyMetadata(260d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty ItemMaxWidthProperty = DependencyProperty.Register(nameof(ItemMaxWidth), typeof(double), typeof(JustifiedVirtualizingWrapPanel), new FrameworkPropertyMetadata(400d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(nameof(ItemHeight), typeof(double), typeof(JustifiedVirtualizingWrapPanel), new FrameworkPropertyMetadata(160d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public Thickness ItemMargin { get => (Thickness)GetValue(ItemMarginProperty); set => SetValue(ItemMarginProperty, value); }
        public int MaxItemsPerRow { get => (int)GetValue(MaxItemsPerRowProperty); set => SetValue(MaxItemsPerRowProperty, value); }
        public double ItemMinWidth { get => (double)GetValue(ItemMinWidthProperty); set => SetValue(ItemMinWidthProperty, value); }
        public double ItemMaxWidth { get => (double)GetValue(ItemMaxWidthProperty); set => SetValue(ItemMaxWidthProperty, value); }
        public double ItemHeight { get => (double)GetValue(ItemHeightProperty); set => SetValue(ItemHeightProperty, value); }

        // Each card's base (minimum) footprint, including its margin.
        private double BaseCellWidth => ItemMinWidth + ItemMargin.Left + ItemMargin.Right;

        // Each card's maximum footprint, including its margin. Mirrors LibraryStackPanel,
        // which caps the footprint at MaxWidth (margins are then carved out of it).
        private double MaxCellWidth => ItemMaxWidth + ItemMargin.Left + ItemMargin.Right;

        private double RowHeight => ItemHeight + ItemMargin.Top + ItemMargin.Bottom;

        /// <summary>
        /// Number of items per row. Mirrors LibraryStackPanel: a card fits while
        /// (k+1) * BaseCellWidth &lt;= availableWidth, i.e. floor(width / BaseCellWidth),
        /// clamped to at least 1 and to <see cref="MaxItemsPerRow"/>.
        /// </summary>
        private int ItemsPerRow(double availableWidth)
        {
            double cell = BaseCellWidth;

            int perRow = cell > 0 ? (int)Math.Floor(availableWidth / cell) : 1;

            perRow = Math.Max(1, perRow);

            if (MaxItemsPerRow > 0)
            {
                perRow = Math.Min(perRow, MaxItemsPerRow);
            }

            return perRow;
        }

        private int ItemCount
        {
            get
            {
                ItemsControl owner = ItemsControl.GetItemsOwner(this);
                return owner?.Items.Count ?? 0;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // Ensure the generator is initialized before any GeneratorPosition math.
            _ = InternalChildren;

            double availableWidth = double.IsInfinity(availableSize.Width) ? ItemMaxWidth : availableSize.Width;
            double availableHeight = double.IsInfinity(availableSize.Height) ? RowHeight : availableSize.Height;

            int count = ItemCount;
            int perRow = ItemsPerRow(availableWidth);
            int rows = (count + perRow - 1) / perRow;

            Size extent = new(availableWidth, rows * RowHeight);
            Size viewport = new(availableWidth, availableHeight);

            UpdateScrollInfo(viewport, extent);

            // Determine the visible row range (plus cache) using the (possibly clamped) offset.
            int firstRow = Math.Max(0, (int)Math.Floor(_offset.Y / RowHeight) - CacheRows);
            int lastRow = Math.Min(Math.Max(0, rows - 1), (int)Math.Ceiling((_offset.Y + availableHeight) / RowHeight) + CacheRows);

            int firstItem = firstRow * perRow;
            int lastItem = Math.Min(count - 1, ((lastRow + 1) * perRow) - 1);

            IItemContainerGenerator generator = ItemContainerGenerator;

            if (count > 0 && perRow > 0)
            {
                GeneratorPosition startPos = generator.GeneratorPositionFromIndex(firstItem);

                // If the requested item is not already realized (Offset != 0) the new child
                // must be inserted AFTER the container at startPos.Index.
                int childIndex = startPos.Offset == 0 ? startPos.Index : startPos.Index + 1;

                using (generator.StartAt(startPos, GeneratorDirection.Forward, true))
                {
                    for (int itemIndex = firstItem; itemIndex <= lastItem; itemIndex++, childIndex++)
                    {
                        UIElement child = generator.GenerateNext(out bool newlyRealized) as UIElement;

                        if (child == null)
                        {
                            break;
                        }

                        if (newlyRealized)
                        {
                            if (childIndex >= InternalChildren.Count)
                            {
                                AddInternalChild(child);
                            }
                            else
                            {
                                InsertInternalChild(childIndex, child);
                            }

                            generator.PrepareItemContainer(child);
                        }

                        int row = itemIndex / perRow;
                        int itemsInThisRow = Math.Min(perRow, count - (row * perRow));

                        // Measure at the exact width the child will be arranged at, so the
                        // child's internal layout matches its final placement.
                        double innerWidth = Math.Max(0, RowCellWidth(itemsInThisRow, availableWidth) - ItemMargin.Left - ItemMargin.Right);

                        child.Measure(new Size(innerWidth, ItemHeight));
                    }
                }
            }

            CleanupContainers(firstItem, lastItem, generator);

            // The panel never reports more than the viewport: it scrolls its own content,
            // so its desired size is the available size (or the extent height when finite).
            return new Size(availableWidth, double.IsInfinity(availableSize.Height) ? extent.Height : availableHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            int count = ItemCount;
            int perRow = ItemsPerRow(finalSize.Width);

            IItemContainerGenerator generator = ItemContainerGenerator;

            for (int i = 0; i < InternalChildren.Count; i++)
            {
                UIElement child = InternalChildren[i];

                int itemIndex = generator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));

                if (itemIndex < 0 || itemIndex >= count)
                {
                    continue;
                }

                int row = itemIndex / perRow;
                int col = itemIndex % perRow;
                int itemsInThisRow = Math.Min(perRow, count - (row * perRow));

                ArrangeChildInRow(child, col, itemsInThisRow, finalSize.Width, row);
            }

            return finalSize;
        }

        /// <summary>
        /// Computes the justified footprint (including margin) of every cell in a row,
        /// reproducing <c>LibraryStackPanel.DistributeExtraSpace</c> under the uniform-width
        /// assumption (each card's natural footprint == <see cref="BaseCellWidth"/>):
        /// distribute the leftover width evenly, then cap each cell at <see cref="MaxCellWidth"/>.
        /// </summary>
        private double RowCellWidth(int itemsInRow, double rowWidth)
        {
            if (itemsInRow <= 0)
            {
                return BaseCellWidth;
            }

            double rowBase = BaseCellWidth * itemsInRow;
            double extraPerItem = (rowWidth - rowBase) / itemsInRow;

            // Mirror DistributeExtraSpace: finalChildWidth = max(MinWidth, min(MaxWidth, base + extra)).
            // Note the cap uses MaxCellWidth/MinWidth+margins to stay in "footprint" space.
            double cell = BaseCellWidth + extraPerItem;

            if (cell > MaxCellWidth)
            {
                cell = MaxCellWidth;
            }

            if (cell < BaseCellWidth)
            {
                cell = BaseCellWidth;
            }

            return cell;
        }

        private void ArrangeChildInRow(UIElement child, int col, int itemsInRow, double rowWidth, int row)
        {
            double cellWidth = RowCellWidth(itemsInRow, rowWidth);
            double rowBase = BaseCellWidth * itemsInRow;

            // Centering matches DistributeExtraSpace: only when the row genuinely underflows
            // (extraSpace <= 0, i.e. the base footprint already exceeds the viewport width).
            // Otherwise the row is left-aligned and any post-cap gap stays on the right.
            double startX = (rowWidth - rowBase) > 0 ? 0 : (rowWidth - (cellWidth * itemsInRow)) / 2;

            double x = startX + (col * cellWidth);
            double y = (row * RowHeight) - _offset.Y;

            double innerWidth = Math.Max(0, cellWidth - ItemMargin.Left - ItemMargin.Right);

            child.Arrange(new Rect(x + ItemMargin.Left, y + ItemMargin.Top, innerWidth, ItemHeight));
        }

        /// <summary>
        /// Recycles/removes any realized container whose item index falls outside the
        /// currently-visible window. <see cref="IItemContainerGenerator.Remove"/> moves the
        /// container to the recycle queue when the owning ItemsControl uses
        /// <c>VirtualizationMode=Recycling</c>, and discards it otherwise.
        /// </summary>
        private void CleanupContainers(int firstItem, int lastItem, IItemContainerGenerator generator)
        {
            for (int i = InternalChildren.Count - 1; i >= 0; i--)
            {
                GeneratorPosition pos = new(i, 0);

                int itemIndex = generator.IndexFromGeneratorPosition(pos);

                if (itemIndex < firstItem || itemIndex > lastItem)
                {
                    generator.Remove(pos, 1);
                    RemoveInternalChildRange(i, 1);
                }
            }
        }

        /// <summary>
        /// Keeps the realized containers in sync when the underlying items collection
        /// changes. Required for any custom <see cref="VirtualizingPanel"/>: without it,
        /// removing/replacing/moving an item corrupts the child/item mapping.
        /// </summary>
        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    if (args.ItemUICount > 0)
                    {
                        RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                    }
                    break;
            }

            // Reset is handled implicitly: the generator clears its realized containers and a
            // fresh measure re-realizes the visible window. Add/Insert are picked up on the
            // next measure pass. In all cases, force a relayout.
            InvalidateMeasure();
        }

        private void UpdateScrollInfo(Size viewport, Size extent)
        {
            bool changed = false;

            if (extent != _extent)
            {
                _extent = extent;
                changed = true;
            }

            if (viewport != _viewport)
            {
                _viewport = viewport;
                changed = true;
            }

            double maxOffset = Math.Max(0, _extent.Height - _viewport.Height);

            if (_offset.Y > maxOffset)
            {
                _offset.Y = maxOffset;
                changed = true;
            }

            // Horizontal scrolling is unsupported: pin the horizontal offset to 0.
            if (_offset.X != 0)
            {
                _offset.X = 0;
                changed = true;
            }

            if (changed)
            {
                ScrollOwner?.InvalidateScrollInfo();
            }
        }

        #region IScrollInfo

        public ScrollViewer ScrollOwner { get; set; }

        public bool CanHorizontallyScroll { get; set; }

        public bool CanVerticallyScroll { get; set; }

        public double ExtentWidth => _extent.Width;
        public double ExtentHeight => _extent.Height;
        public double ViewportWidth => _viewport.Width;
        public double ViewportHeight => _viewport.Height;
        public double HorizontalOffset => 0;
        public double VerticalOffset => _offset.Y;

        public void LineUp() => SetVerticalOffset(VerticalOffset - (RowHeight / 3));
        public void LineDown() => SetVerticalOffset(VerticalOffset + (RowHeight / 3));
        public void WheelUp() => SetVerticalOffset(VerticalOffset - RowHeight);
        public void WheelDown() => SetVerticalOffset(VerticalOffset + RowHeight);
        public void MouseWheelUp() => SetVerticalOffset(VerticalOffset - RowHeight);
        public void MouseWheelDown() => SetVerticalOffset(VerticalOffset + RowHeight);
        public void PageUp() => SetVerticalOffset(VerticalOffset - ViewportHeight);
        public void PageDown() => SetVerticalOffset(VerticalOffset + ViewportHeight);

        // Horizontal scrolling is intentionally a no-op (vertical-only wrap).
        public void LineLeft() { }
        public void LineRight() { }
        public void WheelLeft() { }
        public void WheelRight() { }
        public void MouseWheelLeft() { }
        public void MouseWheelRight() { }
        public void PageLeft() { }
        public void PageRight() { }
        public void SetHorizontalOffset(double offset) { }

        public void SetVerticalOffset(double offset)
        {
            double maxOffset = Math.Max(0, _extent.Height - _viewport.Height);

            offset = Math.Max(0, Math.Min(offset, maxOffset));

            if (Math.Abs(offset - _offset.Y) > 0.001)
            {
                _offset.Y = offset;

                ScrollOwner?.InvalidateScrollInfo();

                // Re-measure so the newly-visible rows are realized and old rows recycled.
                InvalidateMeasure();
            }
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            if (visual is not UIElement child)
            {
                return rectangle;
            }

            int idx = InternalChildren.IndexOf(child);

            if (idx < 0)
            {
                return rectangle;
            }

            int itemIndex = ItemContainerGenerator.IndexFromGeneratorPosition(new GeneratorPosition(idx, 0));

            if (itemIndex < 0)
            {
                return rectangle;
            }

            int perRow = ItemsPerRow(_viewport.Width);
            int row = itemIndex / perRow;

            double top = row * RowHeight;
            double bottom = top + RowHeight;

            if (top < _offset.Y)
            {
                SetVerticalOffset(top);
            }
            else if (bottom > _offset.Y + _viewport.Height)
            {
                SetVerticalOffset(bottom - _viewport.Height);
            }

            // Return the rectangle in the (now-updated) coordinate space of the viewport.
            return new Rect(rectangle.X, top - _offset.Y, rectangle.Width, RowHeight);
        }

        #endregion
    }
}
