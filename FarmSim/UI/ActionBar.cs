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
}
