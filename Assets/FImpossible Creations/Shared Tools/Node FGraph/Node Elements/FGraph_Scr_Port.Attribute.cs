using System;

namespace FIMSpace.Graph
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class PortAttribute : Attribute
    {
        public EPortPinType PinType = EPortPinType.Input;
        public EPortNameDisplay NameDisplay = EPortNameDisplay.Default;
        public EPortValueDisplay ValueDisplay = EPortValueDisplay.Default;
        public string CustomName = "";
        public System.Type[] AdditionalAllows = null;
        public int LimitConnectionsCount = 0;
        public object InitialValue = null;

        public PortAttribute(EPortPinType type, int connectionsCountLimit = 0, params Type[] additionalConnectionsAlllow) { PinType = type; LimitConnectionsCount = connectionsCountLimit; AdditionalAllows = additionalConnectionsAlllow; }
        public PortAttribute(EPortPinType type, object initialValue, int connectionsCountLimit = 0,  params Type[] additionalConnectionsAlllow) { PinType = type; LimitConnectionsCount = connectionsCountLimit; AdditionalAllows = additionalConnectionsAlllow; InitialValue = initialValue; }
        public PortAttribute(EPortPinType type, EPortValueDisplay valDispl, int connectionsCountLimit = 0, params Type[] additionalConnectionsAlllow) : this(type, EPortNameDisplay.Default, valDispl, connectionsCountLimit, additionalConnectionsAlllow) { }
        public PortAttribute(EPortPinType type, EPortValueDisplay valDispl, string customName, int connectionsCountLimit = 0, params Type[] additionalConnectionsAlllow) : this(type, EPortNameDisplay.Default, valDispl, connectionsCountLimit, additionalConnectionsAlllow) { CustomName = customName; }
        public PortAttribute(EPortPinType type, EPortNameDisplay display, int connectionsCountLimit = 0, params Type[] additionalConnectionsAlllow) : this(type, connectionsCountLimit, additionalConnectionsAlllow) { NameDisplay = display; }
        public PortAttribute(EPortPinType type, EPortNameDisplay display, string customName, int connectionsCountLimit = 0, params Type[] additionalConnectionsAlllow) : this(type, display, connectionsCountLimit, additionalConnectionsAlllow) { CustomName = customName; }
        public PortAttribute(EPortPinType type, EPortNameDisplay display, EPortValueDisplay valDispl, int connectionsCountLimit = 0, params Type[] additionalConnectionsAlllow) : this(type, display, connectionsCountLimit, additionalConnectionsAlllow) { ValueDisplay = valDispl; }
        public PortAttribute(EPortPinType type, bool displayOnlyPort, int connectionsCountLimit = 0, params Type[] additionalConnectionsAlllow) : this(type, connectionsCountLimit, additionalConnectionsAlllow) { ValueDisplay = displayOnlyPort ? EPortValueDisplay.HideValue : EPortValueDisplay.Default; NameDisplay = displayOnlyPort ? EPortNameDisplay.HideName : EPortNameDisplay.Default; }
        public PortAttribute(EPortPinType type, EPortNameDisplay display, EPortValueDisplay valDispl, string customName, int connectionsCountLimit = 0, params Type[] additionalConnectionsAlllow) : this(type, display, connectionsCountLimit, additionalConnectionsAlllow) { CustomName = customName; ValueDisplay = valDispl; }
        public PortAttribute(EPortPinType type, string customName, int connectionsCountLimit = 0, params Type[] additionalConnectionsAlllow) : this(type, connectionsCountLimit, additionalConnectionsAlllow) { CustomName = customName; }
    }
}
