using System.Runtime.Serialization;
using UI;

namespace FarmSim.UI;

[DataContract]
class ActionIcon : Panel
{
    [IgnoreDataMember]
    public ButtonEventHandler ButtonEventHandler;

    public ActionIcon(string texture, ButtonEventHandler buttonEventHandler)
    {
        Texture = texture;
        HorizontalAlignment = Alignment.Center;
        VerticalAlignment = Alignment.Center;
        ButtonEventHandler = buttonEventHandler;
    }
}
