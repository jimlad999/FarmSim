using Microsoft.Xna.Framework;
using System.Runtime.Serialization;
using UI;
using UIUtils = UI.Utils;

namespace FarmSim.UI;

[DataContract]
class ActionBar : ButtonGroup
{
    [DataMember]
    public string Padding;
    [IgnoreDataMember]
    private Point? PaddingComputed;

    public override Rectangle PreComputeDestinationCache(Rectangle drawArea, Point offset)
    {
        if (MarginComputed == null)
        {
            MarginComputed = UIUtils.ComputePaddingOrMargin(Margin, drawArea);
        }
        if (PaddingComputed == null)
        {
            PaddingComputed = UIUtils.ComputePaddingOrMargin(Padding, drawArea);
        }
        return UIUtils.PreComputeDestinationCache(
            ref ChildOffsetCache,
            this,
            PaddingComputed.Value,
            drawArea,
            offset,
            positionParentBasedOnChildrenDimensions: true);
    }

    public void CycleNext()
    {
        if (Children.Count == 0 || SelectedButton == null)
        {
            return;
        }
        var nextIndex = Children.IndexOf(SelectedButton);
        ActionButton nextAction;
        do
        {
            if (++nextIndex >= Children.Count)
            {
                nextIndex = 0;
            }
            nextAction = (ActionButton)Children[nextIndex];
        // break out if full cycle
        } while (!(nextAction.IsActive || nextAction == SelectedButton));
        if (nextAction != SelectedButton)
        {
            nextAction.Select();
        }
    }

    public void CyclePrevious()
    {
        if (Children.Count == 0 || SelectedButton == null)
        {
            return;
        }
        var nextIndex = Children.IndexOf(SelectedButton);
        ActionButton nextAction;
        do
        {
            if (--nextIndex < 0)
            {
                nextIndex = Children.Count - 1;
            }
            nextAction = (ActionButton)Children[nextIndex];
            // break out if full cycle
        } while (!(nextAction.IsActive || nextAction == SelectedButton));
        if (nextAction != SelectedButton)
        {
            nextAction.Select();
        }
    }
}
