using System.Collections.Generic;

namespace UI.Data;

public class UISpriteData
{
    public string BaseFolder { get; set; }
    public Dictionary<string, UIElementData> Elements { get; set; }
}
